# تقرير التدقيق المعماري — Yuraani (يرعاني)

- **التاريخ:** 2026-11-14
- **النطاق:** جميع ملفات `src/` و`tests/` وملفات المشاريع (`.csproj`) وسكربتات SQL. لم يُعدَّل أي ملف مصدري.
- **طريقة الفحص:** قراءة ثابتة للكود (Static Review). لم يُبنَ المشروع ولم تُشغَّل الاختبارات، لذا البنود الموسومة بـ«يحتاج تحقق» هي مخاطر مستنتجة من القراءة وليست أعطالاً مُثبتة بالتشغيل.
- **حالة المشروع:** وحدتان منفَّذتان فقط (Identity & Access، Professional Approval). بقية الوحدات ومشروع `Yuraani.Web` هياكل فارغة.

## الخلاصة التنفيذية

الهيكل العام سليم: طبقة Domain معزولة فعلاً، و Application تعتمد على واجهات فقط، والكيانات الأساسية غنية بالسلوك وليست Anemic. المشاكل الحقيقية ليست في «شكل» الطبقات بل في أربعة مواضع:

1. **معالجة الأخطاء** مبنية على أنواع استثناءات عامة من .NET، فتُحوَّل أخطاء الإطار الداخلية إلى 409/400 وتُسرَّب رسائلها للعميل، بينما تعارضات قاعدة البيانات تخرج 500.
2. **الذرّية والتزامن:** لا يوجد Unit of Work صريح، وعملية `ExecuteUpdate` تُنفَّذ خارج المعاملة، وعدادات الحماية (محاولات الدخول ورمز التحقق) بلا Concurrency Token.
3. **تخطٍّ واحد صريح للطبقات:** `DbContext` مستخدم مباشرة داخل طبقة API، وكيان Domain يُعاد كما هو في استجابة HTTP.
4. **إدارة مخطط قاعدة البيانات:** سكربتات SQL يدوية غير مكتملة، والاختبارات تستخدم `EnsureCreated`، فالمخطط المُختبَر ليس المخطط المنشور.

| المحور | التقييم |
|---|---|
| اتجاه التبعيات | جيد، مع مخالفة واحدة صريحة في API |
| نموذج المجال | جيد في الكيانات الأساسية، ضعيف في حدود الـ Aggregates والـ Value Objects |
| حالات الاستخدام | منظمة (CQRS + Validation Pipeline)، لكن فيها تكرار وقواعد أعمال متسربة |
| الأخطاء والمعاملات | أضعف نقطة في المشروع |

---

## 1. هيكل الطبقات واتجاه التبعيات

### 1.1 عزل طبقة Domain — سليم

- `src/Yuraani.Domain/Yuraani.Domain.csproj:1` لا يحتوي أي `PackageReference` أو `ProjectReference`.
- الاستيرادات كلها من مكتبة .NET الأساسية فقط: `System.Text.RegularExpressions` في `src/Yuraani.Domain/Identity/UserAccount.cs:1` و`System.Security.Cryptography` في `src/Yuraani.Domain/Identity/VerificationChallenge.cs:1`. لا يوجد EF Core ولا Attributes خاصة بالـ ORM.

تسرّبان بسيطان لمفاهيم التخزين:

| # | الملف:السطر | المخالفة |
|---|---|---|
| D-1 | `src/Yuraani.Domain/Identity/RefreshSession.cs:14` | الخاصية `RowVersion` هي Concurrency Token خاص بـ SQL Server/EF ولا معنى لها في المجال. |
| D-2 | `src/Yuraani.Domain/Identity/UserRole.cs:2` و`src/Yuraani.Domain/Identity/RolePermission.cs:2` | جداول ربط (Join Tables) مُنمذجة كأصناف Domain مستقلة بدل أن تكون جزءاً من Aggregate الدور/الحساب. |

### 1.2 اعتماد Application على الواجهات — سليم مع ملاحظات

- `src/Yuraani.Application/Yuraani.Application.csproj:3-9` يشير إلى Domain فقط، مع MediatR و FluentValidation و DI Abstractions. لا إشارة إلى Infrastructure أو EF Core.
- كل الوصول للبيانات والخدمات عبر واجهات في `Features/*/Abstractions`.

ملاحظات:

