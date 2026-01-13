HCAMiniEHR — Mini Electronic Health Record (Razor Pages)

Overview
--------
This is a sample Razor Pages application (ASP.NET Core / .NET 8) implementing a small EHR system with Patients, Appointments, Lab Orders and Reports/Analytics generated via LINQ/EF Core.

Quick start
-----------
1. Requirements
   - .NET 8 SDK
   - SQL Server instance accessible from the machine
   - Recommended: Visual Studio 2022/2023 or VS Code

2. Configure connection string
   - Edit `appsettings.json` in the `HCAMiniEHR` project.
   - The project expects a connection string key: `EHRConnection`.
     Example:
     ```json
     "ConnectionStrings": {
       "EHRConnection": "Server=.;Database=MiniProject;Trusted_Connection=True;TrustServerCertificate=True"
     }
     ```
   - If using SQL Server Authentication, replace with `User Id` / `Password` values.

3. Run database (development)
   - The app will attempt to `EnsureCreated()` at startup and seed minimal data when the DB is empty (for convenience). If you prefer migrations:
     - Add migration: `dotnet ef migrations add InitialCreate -p HCAMiniEHR/HCAMiniEHR.csproj`
     - Update database: `dotnet ef database update -p HCAMiniEHR/HCAMiniEHR.csproj`

4. Run the app
   - From the project folder: `dotnet run --project HCAMiniEHR/HCAMiniEHR.csproj` or run from your IDE.
   - Open browser: https://localhost:5001 or the port shown in the console.

Project structure (important files)
----------------------------------
- `Data/` — EF Core context and model classes (`EHRDbContext`, `Patient`, `Appointment`, `LabOrder`, `AuditLog`).
- `Services/` — Application services for business logic (PatientService, AppointmentService, LabOrderService, ReportService).
- `Pages/` — Razor Pages for Patients, Appointments, LabOrders, Reports.
- `Program.cs` — DI and app bootstrap.
- `appsettings.json` — Connection string configuration.

How to use reports
------------------
- Reports page: `/Reports` (UI provides links). You can open individual reports via query string:
  - Pending Lab Orders: `/Reports?type=pending`
  - Patients Without Follow-up: `/Reports?type=followup`
  - Doctor Productivity: `/Reports?type=doctor`
  - Lab Test Summary: `/Reports?type=labsummary`

Known issues & implemented workarounds
-------------------------------------
1. EF Core and DB triggers
   - If the database table has triggers, EF Core's default INSERT uses an `OUTPUT` clause which may fail (SQL Server error about OUTPUT and triggers).
   - Workaround implemented for `Appointment` creation: the service uses a parameterized ADO.NET INSERT + `SCOPE_IDENTITY()` to obtain the new identity value, avoiding the OUTPUT clause. If you control the DB and prefer, you may remove or adjust triggers instead.

2. LINQ translation for TimeSpan averages in reports
   - Averaging `(CompletedDate - OrderDate).TotalDays` cannot be translated into SQL by EF Core. The `ReportService` uses `EF.Functions.DateDiffDay` to compute day differences server-side and then averages those values so the query remains server-translatable.

3. Model binding and validation for appointments
   - Ensure the patient dropdown default value is numeric (0) and server-side validation checks `PatientId > 0`. The Create/Edit pages include validation summary and server checks to produce clear errors if no patient is selected.

Troubleshooting
---------------
- "The Patient field is required." — Open the Create page, verify you selected a patient, check browser DevTools ? Network ? Form Data; `Appointment.PatientId` should be sent as a numeric value. If not, ensure the select default option value is `0` and that the form control is named `Appointment.PatientId`.
- Reports show "No data available" — confirm the DB has rows for LabOrders and Appointments. The app seeds minimal demo data when it finds an empty DB. Alternatively, run migrations and seed data manually.
- SQL connectivity errors — verify server name, database name, and authentication in `appsettings.json`.

Developer notes
---------------
- EF Core package versions are specified in the project file. Keep them aligned with .NET 8 supported EF Core 8 packages.
- To change seed data or remove auto-seed behavior, update `Program.cs` (the app currently seeds sample data when `db.Patients` is empty).
- The `ReportService` uses anonymous projections returning `List<object>` for quick rendering in Razor. For better type safety, consider defining DTO types.

Further improvements (recommended)
---------------------------------
- Replace anonymous objects in reports with explicit DTO classes.
- Add unit/integration tests for report queries and appointment create/update logic.
- Add pagination for reports and appointment lists.
- Add role-based access and authentication for production use.

If you need
-----------
- I can add automated EF Core migrations, convert anonymous report results to DTOs, or add detailed logging for failed DB operations.

