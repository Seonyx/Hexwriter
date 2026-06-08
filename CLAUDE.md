# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

HexWriter is a standalone book-editor web application at Hexwriter.com, extracted from the Seonyx.com codebase and extended with a multiuser permissions system.

## Stack

- **Backend**: ASP.NET MVC 5, .NET Framework 4.6.2, Entity Framework 6, SQL Server, C#
- **Dev workflow**: Code is written on Linux (Vimes) with Claude Code / VSCode. Files are copied to Windows 10 (Dibbler) via `copy-updates.ps1` and built/tested in Visual Studio 2022. Do not attempt to compile or run the project on Linux.
- **Copy script**: `copy-updates.ps1` in the repo root. Source: `\\192.168.69.19\hexwriter`. Destination: `C:\Users\Steve\Dropbox\VITALIS\Hexwriter\Hexwriter_web_src`

## Local Setup on Dibbler (first time)

**1. Create the database in SSMS:**
- Create a new empty database named `HexWriter`
- Open and run `Database\hexwriter-schema.sql` against it

**2. Create Web.config from the template:**
- Copy `Web.config.template` to `Web.config` in the same folder
- Update the connection string:
  ```xml
  <add name="HexWriterContext"
       connectionString="Server=(LocalDB)\MSSQLLocalDB;Database=HexWriter;Integrated Security=True;"
       providerName="System.Data.SqlClient" />
  ```
  Dibbler uses SQL Server LocalDB, not SQL Express.

**3. Build:**
- Right-click solution → Restore NuGet Packages
- Build → Build Solution (Ctrl+Shift+B)
- F5 to run — navigate to `/admin/login`

## Architecture

- **Database**: Manual SQL schema (`Database/hexwriter-schema.sql`). EF `SetInitializer` is disabled — no automatic migration on startup. New tables added in Phases 2+ use EF6 Code First migrations.
- **Authentication**: Currently Forms Authentication with a single admin account (web.config hash — to be replaced in Phase 2).
- **DbContext**: `HexWriter.Web.Models.HexWriterContext`
- **Layouts**: Admin pages use `_AdminLayout.cshtml`; book editor pages use `_BookEditorLayout.cshtml`
- **Routes**: All routes are under `/admin/...` — there is no public-facing website. Defined in `App_Start/RouteConfig.cs`.
- **ContentAnalysisEngine**: Separate class library project in the solution, referenced as a compiled DLL. Must be built before HexWriter (handled automatically by the solution's project dependency).

## BookML

The book editor uses a formal XML interchange format. See `Documentation/XML-transition-docs/BOOKML-SPEC.md` for the full spec. Key rules:
- `pid` is permanent and immutable; `seq` is a display ordinal only
- `ParagraphVersions` is append-only — never update or delete rows
- Always join on `pid`, never on `seq` or position
