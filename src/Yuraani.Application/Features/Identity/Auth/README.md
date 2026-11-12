# Identity / Auth

Implemented CQRS use cases:
- RegisterAccount: creates a PendingVerification account and independent phone/email verification challenges.
- VerifyContact: consumes one valid challenge for one contact. The account becomes Active only after both phone and email are verified.
- Login: accepts normalized phone or email, verifies the password, enforces the five-failure/15-minute lockout, records LoginHistory, and issues a short-lived JWT.

Registration does not create a Patient, CareProvider, Doctor, Nurse, or Administrator profile. Profile onboarding is a separate feature and an account may later own multiple profiles.

The verification sender is development-only; configure production SMS and email delivery before deployment. Configure JWT signing settings using secrets/environment variables.