| # | الملف:السطر | المخالفة |
|---|---|---|
| A-1 | `src/Yuraani.Application/Features/Identity/Auth/Commands/Login/LoginHandler.cs:57-58`، `.../RefreshSession/RefreshSessionHandler.cs:33-34`، `.../Logout/LogoutHandler.cs:11` | توليد Refresh Token وتجزئته بـ SHA-256 مكتوب مباشرة داخل الـ Handlers ومكرر ثلاث مرات. هذا تفصيل تقني يجب أن يكون خلف واجهة (مثل `IRefreshTokenService`)، كما فُعل مع `IVerificationCodeHasher`. |
| A-2 | `.../RegisterAccount/RegisterAccountHandler.cs:46` و`.../ResendVerification/ResendVerificationHandler.cs:39` | توليد رمز التحقق مكرر في موضعين. |
| A-3 | `src/Yuraani.Application/Features/Identity/Auth/Abstractions/IAuthRepository.cs:5-18` | واجهة واحدة بـ 14 عملية تجمع الحسابات والجلسات ورموز التحقق وسجل الدخول والأدوار — مخالفة Interface Segregation، ومستودع «لكل ميزة» بدل «لكل Aggregate». |
| A-4 | `src/Yuraani.Application/Features/ProfessionalApproval/Abstractions/IProfessionalApprovalRepository.cs:14` | `AddAuditLogAsync` داخل مستودع الاعتماد المهني؛ التدقيق اهتمام عرضي (Cross-cutting) لا يخص هذا المستودع. السطر `:16` (`GetProviderApprovalsAsync`) غير مستخدم في أي Handler. |

### 1.3 تخطي الطبقات

| # | الملف:السطر | المخالفة |
|---|---|---|
| L-1 | `src/Yuraani.API/Security/PermissionAuthorization.cs:9` و`:17-22` | **المخالفة الصريحة الوحيدة لقاعدة التبعية:** `PermissionAuthorizationHandler` يحقن `YuraaniDbContext` ويكتب استعلام LINQ على الجداول مباشرة من طبقة API، دون واجهة في Application. |
| L-2 | `src/Yuraani.Application/Features/ProfessionalApproval/Queries/GetPendingApprovals/GetPendingApprovalsQuery.cs:4`، `GetPendingApprovalsHandler.cs:6-8`، `src/Yuraani.API/Controllers/ProfessionalApprovalController.cs:26-27` | الاستعلام يعيد كيان `ProfessionalApproval` نفسه فيُسلسَل مباشرة إلى JSON. أي تغيير في الكيان يكسر عقد الـ API، وكل خاصية جديدة تُكشف تلقائياً. |
| L-3 | `src/Yuraani.API/Controllers/AuthController.cs:19`، `:25`، `:30`، `:35`، `:37` | الـ Commands تُستخدم كنموذج Request مباشرة (Model Binding). عقد HTTP وعقد حالة الاستخدام صارا شيئاً واحداً. |
| L-4 | `src/Yuraani.API/Controllers/ProfessionalApprovalController.cs:8` و`:44` | نموذج الطلب `SubmitApprovalRequest` يستخدم enum من Domain (`ProfessionalType`) مباشرة. |
| L-5 | `src/Yuraani.Infrastructure/Identity/HttpLoginContext.cs:4-7` مع `src/Yuraani.Infrastructure/Yuraani.Infrastructure.csproj:6` | قراءة `HttpContext` مكانها طبقة العرض (API) لا Infrastructure؛ وهي سبب إضافة `FrameworkReference` لـ ASP.NET Core كاملاً إلى Infrastructure. |
| L-6 | `src/Yuraani.Infrastructure/Yuraani.Infrastructure.csproj:14` و`src/Yuraani.Infrastructure/Identity/SecurePasswordService.cs:9-15` | حزمة `Microsoft.AspNetCore.Identity.EntityFrameworkCore` كاملة مضافة لاستخدام `PasswordHasher` فقط، مع تمرير `null!` كمستخدم. |
| L-7 | `tests/Yuraani.UnitTests/Yuraani.UnitTests.csproj:5` و`:12` | اختبارات الوحدة تشير إلى مشروع API و`Mvc.Testing` دون حاجة. |

نقاط إيجابية: الـ Controllers رفيعة وتمر كلها عبر `ISender`، ولا يوجد استدعاء Repository من Controller. مشروع `Yuraani.Web` (`src/Yuraani.Web/Yuraani.Web.csproj:1-2`) لا يشير إلى أي طبقة داخلية، وهو مطابق للقرار المعماري.

---

## 2. تصميم الـ Entities ونموذج المجال

### 2.1 هل النموذج Anemic؟ — لا، في الكيانات الأساسية

كل الكيانات لها `private set` ومُنشئ خاص و Factory Method، فلا يمكن تعديل حالتها من الخارج مباشرة. السلوك موجود فعلاً:

