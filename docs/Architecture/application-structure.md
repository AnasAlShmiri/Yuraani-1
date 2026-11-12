# Yuraani Application and CQRS Structure

Yuraani uses a feature-oriented CQRS organization within its existing Modular Monolith and Clean Architecture solution. This adapts the organization style of the Rawaa reference project to Yuraani's own business modules; it does not copy Rawaa's modules.

## Existing baseline

The solution already contains Domain, Application, Infrastructure, API, Web, and UnitTests projects. Application already references MediatR 12.4.1 and FluentValidation 12.0.0. Its DependencyInjection extension registers MediatR handlers, validators, and the ValidationBehavior pipeline.

## Application layout

```text
Yuraani.Application/
  Behaviors/
  Features/
    Identity/
    ProfessionalProfiles/
    ProfessionalApproval/
    CareServices/
    Appointments/
    CareExecution/
    MedicalRecords/
    Treatment/
    Administration/
  Interfaces/
  Mappings/
```

Organize use cases by feature/capability. A typical use case keeps its request, handler, and validator together. Commands change state; Queries read state. Add only subfolders actually needed; do not create empty placeholder classes or duplicate DTOs.

## Dependency direction

- Domain has no dependency on other solution layers.
- Application depends on Domain and owns abstractions required by use cases.
- Infrastructure implements persistence and external-service abstractions.
- API dispatches requests to Application and stays thin.
- Web and Flutter communicate with API over HTTP; neither accesses the database directly.

## CQRS conventions

- One Command represents a state-changing use case and is handled by its handler.
- One Query represents a read use case and is handled by its handler.
- Handlers depend on Application abstractions, not concrete Infrastructure classes.
- FluentValidation validators run through the existing validation behavior.
- Keep database, email, cryptography, HTTP, and provider-specific details outside Domain.
- Do not create a repository for every entity automatically; add abstractions only where they provide a meaningful boundary.

## Infrastructure and API

EF Core DbContext, configurations, migrations, repositories, seeders, and external-service implementations belong in Infrastructure. Controllers bind HTTP input, dispatch MediatR requests, and map results to HTTP responses. Middleware handles cross-cutting HTTP concerns, not business workflows.

## Implementation sequence

1. Establish folder conventions.
2. Review existing MediatR and validation registrations (already present in baseline).
3. Implement one complete vertical slice at a time, starting with Identity only after its domain review is accepted.
4. Add Domain and Application tests.
5. Add EF Core mappings and migrations after the domain model is stable.

This document describes intended organization; it does not claim all features are implemented.
