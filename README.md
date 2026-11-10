# Yuraani — يرعاني

منصة لخدمات الرعاية الطبية المنزلية.

## التقنية
- .NET 10 / ASP.NET Core Web API
- ASP.NET Core MVC
- Flutter
- SQL Server / Entity Framework Core
- Clean Architecture + CQRS

## المعمارية
- `src/Yuraani.Domain`: الكيانات وقواعد المجال، دون اعتماد على طبقات خارجية.
- `src/Yuraani.Application`: حالات الاستخدام وواجهات الخدمات وCQRS.
- `src/Yuraani.Infrastructure`: تنفيذ الوصول إلى البيانات والخدمات الخارجية.
- `src/Yuraani.API`: واجهة HTTP التي يستهلكها تطبيق Flutter وواجهة MVC.
- `src/Yuraani.Web`: واجهة ASP.NET Core MVC، وتتواصل مع API عبر HTTP.
- `mobile/yuraani_mobile`: تطبيق Flutter.
- `tests`: اختبارات الوحدات والتكامل.

## وحدات التنفيذ
1. Identity & Access
2. Professional Approval
3. Care Services & Requests
4. Appointments
5. Care Execution
6. Medical Records
7. Treatment & Follow-up
8. Ratings & Administration
9. AI Integration

## نقطة البداية
نبدأ بوحدة **Identity & Access** بعد تثبيت الهيكل الأساسي، ثم نضيف الوحدات تدريجيًا.
لا يتصل Flutter أو MVC بقاعدة البيانات مباشرة؛ جميع العمليات تمر عبر Web API.