- `UserAccount`: التفعيل بعد توثيق القناتين (`UserAccount.cs:48-58`)، والقفل بعد خمس محاولات (`:63-72`).
- `VerificationChallenge`: الصلاحية وعدّ المحاولات والاستهلاك (`VerificationChallenge.cs:33-67`).
- `ProfessionalApproval.Decide` و`Withdraw` (`ProfessionalApproval.cs:31-48`)، و`ProfessionalLicense.Review/Revoke/MarkExpired` (`ProfessionalLicense.cs:37-65`).

الكيانات الضعيفة:

| # | الملف:السطر | المخالفة |
|---|---|---|
| E-1 | `src/Yuraani.Domain/ProfessionalApproval/CareProvider.cs:11-13` | `BasePrice` و`ServiceArea` و`YearsExperience` لها `private set` ولا توجد أي دالة تضبطها — خصائص ميتة لا يمكن ملؤها إلا من قاعدة البيانات. |
| E-2 | `src/Yuraani.Domain/Identity/UserAccount.cs:18` مع `AccountStatus.cs:2` | الحالتان `Suspended` و`Closed` لا توجد أي دالة توصل إليهما (لا `Suspend` ولا `Close` ولا تغيير كلمة المرور). |

### 2.2 ثغرات تسمح بإنشاء حالة غير صالحة

| # | الملف:السطر | المخالفة |
|---|---|---|
| E-3 | `src/Yuraani.Domain/Identity/LoginHistory.cs:13` | `Record` بلا أي تحقق. أعمدة الجدول محدودة (`LoginHistoryConfiguration.cs:7`: ‏`UserAgent` 512، `IpAddress` 64، `FailureReason` 160). ترويسة `User-Agent` أطول من 512 حرفاً يُتوقع أن تُفشل الحفظ فيرجع تسجيل الدخول 500 (يحتاج تحقق بالتشغيل). |
| E-4 | `src/Yuraani.Domain/Identity/RefreshSession.cs:15-21` مع `RefreshSessionConfiguration.cs:14-15` | نفس المشكلة: `ip` و`agent` بلا حد طول في الكيان. |
| E-5 | `src/Yuraani.Domain/Identity/VerificationChallenge.cs:21-30` | `Create` لا يتحقق من `accountId` الفارغ ولا من طول/صيغة `hash` ولا من قيمة `channel`. |
| E-6 | `src/Yuraani.Domain/Identity/UserAccount.cs:29-31` | تحقق ضعيف: لا صيغة للبريد ولا حد لطول الاسم (العمود 160)، ورسالة خطأ واحدة عامة. الكيان يعتمد فعلياً على FluentValidation ليبقى صالحاً. |
| E-7 | `src/Yuraani.Domain/ProfessionalApproval/ProfessionalLicense.cs:45-46` مع `ProfessionalApprovalConfigurations.cs:82` | اعتماد ترخيص جديد يضبط `IsCurrent = true` دون إلغاء الترخيص الحالي السابق، بينما الفهرس الفريد المفلتر يمنع وجود اثنين. القاعدة مفروضة في قاعدة البيانات فقط وليست في المجال. |

### 2.3 حدود الـ Aggregates

| # | الملف:السطر | المخالفة |
|---|---|---|
| G-1 | `.../ReviewProfessionalApproval/ReviewProfessionalApprovalHandler.cs:15-17` | قاعدة «كل قرار له سجل مراجعة» ينفذها الـ Handler يدوياً (استدعاء `Decide` ثم `Record`). `ProfessionalApprovalReview` يجب أن يكون كياناً ابناً داخل Aggregate الاعتماد يُنشأ من داخل `Decide`. |
| G-2 | `src/Yuraani.Domain/ProfessionalApproval/ProfessionalApprovalReview.cs:18` مع `ApprovalReviewAction.cs:5` و`:8` | السجل يشترط `adminId` غير فارغ، بينما الإجراءان `Submitted` و`Withdrawn` يصدران عن مقدم الرعاية لا المسؤول — النموذج يناقض نفسه. |
| G-3 | `src/Yuraani.Domain/ProfessionalApproval/ProfessionalApproval.cs:31-41` | الموافقة لا تشترط وجود ترخيص معتمد. `ProfessionalLicense` غير مرتبط بأي حالة استخدام، فيمكن اعتماد طبيب بلا ترخيص. |
| G-4 | `.../SubmitProfessionalApproval/SubmitProfessionalApprovalHandler.cs:14-17` مع `ProfessionalApproval.cs:23` | قاعدة «ثلاث محاولات كحد أقصى» مقسومة بين الـ Handler والكيان، وقاعدة «لا طلب مفتوح أو معتمد» في الـ Handler فقط. |
| G-5 | `SubmitProfessionalApprovalHandler.cs:21-22` مقابل `ReviewProfessionalApprovalHandler.cs:15-18` | التقديم يكتب `AuditLog` والقرار لا يكتبه — عدم اتساق في التدقيق. |
| G-6 | `src/Yuraani.Domain/Common/IDomainEvent.cs:3` و`src/Yuraani.Domain/Common/Entity.cs:3-8` | `IDomainEvent` معرَّف وغير مستخدم، و`Entity` بلا مساواة بالهوية وبلا قائمة أحداث. لا يوجد تمييز `AggregateRoot`. |

