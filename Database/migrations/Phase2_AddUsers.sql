-- Phase 2 migration: create Users table
-- Run this in SSMS against the HexWriter database if Update-Database fails.
-- After running this script, start the application once — Application_Start
-- will seed the admin user (username: admin, password: ChangeMe123!)

CREATE TABLE [dbo].[Users] (
    [Id]           INT           IDENTITY(1,1) NOT NULL,
    [Username]     NVARCHAR(100) NOT NULL,
    [Email]        NVARCHAR(320) NOT NULL,
    [DisplayName]  NVARCHAR(200) NOT NULL,
    [PasswordHash] NVARCHAR(255) NOT NULL,
    [Role]         NVARCHAR(50)  NOT NULL,
    [IsActive]     BIT           NOT NULL,
    [CreatedAt]    DATETIME      NOT NULL,
    [LastLoginAt]  DATETIME      NULL,
    CONSTRAINT [PK_dbo.Users] PRIMARY KEY ([Id])
);
CREATE UNIQUE INDEX [UQ_Users_Username] ON [dbo].[Users]([Username]);
CREATE UNIQUE INDEX [UQ_Users_Email]    ON [dbo].[Users]([Email]);

-- EF migration history so Update-Database knows these migrations are applied
CREATE TABLE [dbo].[__MigrationHistory] (
    [MigrationId]    NVARCHAR(150) NOT NULL,
    [ContextKey]     NVARCHAR(300) NOT NULL,
    [Model]          VARBINARY(MAX) NOT NULL,
    [ProductVersion] NVARCHAR(32) NOT NULL,
    CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY ([MigrationId], [ContextKey])
);
INSERT INTO [dbo].[__MigrationHistory] ([MigrationId], [ContextKey], [Model], [ProductVersion])
VALUES
    ('20260609120001_InitialCreate', 'HexWriter.Web.Models.HexWriterContext', 0x, '6.4.4'),
    ('20260609120002_AddUsers',      'HexWriter.Web.Models.HexWriterContext', 0x, '6.4.4');
