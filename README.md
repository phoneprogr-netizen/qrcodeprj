# QR Portal (ASP.NET Core MVC / .NET 10)

Portale multi-tenant per gestione QR Code statici/dinamici con ruoli **SuperAdmin/Admin/Customer**, tracking scansioni, piani/abbonamenti e aree separate admin/customer.

## Struttura solution
- `src/QrPortal.Web`: MVC, Razor, Bootstrap, autenticazione, controller admin/customer/public.
- `src/QrPortal.Application`: servizi applicativi, regole business, DTO.
- `src/QrPortal.Domain`: entità dominio, enum, costanti.
- `src/QrPortal.DataAccess`: repository Dapper e accesso SQL Server.
- `database-scripts`: script SQL manuali (no migrations/EF).

## Setup
1. Creare DB SQL Server ed eseguire in ordine gli script in `database-scripts`.
2. Configurare `ConnectionStrings:DefaultConnection` in `appsettings.json` del progetto web.
3. Configurare `AppSettings:BaseRedirectUrl`.
4. Avviare web app.

## Credenziali seed
- Username: `superadmin`
- Password: `ChangeMe123!`

> Password hash e sale vanno rigenerati in produzione.

## Note tecniche
- Accesso dati con **Dapper**.
- Nessun uso di EF e migrations.
- Filtraggio multi-tenant su `ClientId` lato backend.
- Soft delete per QR.
- Controlli piano/abbonamento in `QrCodeService`.
