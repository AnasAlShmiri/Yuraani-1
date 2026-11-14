namespace Yuraani.Domain.Common;

internal static class StringExtensions
{
    /// <summary>يقتطع القيمة إلى الحد الأقصى المسموح به دون رمي استثناء.</summary>
    public static string? TruncateTo(this string? value, int maxLength) =>
        value is null || value.Length <= maxLength ? value : value[..maxLength];
}
