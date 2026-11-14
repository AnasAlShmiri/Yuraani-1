namespace Yuraani.Domain.Common;

public abstract class DomainException : Exception
{
    public string Code { get; }
    protected DomainException(string code, string message) : base(message) => Code = code;
}

public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string message, string code = "validation_error") : base(code, message) { }
}

public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message, string code = "business_rule_violation") : base(code, message) { }
}

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message, string code = "not_found") : base(code, message) { }
}

public sealed class ConflictException : DomainException
{
    public ConflictException(string message, string code = "conflict") : base(code, message) { }
}

public sealed class ForbiddenException : DomainException
{
    public ForbiddenException(string message, string code = "forbidden") : base(code, message) { }
}
