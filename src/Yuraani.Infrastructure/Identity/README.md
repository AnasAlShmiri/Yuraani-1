# Identity Infrastructure

تحتوي هذه الطبقة على تطبيق خدمات الهوية والأمان، مع إبقاء تفاصيل التشفير ومزودي الرسائل والتخزين خارج Domain.

## إعداد مفتاح رموز التحقق

يعتمد `VerificationCodeHasher` على مفتاح HMAC سري لا يقل عن 32 بايت، ولا يجوز وضع المفتاح الحقيقي داخل ملفات Git.

في Windows PowerShell، أنشئ مفتاحاً عشوائياً واحفظه في User Secrets للمشروع:

```powershell
dotnet user-secrets init --project src/Yuraani.API
$key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets set "Verification:HashKey" "$key" --project src/Yuraani.API
```

يجب الاحتفاظ بالمفتاح نفسه بين مرات التشغيل؛ تغييره يمنع التحقق من الرموز التي أُنشئت بالمفتاح السابق. لا تستخدم مفتاح JWT نفسه لهذا الغرض.

مرسل رموز التحقق الحالي مخصص للتطوير فقط. اربط مزودي SMS والبريد الفعليين قبل النشر.
