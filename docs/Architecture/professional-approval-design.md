# Professional Approval — Domain and Persistence Design

This implementation follows the uploaded Use Case, Class, and ERD diagrams.

## Diagram-derived model

- A CareProvider may submit many ProfessionalApproval requests.
- Each request belongs to one professional type (Doctor or Nurse) and has an attempt number from 1 to 3.
- A unique constraint on (ProviderId, ProfessionalType, AttemptNumber) preserves the three-attempt limit independently for each profession.
- Administrator is the reviewer identity for decisions and review history.
- ProfessionalApprovalReview is append-only history; decisions do not overwrite earlier review events.
- ProfessionalLicense belongs to a CareProvider and a professional type. Its validity is evaluated independently per type.
- Expiry of a Doctor license must not affect Nurse eligibility, and vice versa.

## Relationship boundaries

CareProvider and Administrator are profile records linked to UserAccount. CareProvider is unique per account. Doctor and Nurse remain specializations/professional types, not duplicate CareProvider records.

## Current implementation scope

The current commit establishes Domain entities and EF Core mappings only. It does not yet expose endpoints or claim production authorization. API commands, queries, validators, file-upload storage, admin authorization, scheduled expiry processing, and integration tests are subsequent vertical slices.

## Important operational rules

- Rejection requires a reason.
- Only pending approval requests can be decided or withdrawn.
- Only pending licenses can be reviewed.
- Only approved licenses can be revoked.
- Only one current license per provider and professional type is allowed by a filtered unique index.
- License document paths are metadata only; secure upload/download authorization must be implemented in the application/API layer.
- The three-attempt invariant is protected by both domain validation and a database unique constraint. A concurrent submission must still handle unique-key conflicts in the application layer.
