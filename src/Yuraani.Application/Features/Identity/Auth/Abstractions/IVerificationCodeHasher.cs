namespace Yuraani.Application.Features.Identity.Auth.Abstractions;

// مسؤول عن إنشاء بصمة آمنة لرمز التحقق دون تخزين الرمز نفسه.
public interface IVerificationCodeHasher
{
    string Hash(string code);
}
