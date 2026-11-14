namespace Yuraani.Application.Common.Abstractions;

/// <summary>وحدة العمل - تجميع حفظ التغييرات في سياق واحد.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