### 2.4 Primitive Obsession وتكرار القواعد

| # | الملف:السطر | المخالفة |
|---|---|---|
| V-1 | `src/Yuraani.Domain/Identity/UserAccount.cs:82-92` | `NormalizePhone` و`IsValidInternationalPhone` دوال `public static` على الكيان، تستدعيها Application في `RegisterAccountHandler.cs:17-21` و`RegisterAccountValidator.cs:11` و`LoginHandler.cs:17`. يجب أن تكون Value Object ‏(`PhoneNumber`)، ومثلها `Email`. |
| V-2 | `UserAccount.cs:37-38` | `PhoneNumber` و`NormalizedPhoneNumber` يأخذان القيمة نفسها دائماً — خاصية مكررة. |
| V-3 | `UserAccount.cs:66-68`، `RefreshSession.cs:20`، `VerificationChallenge.cs:29`، `ResendVerificationHandler.cs:29` و`:34` | أرقام سياسات مثبتة في الكود (5 محاولات، 15 دقيقة، 30 يوماً، 10 دقائق، 60 ثانية، 3 رموز في الساعة). |

### 2.5 الخلط بين الكيانات والجداول والـ DTOs

- الكيانات تُخزَّن مباشرة عبر `IEntityTypeConfiguration` خارج Domain — أسلوب مقبول ولا يحتاج نموذج تخزين منفصل.
- الخلط مع الـ DTO موجود في L-2 (كيان في استجابة HTTP).
- `ProfessionalApprovalConfigurations.cs:36-37` و`:69-71`: قيود CHECK تكتب أسماء قيم الـ enum نصاً؛ إعادة تسمية أي قيمة في Domain تكسر قاعدة البيانات بصمت.

---

## 3. طبقة Application وحالات الاستخدام

### 3.1 توزيع منطق الأعمال

الـ Controllers خالية من منطق الأعمال. التسرب موجود في اتجاهين آخرين:

| # | الملف:السطر | المخالفة |
|---|---|---|
| B-1 | `.../Login/LoginHandler.cs:39` | قاعدة «الحساب نشط والقناتان موثقتان» مكتوبة في الـ Handler. مكانها دالة على الكيان (مثل `EnsureCanSignIn`). الشرط نفسه مكرر جزئياً في `RefreshSessionHandler.cs:22`. |
| B-2 | `.../ResendVerification/ResendVerificationHandler.cs:29-35` | سياسة الحد من إعادة الإرسال (60 ثانية، 3 في الساعة) قاعدة مجال مكتوبة إجرائياً في الـ Handler. |
| B-3 | `src/Yuraani.Infrastructure/Identity/AuthRepository.cs:24-26` | **منطق أعمال داخل Repository:** إبطال الرموز يتم بـ `ExecuteUpdateAsync` ويتجاوز `VerificationChallenge.Revoke` (`VerificationChallenge.cs:64-67`). |
| B-4 | `.../RegisterAccount/RegisterAccountHandler.cs:20-21` | تحقق صيغة الهاتف مكرر في ثلاثة مواضع: الـ Validator والـ Handler والكيان. |
| B-5 | `src/Yuraani.Infrastructure/ProfessionalApproval/ProfessionalApprovalRepository.cs:25-26` | `GetPendingAsync` بلا ترقيم صفحات (Pagination) ويحمّل كيانات كاملة لاستعلام قراءة. |

### 3.2 معالجة الأخطاء

الأسلوب الحالي: رمي استثناءات .NET العامة ثم تحويلها حسب النوع في `src/Yuraani.API/Middleware/ExceptionHandlingMiddleware.cs:14-22`. لا يوجد تسلسل استثناءات خاص بالمجال ولا Result Pattern.

