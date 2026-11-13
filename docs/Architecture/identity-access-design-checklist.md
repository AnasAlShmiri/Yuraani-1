# Identity & Access — Design Checklist

Before implementing account persistence and authentication flows, settle these decisions:

1. Is UserAccount the authentication identity and Patient/CareProvider/Administrator separate one-to-one profiles?
2. Are Doctor and Nurse specializations of CareProvider, represented by separate profile tables or a discriminator plus subtype data?
3. Are roles coarse-grained access groups while permissions represent specific actions?
4. How is Family Authorization represented, scoped, granted, revoked, and audited?
5. Can one account hold multiple roles or profiles?
6. Which login identifier is authoritative: phone, email, or either?
7. Which account states block authentication and which require approval?

No production migrations should be generated until these decisions are reconciled with the project use-case and class diagrams.
