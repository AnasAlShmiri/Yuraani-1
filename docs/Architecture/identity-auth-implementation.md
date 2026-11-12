# Identity Auth — Initial Implementation

## Implemented use cases
- Register an account with full name, phone, email, and password.
- Create the account in PendingVerification state.
- Issue independent one-time six-digit verification challenges for phone and email. Only SHA-256 hashes are persisted; challenges expire after 10 minutes and are consumed once.
- Verify each contact independently. The account becomes Active only after both contacts are verified.
- Sign in with either normalized phone number or email.
- Enforce five consecutive failed password attempts followed by a 15-minute lockout.
- Issue a 15-minute JWT access token after successful authentication.

## Security and integration boundaries
- Passwords are hashed with ASP.NET Core Identity's PasswordHasher.
- JWT signing settings must be supplied using configuration keys Jwt:Secret (at least 32 UTF-8 bytes), Jwt:Issuer, and Jwt:Audience. Use environment variables/secrets; do not commit signing secrets.
- The current verification sender is development-only and writes codes to the development application log. It deliberately throws outside Development. Configure real SMS and email providers before production.
- The access token currently has no persisted session/revocation check. Session listing, per-session revocation, global revocation after password changes, and refresh-token rotation remain a separate required security slice.
- Role/permission assignment, administrator TOTP enforcement, LoginHistory, and AuditLog are not implemented in this first Auth slice and must be completed before production use.
- No EF Core migration was generated. Generate and review it after package restore/build is available and the schema has been reviewed.

## API routes
- POST /api/auth/register
- POST /api/auth/verify-contact
- POST /api/auth/login