| # | الملف:السطر | المخالفة |
|---|---|---|
| X-1 | `ExceptionHandlingMiddleware.cs:20` و`:26` | كل `InvalidOperationException` يتحول إلى 409 **مع إرسال `ex.Message` للعميل**. هذا النوع ترميه مكتبات .NET و EF Core نفسها (مثل `SingleOrDefaultAsync` عند وجود أكثر من صف)، وترميه رسائل الإعدادات الداخلية في `JwtAuthTokenService.cs:17` و`DevelopmentVerificationCodeSender.cs:11`. النتيجة: أعطال داخلية تظهر كـ 409 وتتسرب تفاصيلها. |
| X-2 | `ExceptionHandlingMiddleware.cs:17` | الأمر نفسه مع `ArgumentException` ← 400 (يشمل `ArgumentNullException` القادمة من الإطار). |
| X-3 | `ExceptionHandlingMiddleware.cs:14-22` | لا معالجة لـ `DbUpdateException` ولا `DbUpdateConcurrencyException`، فتخرج 500. يتأثر بذلك: سباق التسجيل المتزامن (`RegisterAccountHandler.cs:23-27` مع الفهارس الفريدة)، سباق تقديم الاعتماد (`SubmitProfessionalApprovalHandler.cs:14-19` مع الفهرس في `ProfessionalApprovalConfigurations.cs:45`)، وتعارض `RowVersion` عند تحديث الجلسة مرتين في اللحظة نفسها (`RefreshSessionHandler.cs:26-29`). |
| X-4 | `ReviewProfessionalApprovalHandler.cs:11` و`SubmitProfessionalApprovalHandler.cs:13` مع `ExceptionHandlingMiddleware.cs:18` | مستخدم مصادَق عليه لكن بلا ملف مسؤول/مقدم رعاية يحصل على 401 بدل 403. |
| X-5 | `ExceptionHandlingMiddleware.cs:35` | أخطاء FluentValidation تُختزل في نص واحد (`ex.Message`)؛ قائمة الأخطاء لكل حقل تضيع، فلا يستطيع Flutter عرضها بجانب الحقول. |
| X-6 | `ExceptionHandlingMiddleware.cs:12` | كل خطأ 4xx متوقع يُسجَّل بمستوى `Error` مع Stack Trace كامل — ضجيج في السجلات. |
| X-7 | `RegisterAccountHandler.cs:24` و`:27`، `ResendVerificationHandler.cs:17`، `LoginHandler.cs:30` | الرسائل تكشف وجود الحساب من عدمه (Account Enumeration) على نقاط نهاية بلا مصادقة. |

### 3.3 التحقق من المدخلات

- **جيد:** `src/Yuraani.Application/Behaviors/ValidationBehavior.cs:10-18` يشغّل كل الـ Validators قبل أي Handler، ولكل Command مدقق.
- **ناقص:** القواعد موزعة على ثلاث طبقات (Validator، كيان، قيود قاعدة البيانات) بلا مصدر واحد للحقيقة — انظر E-3 إلى E-6 و B-4. المدققات تغطي ما يأتي من المستخدم فقط؛ القيم التي يولدها النظام (IP، User-Agent) لا يتحقق منها أحد.

### 3.4 المعاملات والتزامن

| # | الملف:السطر | المخالفة |
|---|---|---|
| T-1 | `IAuthRepository.cs:18` و`IProfessionalApprovalRepository.cs:17` | لا يوجد `IUnitOfWork`. كل مستودع يكشف `SaveChangesAsync` خاصاً به، والذرّية تعتمد ضمنياً على أن المستودعين يتشاركان `DbContext` واحداً. |
| T-2 | `ResendVerificationHandler.cs:37-44` مع `AuthRepository.cs:24-26` | `ExecuteUpdateAsync` يُنفَّذ فوراً خارج معاملة `SaveChanges`. إذا فشل حفظ الرمز الجديد تبقى الرموز القديمة مُبطلة ولا رمز صالح لدى المستخدم. |
| T-3 | `RegisterAccountHandler.cs:37-41` | الإرسال بعد الحفظ بلا Outbox (التعليق في السطر 39 يعترف بذلك). فشل المزود = حساب محفوظ بلا رموز واستجابة خطأ. وبما أن `DevelopmentVerificationCodeSender.cs:11` يرمي استثناء خارج بيئة التطوير، فكل تسجيل خارج Development ينشئ الحساب ثم يفشل. |
| T-4 | `UserAccount.cs:63-72` و`UserAccountConfiguration.cs:9` | عداد محاولات الدخول الفاشلة بلا Concurrency Token؛ الطلبات المتوازية تكتب فوق بعضها (Lost Update) فيمكن تجاوز حد الخمس محاولات. |
| T-5 | `VerificationChallenge.cs:51-56` و`VerificationChallengeConfiguration.cs:9-19` | المشكلة نفسها في `AttemptCount` لرمز من 6 أرقام — أخطر لأن مساحة التخمين صغيرة. |
| T-6 | `LoginHandler.cs:50-51` | الجلسة تُحفظ قبل إنشاء Access Token؛ فشل الإنشاء يترك جلسة يتيمة. الترتيب الصحيح موجود في `RefreshSessionHandler.cs:28-29`. |
| T-7 | `RefreshSessionHandler.cs:19-20` | إعادة استخدام Refresh Token مُبطل ترجع 401 فقط، دون إبطال سلسلة الجلسات (لا يوجد Reuse Detection) رغم وجود `ReplacedBySessionId`. |

