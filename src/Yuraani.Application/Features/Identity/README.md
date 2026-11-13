# Identity & Access — Application Module

This is the first implementation module.

Planned use cases are organized by CQRS:

- Commands: registration, login, logout/token lifecycle, profile updates, role/permission administration.
- Queries: current account/profile, account lookup, role and permission reads.
- Validators: request-level input validation through FluentValidation.
- Handlers: orchestrate Domain rules and Application abstractions; persistence is implemented in Infrastructure.

Before adding Identity entities and migrations, confirm the account/profile relationship, provider subtype strategy (Doctor/Nurse), role model, and family authorization rules against the approved diagrams.
