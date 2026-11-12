# Identity Auth — Initial Implementation

## Implemented use cases
- Register an account with full name, international phone, email, and password.
- Create the account in PendingVerification state.
- Issue independent one-time six-digit verification challenges for phone and email.
- Store HMAC-SHA256 hashes using the secret configuration key `Verification:HashKey`.
- Expire challenges after 10 minutes and invalidate them after five failed attempts.
- Verify each contact independently. The account becomes Active only after both contacts are verified.
- Resend a verification code using `POST /api/auth/resend-verification`, with a 60-second cooldown and a maximum of three codes per contact in one hour.
- Sign in with either normalized phone number or email.
- Enforce five consecutive failed password attempts followed by a 15-minute lockout.
- Issue a 15-minute JWT access token after successful authentication.

## Security and integration boundaries
- Passwords are hashed with ASP.NET Core Identity's PasswordHasher.
- Configure `Verification:HashKey` with at least 32 random bytes. Do not commit the secret.
- JWT signing settings must be supplied using configuration keys Jwt:Secret (at least 32 UTF-8 bytes), Jwt:Issuer, and Jwt:Audience. Use environment variables or User Secrets.
- The current verification sender is development-only and writes codes to the development application log. It deliberately throws outside Development. Configure real SMS and email providers before production.
- Auth endpoints use an in-memory IP rate limiter (10 requests per minute). A distributed store is required when scaling to multiple API instances.
- The access token currently has no persisted session/revocation check. Session listing, per-session revocation, global revocation after password changes, and refresh-token rotation remain a separate required security slice.
- Role/permission assignment, administrator TOTP enforcement, LoginHistory, and AuditLog must be reviewed and completed before production use.
- Apply the SQL schema update in `docs/Architecture/auth-security-configuration.md` before running against an existing database.