### 3.5 إدارة مخطط قاعدة البيانات

| # | الملف:السطر | المخالفة |
|---|---|---|
| S-1 | `src/Yuraani.Infrastructure/Persistence/Migrations/` | ثلاثة سكربتات SQL يدوية فقط. لا يوجد سكربت ينشئ `UserAccounts` أو `VerificationChallenges` أو `LoginHistory` أو جداول الاعتماد المهني الخمسة، ولا توجد EF Migrations. |
| S-2 | `tests/Yuraani.IntegrationTests/ProfessionalApprovalAuthorizationTests.cs:75` | الاختبارات تنشئ المخطط بـ `EnsureCreated` من نموذج EF، فهي لا تختبر السكربتات التي ستُنفَّذ فعلياً. |
| S-3 | `AccessControlConfigurations.cs:30` و`:55`، `RefreshSessionConfiguration.cs:11-13`، `ProfessionalApprovalConfigurations.cs:14`، `:24`، `:42-43`، `:57`، `:80` | خصائص `DateTimeOffset` مربوطة بأعمدة `datetime2`، بينما `RevokedAt` في `20261112_AddVerificationChallengeProtection.sql:16` من نوع `DATETIMEOFFSET`، وتواريخ `UserAccount` بلا تحديد نوع. عدم اتساق يفقد الإزاحة الزمنية، وسلوك القراءة يحتاج تحقق باختبار تكامل. |
| S-4 | `src/Yuraani.API/Program.cs:35-41` و`JwtAuthTokenService.cs:13-17` | إعدادات JWT تُقرأ وتُتحقق في موضعين بدل Options Pattern. و`VerificationCodeHasher.cs:14-16` يكتشف غياب مفتاحه عند أول طلب لا عند الإقلاع. |

### 3.6 تغطية الاختبارات

اختبارات الوحدة تغطي كيانات Domain فقط (`tests/Yuraani.UnitTests/`). لا يوجد أي اختبار لأي Handler، واختبارات التكامل اثنان فقط للتفويض. هذا يجعل إعادة الهيكلة في القسم 4 خطرة دون البدء بالخطوة 1.

---

## 4. خطة الإصلاح وإعادة الهيكلة

### 4.1 ترتيب العيوب حسب الخطورة

**حرجة (Critical)** — تسبب سلوكاً خاطئاً أو ثغرة أو فشل نشر:

| الرمز | العيب | المراجع |
|---|---|---|
| C1 | تحويل الاستثناءات العامة إلى 409/400 مع تسريب رسائلها، وتعارضات قاعدة البيانات تخرج 500 | X-1، X-2، X-3 |
| C2 | عدادات الحماية بلا Concurrency Token (تجاوز حد المحاولات) | T-4، T-5 |
| C3 | عمليات غير ذرّية: `ExecuteUpdate` خارج المعاملة، والتسجيل بلا Outbox ويفشل دائماً خارج Development | T-2، T-3 |
| C4 | مخطط قاعدة البيانات غير قابل للنشر من المستودع، والاختبارات لا تختبره | S-1، S-2، S-3 |

**متوسطة (High)** — مخالفات معمارية تتفاقم مع كل وحدة جديدة:

