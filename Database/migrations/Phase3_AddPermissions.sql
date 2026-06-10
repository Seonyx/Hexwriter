-- Phase 3 migration: Groups, GroupUsers, BookGroups, BookUsers
-- Run against the HexWriter database if Update-Database fails.

CREATE TABLE [dbo].[Groups] (
    [Id]          INT           IDENTITY(1,1) NOT NULL,
    [Name]        NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [CreatedAt]   DATETIME      NOT NULL,
    CONSTRAINT [PK_dbo.Groups] PRIMARY KEY ([Id])
);
CREATE UNIQUE INDEX [UQ_Groups_Name] ON [dbo].[Groups]([Name]);

CREATE TABLE [dbo].[GroupUsers] (
    [Id]      INT      IDENTITY(1,1) NOT NULL,
    [GroupId] INT      NOT NULL,
    [UserId]  INT      NOT NULL,
    [AddedAt] DATETIME NOT NULL,
    CONSTRAINT [PK_dbo.GroupUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GroupUsers_Groups] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[Groups]([Id]),
    CONSTRAINT [FK_GroupUsers_Users]  FOREIGN KEY ([UserId])  REFERENCES [dbo].[Users]([Id])
);
CREATE UNIQUE INDEX [UQ_GroupUsers_GroupUser] ON [dbo].[GroupUsers]([GroupId], [UserId]);

CREATE TABLE [dbo].[BookGroups] (
    [Id]            INT          IDENTITY(1,1) NOT NULL,
    [BookProjectID] INT          NOT NULL,
    [GroupId]       INT          NOT NULL,
    [AccessLevel]   NVARCHAR(20) NOT NULL,
    [GrantedAt]     DATETIME     NOT NULL,
    CONSTRAINT [PK_dbo.BookGroups]          PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BookGroups_BookProjects] FOREIGN KEY ([BookProjectID]) REFERENCES [dbo].[BookProjects]([BookProjectID]),
    CONSTRAINT [FK_BookGroups_Groups]       FOREIGN KEY ([GroupId])       REFERENCES [dbo].[Groups]([Id])
);
CREATE UNIQUE INDEX [UQ_BookGroups_BookGroup] ON [dbo].[BookGroups]([BookProjectID], [GroupId]);

CREATE TABLE [dbo].[BookUsers] (
    [Id]            INT          IDENTITY(1,1) NOT NULL,
    [BookProjectID] INT          NOT NULL,
    [UserId]        INT          NOT NULL,
    [AccessLevel]   NVARCHAR(20) NOT NULL,
    [GrantedAt]     DATETIME     NOT NULL,
    CONSTRAINT [PK_dbo.BookUsers]          PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BookUsers_BookProjects] FOREIGN KEY ([BookProjectID]) REFERENCES [dbo].[BookProjects]([BookProjectID]),
    CONSTRAINT [FK_BookUsers_Users]        FOREIGN KEY ([UserId])        REFERENCES [dbo].[Users]([Id])
);
CREATE UNIQUE INDEX [UQ_BookUsers_BookUser] ON [dbo].[BookUsers]([BookProjectID], [UserId]);

-- Mark migration as applied in EF history
INSERT INTO [dbo].[__MigrationHistory] ([MigrationId], [ContextKey], [Model], [ProductVersion])
VALUES ('20260610120001_AddPermissions', 'HexWriter.Web.Models.HexWriterContext', 0x, '6.4.4');
