# Yuraani Architecture Decisions

## Approved baseline

- Clean Architecture: Domain, Application, Infrastructure, Presentation.
- CQRS with MediatR request/handler dispatch.
- FluentValidation registered through a MediatR pipeline behavior.
- Modular Monolith: capabilities are modules in one deployable backend.
- SQL Server with Entity Framework Core 10.
- ASP.NET Core Web API is the only backend interface used by Flutter and MVC.
- Flutter is independent from the .NET solution.

## Dependency rules

1. Domain has no dependency on other layers or infrastructure frameworks.
2. Application depends on Domain and owns use cases and abstractions.
3. Infrastructure implements Application abstractions and persistence.
4. API is the composition root for Application and Infrastructure.
5. MVC calls API over HTTP; it does not reference Infrastructure or DbContext.
6. Flutter calls API over HTTP and never connects directly to SQL Server.

## Identity & Access modeling guardrail

Finalize the account/profile relationship, provider specializations (Doctor/Nurse),
role and permission model, and family authorization against the approved use-case
and class diagrams before creating persistence entities or migrations.