| الرمز | العيب | المراجع |
|---|---|---|
| H1 | `DbContext` داخل طبقة API | L-1 |
| H2 | كيان Domain في استجابة HTTP، والـ Commands كعقد HTTP | L-2، L-3، L-4 |
| H3 | Factory Methods بلا تحقق (طول User-Agent وغيره) | E-3، E-4، E-5، E-6 |
| H4 | حدود Aggregates خاطئة في الاعتماد المهني، واعتماد بلا ترخيص | G-1 إلى G-5، E-7 |
| H5 | لا `IUnitOfWork`، ومستودعات سمينة لكل ميزة | T-1، A-3، A-4 |
| H6 | 401 بدل 403، وضياع تفاصيل أخطاء التحقق، وكشف وجود الحسابات | X-4، X-5، X-7 |
| H7 | قواعد أعمال في الـ Handlers والـ Repository | B-1، B-2، B-3 |
| H8 | لا اختبارات للـ Handlers | 3.6 |

**تحسينية (Medium):**

| الرمز | العيب | المراجع |
|---|---|---|
| M1 | Value Objects للهاتف والبريد، وإزالة التكرار | V-1، V-2، B-4 |
| M2 | تكرار كود التشفير وتوليد الرموز في الـ Handlers | A-1، A-2 |
| M3 | أرقام السياسات مثبتة في الكود | V-3 |
| M4 | `RowVersion` في Domain، و`IDomainEvent` غير مستخدم، و`Entity` بلا مساواة | D-1، G-6 |
| M5 | `HttpLoginContext` وحزمة Identity في Infrastructure | L-5، L-6 |
| M6 | Options Pattern للإعدادات، وترقيم الصفحات، وإشارات مشروع الاختبارات، وخصائص ميتة | S-4، B-5، L-7، E-1، E-2 |
| M7 | لا Reuse Detection للـ Refresh Token، وترتيب الحفظ في Login، ومستوى السجلات | T-7، T-6، X-6 |

### 4.2 الخطة العملية

الترتيب مقصود: كل مرحلة صغيرة وقابلة للدمج وحدها، ولا تغيّر عقد الـ API إلا حيث ذُكر ذلك صراحة.

**المرحلة 0 — شبكة أمان (قبل أي تعديل)**

1. اكتب اختبارات وحدة للـ Handlers الثمانية الحالية بمستودعات وهمية (Fakes) تثبّت السلوك الحالي، واختبارات تكامل للمسارات الأساسية (تسجيل، توثيق، دخول، تحديث جلسة، تقديم وقرار اعتماد). احذف إشارة API من `Yuraani.UnitTests.csproj:12`.

**المرحلة 1 — الحرجة**

2. **(C1)** أنشئ في Domain تسلسل استثناءات: `DomainException` ومنه `ConflictException` و`NotFoundException` و`ForbiddenException` و`BusinessRuleException`، كلٌّ برمز خطأ ثابت (`code`). استبدل الرمي في الـ Handlers والكيانات تدريجياً.
3. **(C1)** عدّل الـ Middleware ليحوّل هذه الأنواع فقط، ويحوّل `DbUpdateConcurrencyException` وانتهاك الفهرس الفريد إلى 409، ويُرجع 500 برسالة عامة لكل ما عداها. أبقِ التحويل القديم مؤقتاً إلى أن تنتهي الخطوة 2 ثم احذفه.
4. **(C2)** أضف `rowversion` لجدولي `UserAccounts` و`VerificationChallenges` كـ Shadow Property في إعدادات EF (لا في الكيان)، وأعد المحاولة مرة واحدة عند التعارض في `LoginHandler` و`VerifyContactHandler`.
5. **(C3)** استبدل `ExecuteUpdateAsync` في `AuthRepository.cs:24-26` بتحميل الرموز النشطة واستدعاء `Revoke` عليها، لتُحفظ مع الرمز الجديد في `SaveChanges` واحدة.
6. **(C3)** أضف جدول Outbox لرسائل التحقق يُكتب في المعاملة نفسها مع الحساب، ومعالجاً خلفياً (`BackgroundService`) يرسل ويعيد المحاولة. اجعل اختيار المرسل (تطوير/إنتاج) من الإعدادات، وافشل عند الإقلاع إن لم يُضبط مزود في الإنتاج.
7. **(C4)** اعتمد EF Core Migrations كمصدر وحيد للمخطط: أنشئ Migration أولية من النموذج الحالي، قارنها بقاعدة البيانات الموجودة، وانقل السكربتات اليدوية إلى أرشيف. وحّد أنواع التواريخ (الموصى به `datetimeoffset` لكل `DateTimeOffset`). غيّر `EnsureCreated` إلى `Migrate` في اختبارات التكامل.

**المرحلة 2 — المتوسطة**

