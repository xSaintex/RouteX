# Data Encryption (brief)

This document describes what data in the application is encrypted, which libraries or mechanisms are used, and how to confirm encrypted values in the database.

Passwords stored by the application are not kept in plaintext; they are hashed by ASP.NET Core Identity before being persisted. The application creates `IdentityUser` accounts via `UserManager.CreateAsync(...)` and the resulting `PasswordHash` (a salted PBKDF2 hash produced by Identity's `IPasswordHasher<TUser>` implementation) is saved into the custom `Users` table. Because only the hash is stored, the original password cannot be recovered from the database, and authentication is performed by comparing hashes via Identity APIs rather than by reusing plaintext values.

Transport-level encryption to the database is enabled via the connection string flag `Encrypt=True`, which requests encrypted TLS connections to the SQL Server. This protects data in transit between the application and the database. The repository does not currently implement application-level at-rest encryption (for example, column encryption or Transparent Data Encryption is not configured by the application itself); if you require at-rest protection for sensitive PII or financial values, consider using database-managed TDE, Always Encrypted / column-level encryption, or encrypting values before storing them with a strong authenticated encryption algorithm such as AES-GCM with key material stored in a managed secret store.

The codebase references Data Protection packages (Microsoft.AspNetCore.DataProtection and System.Security.Cryptography.ProtectedData are available in the build), but there is no explicit `IDataProtector` usage or key ring persistence configuration in the current code. If you need to protect small secrets or cookie/session keys at the application level, use `IDataProtection` and persist keys to a secure store (file system protected by OS ACLs, Azure Blob Storage, or Key Vault) and rotate keys periodically.

To verify encrypted/hashed data in the database you can query the `Users` table for the `Password` column; the values in that column should be non-human-readable hashed strings (Identity hash format). Example SQL to run against the configured database:

```sql
SELECT TOP 10 UserId, Email, Password FROM Users ORDER BY UserId DESC;
```

Run the query in your database tool (SQL Server Management Studio, Azure Data Studio, or your cloud provider console) and take a screenshot of the returned rows showing the `Password` values (they will be long, base64-like hash strings). Note: do not share password hash values in public channels.

Provide screenshots of encrypted data in the database.

If you would like application-level encryption for specific columns, recommended options are:
- Use SQL Server Always Encrypted for client-side column encryption with keys stored in Key Vault.
- Use AES-GCM with keys retrieved from a managed secret store and an authenticated envelope-encryption approach for larger blobs.
- Enable database Transparent Data Encryption (TDE) for at-rest disk encryption (configured at the DB server level, not in application code).

---
## Screenshot locations (code lines to capture)
- `RouteX/Models/User.cs` — `Password` property declaration: [RouteX/Models/User.cs](RouteX/Models/User.cs#L12)
- `RouteX/Controllers/UsersController.cs` — where backfill assigns hashed password to model: [RouteX/Controllers/UsersController.cs](RouteX/Controllers/UsersController.cs#L123)
- `RouteX/Controllers/UsersController.cs` — where the hashed password is stored when creating a new user (`Password = passwordHash`): [RouteX/Controllers/UsersController.cs](RouteX/Controllers/UsersController.cs#L217)
- `RouteX/Controllers/UsersController.cs` — where existing user password is updated from Identity (`existingUser.Password = passwordHash`): [RouteX/Controllers/UsersController.cs](RouteX/Controllers/UsersController.cs#L369)
- `RouteX/appsettings.json` — connection string with `Encrypt=True` (transport encryption): [RouteX/appsettings.json](RouteX/appsettings.json#L15-L16)

To capture encrypted data from the database, run the SQL above and screenshot the results grid returned by your DB client.

---
File added to describe encryption usage and indicate where to capture screenshots of hashed/encrypted values.