8. **(H1)** عرّف `IPermissionChecker` في Application ونفّذه في Infrastructure، واجعل `PermissionAuthorizationHandler` يعتمد عليه. بعدها لا يبقى في API أي `using` لـ `Infrastructure.Persistence` خارج `Program.cs`.
9. **(H2)** أضف `PendingApprovalDto` واجعل `GetPendingApprovalsQuery` يعيده عبر Projection مع ترقيم صفحات. شكل JSON يبقى كما هو إن طابقت الأسماء، فلا يتأثر العميل.
10. **(H2)** أضف نماذج Request في API لنقاط `AuthController` وحوّلها إلى Commands، مع الإبقاء على أسماء الحقول نفسها.
11. **(H3)** أضف التحقق والاقتطاع (Truncate) في `LoginHistory.Record` و`RefreshSession.Create` و`VerificationChallenge.Create`، وقوِّ `UserAccount.Register`.
12. **(H5)** أضف `IUnitOfWork` في Application ينفذه `YuraaniDbContext`، واحذف `SaveChangesAsync` من المستودعين. ثم قسّم `IAuthRepository` إلى `IUserAccountRepository` و`IRefreshSessionRepository` و`IVerificationChallengeRepository` و`ILoginHistoryWriter`، وانقل `AddAuditLogAsync` إلى `IAuditLogWriter`. التنفيذ في Infrastructure يمكن أن يبقى صنفاً واحداً ينفذ عدة واجهات في البداية.
13. **(H4)** اجعل `ProfessionalApprovalReview` كياناً ابناً داخل `ProfessionalApproval`: ‏`Submit` و`Decide` و`Withdraw` تضيف السجل داخلياً، واجعل `AdminId` اختيارياً لإجراءات مقدم الرعاية. أضف شرط الترخيص المعتمد قبل الموافقة، وحالة استخدام لتقديم الترخيص، ومعالجة `IsCurrent` للترخيص السابق.
14. **(H6، H7)** انقل قاعدة السماح بالدخول إلى `UserAccount`، وسياسة إعادة الإرسال إلى خدمة مجال أو إلى `VerificationChallenge`. استخدم `ForbiddenException` لحالتي X-4، وأرجع أخطاء التحقق كـ `ValidationProblemDetails` بقائمة لكل حقل، ووحّد رسائل نقاط النهاية المجهولة حتى لا تكشف وجود الحساب. **هذه الخطوة تغيّر أكواد وأشكال استجابات الأخطاء، فنسّقها مع فريق Flutter.**

**المرحلة 3 — التحسينية**

15. **(M1)** أنشئ Value Objects ‏`PhoneNumber` و`Email` مع EF Value Converters (بلا تغيير في الأعمدة)، واحذف الدوال الساكنة من `UserAccount` والخاصية المكررة.
16. **(M2)** استخرج `IRefreshTokenService` (توليد + تجزئة) و`IVerificationCodeGenerator`.
17. **(M3، M6)** انقل أرقام السياسات إلى Options (`AuthPolicyOptions`) تُمرَّر للكيانات كمعاملات، واستخدم `IOptions<JwtOptions>` مع `ValidateOnStart`.
18. **(M4)** حوّل `RefreshSession.RowVersion` إلى Shadow Property، وأضف `AggregateRoot` بقائمة أحداث، وانشر الأحداث من `SaveChanges`؛ استخدمها أولاً لكتابة `AuditLog` بشكل موحد (يحل G-5).
19. **(M5)** انقل `HttpLoginContext` إلى API، واستبدل حزمة Identity بـ `Microsoft.Extensions.Identity.Core`، ثم احذف `FrameworkReference` من Infrastructure إن لم يبقَ له مستخدم.
20. **(M7)** أضف Reuse Detection للـ Refresh Token، وصحّح ترتيب الحفظ في `LoginHandler`، وسجّل أخطاء 4xx بمستوى `Warning`.
21. أضف اختبارات معمارية (NetArchTest أو ما يماثلها) تفرض: Domain بلا تبعيات، Application لا يشير إلى Infrastructure، و API لا يستخدم `DbContext` — حتى لا تعود المخالفات مع الوحدات السبع القادمة.

### 4.3 توصية التوقيت

نفّذ المرحلتين 0 و1 والخطوات 8 و12 و13 **قبل** البدء بوحدة Care Services. الأنماط الحالية (مستودع لكل ميزة، استثناءات عامة، `SaveChanges` على المستودع) ستُنسخ إلى كل وحدة جديدة، وتكلفة تصحيحها تتضاعف مع كل وحدة.
