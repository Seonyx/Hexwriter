-- ============================================
-- HexWriter Fresh Database Schema
-- Run this on a new empty HexWriter database.
-- All migrations are consolidated here — do not
-- run the individual legacy scripts separately.
-- ============================================

-- Divisions
CREATE TABLE Divisions (
    DivisionId      INT           PRIMARY KEY IDENTITY(1,1),
    Name            NVARCHAR(200) NOT NULL,
    Slug            NVARCHAR(200) NOT NULL UNIQUE,
    Description     NVARCHAR(MAX),
    LogoUrl         NVARCHAR(500),
    WebsiteUrl      NVARCHAR(500),
    SortOrder       INT           DEFAULT 0,
    IsActive        BIT           DEFAULT 1,
    BackgroundColor NVARCHAR(7),
    ForegroundColor NVARCHAR(7),
    CreatedDate     DATETIME      DEFAULT GETDATE()
);
CREATE INDEX IX_Divisions_Slug ON Divisions(Slug);

-- Pages
CREATE TABLE Pages (
    PageId            INT           PRIMARY KEY IDENTITY(1,1),
    Slug              NVARCHAR(200) NOT NULL UNIQUE,
    Title             NVARCHAR(500) NOT NULL,
    MetaDescription   NVARCHAR(500),
    MetaKeywords      NVARCHAR(500),
    Content           NVARCHAR(MAX) NOT NULL,
    ParentPageId      INT           NULL,
    DivisionId        INT           NULL,
    SortOrder         INT           DEFAULT 0,
    IsPublished       BIT           DEFAULT 1,
    ShowInNavigation  BIT           DEFAULT 1,
    CreatedDate       DATETIME      DEFAULT GETDATE(),
    ModifiedDate      DATETIME      DEFAULT GETDATE(),
    CONSTRAINT FK_Pages_Parent   FOREIGN KEY (ParentPageId) REFERENCES Pages(PageId),
    CONSTRAINT FK_Pages_Division FOREIGN KEY (DivisionId)  REFERENCES Divisions(DivisionId)
);
CREATE INDEX IX_Pages_Slug     ON Pages(Slug);
CREATE INDEX IX_Pages_Division ON Pages(DivisionId);
CREATE INDEX IX_Pages_Parent   ON Pages(ParentPageId);

-- ContentBlocks
CREATE TABLE ContentBlocks (
    BlockId      INT           PRIMARY KEY IDENTITY(1,1),
    BlockKey     NVARCHAR(100) NOT NULL UNIQUE,
    Title        NVARCHAR(200) NOT NULL,
    Content      NVARCHAR(MAX) NOT NULL,
    IsActive     BIT           DEFAULT 1,
    ModifiedDate DATETIME      DEFAULT GETDATE()
);
CREATE INDEX IX_ContentBlocks_Key ON ContentBlocks(BlockKey);

-- ContactSubmissions
CREATE TABLE ContactSubmissions (
    SubmissionId  INT           PRIMARY KEY IDENTITY(1,1),
    Name          NVARCHAR(200) NOT NULL,
    Email         NVARCHAR(320) NOT NULL,
    Subject       NVARCHAR(500),
    Message       NVARCHAR(MAX) NOT NULL,
    IpAddress     NVARCHAR(45),
    UserAgent     NVARCHAR(500),
    IsRead        BIT           DEFAULT 0,
    IsSpam        BIT           DEFAULT 0,
    SubmittedDate DATETIME      DEFAULT GETDATE()
);
CREATE INDEX IX_ContactSubmissions_Date ON ContactSubmissions(SubmittedDate DESC);
CREATE INDEX IX_ContactSubmissions_Read ON ContactSubmissions(IsRead);

-- PaidContactSubmissions
CREATE TABLE PaidContactSubmissions (
    Id                      INT           PRIMARY KEY IDENTITY(1,1),
    ReferenceId             NVARCHAR(36)  NOT NULL,
    Name                    NVARCHAR(200) NOT NULL,
    Email                   NVARCHAR(320) NOT NULL,
    Company                 NVARCHAR(200) NULL,
    Subject                 NVARCHAR(500) NOT NULL,
    Message                 NVARCHAR(MAX) NOT NULL,
    IpAddress               NVARCHAR(45)  NULL,
    UserAgent               NVARCHAR(500) NULL,
    StripeCheckoutSessionId NVARCHAR(200) NULL,
    StripePaymentIntentId   NVARCHAR(200) NULL,
    AmountPaid              INT           NULL,
    Status                  NVARCHAR(20)  NOT NULL DEFAULT 'Pending',
    SubmittedDate           DATETIME      NOT NULL DEFAULT GETUTCDATE(),
    ProcessedDate           DATETIME      NULL,
    CONSTRAINT UQ_PaidContactSubmissions_ReferenceId UNIQUE (ReferenceId)
);
CREATE INDEX IX_PaidContactSubmissions_ReferenceId ON PaidContactSubmissions(ReferenceId);
CREATE INDEX IX_PaidContactSubmissions_Status      ON PaidContactSubmissions(Status);

-- Authors
CREATE TABLE Authors (
    AuthorId    INT           PRIMARY KEY IDENTITY(1,1),
    PenName     NVARCHAR(200) NOT NULL,
    Biography   NVARCHAR(MAX),
    PhotoUrl    NVARCHAR(500),
    Genre       NVARCHAR(200),
    Website     NVARCHAR(500),
    SortOrder   INT           DEFAULT 0,
    IsActive    BIT           DEFAULT 1,
    CreatedDate DATETIME      DEFAULT GETDATE()
);

-- Books
CREATE TABLE Books (
    BookId          INT           PRIMARY KEY IDENTITY(1,1),
    AuthorId        INT           NOT NULL,
    Title           NVARCHAR(500) NOT NULL,
    Synopsis        NVARCHAR(MAX),
    CoverImageUrl   NVARCHAR(500),
    AmazonUrl       NVARCHAR(500),
    KDPUrl          NVARCHAR(500),
    ISBN            NVARCHAR(20),
    PublicationDate DATE,
    Genre           NVARCHAR(200),
    SortOrder       INT           DEFAULT 0,
    IsPublished     BIT           DEFAULT 0,
    CreatedDate     DATETIME      DEFAULT GETDATE(),
    CONSTRAINT FK_Books_Author FOREIGN KEY (AuthorId) REFERENCES Authors(AuthorId)
);
CREATE INDEX IX_Books_Author ON Books(AuthorId);

-- SiteSettings
CREATE TABLE SiteSettings (
    SettingKey   NVARCHAR(100) PRIMARY KEY,
    SettingValue NVARCHAR(MAX),
    Description  NVARCHAR(500),
    ModifiedDate DATETIME      DEFAULT GETDATE()
);

-- ============================================
-- Book Editor
-- ============================================

-- BookProjects (includes all migration columns)
CREATE TABLE BookProjects (
    BookProjectID       INT           PRIMARY KEY IDENTITY(1,1),
    ProjectName         NVARCHAR(255) NOT NULL UNIQUE,
    CoverImagePath      NVARCHAR(500) NULL,
    FolderPath          NVARCHAR(500) NOT NULL,
    Author              NVARCHAR(255) NULL,
    BookmlId            NVARCHAR(100) NULL,
    CurrentDraftNumber  INT           NOT NULL DEFAULT 1,
    BookmarkParagraphID INT           NULL,
    CreatedDate         DATETIME      NOT NULL DEFAULT GETDATE(),
    LastModifiedDate    DATETIME      NOT NULL DEFAULT GETDATE(),
    IsActive            BIT           NOT NULL DEFAULT 1
);

-- Chapters (includes SortOrder and BookmlChapterId)
CREATE TABLE Chapters (
    ChapterID       INT           PRIMARY KEY IDENTITY(1,1),
    BookProjectID   INT           NOT NULL,
    ChapterNumber   INT           NOT NULL,
    SortOrder       INT           NOT NULL DEFAULT 0,
    ChapterTitle    NVARCHAR(500) NULL,
    BookmlChapterId NVARCHAR(50)  NULL,
    POV             NVARCHAR(255) NULL,
    Setting         NVARCHAR(500) NULL,
    ChapterPurpose  NVARCHAR(MAX) NULL,
    SourceFileName  NVARCHAR(255) NULL,
    CONSTRAINT FK_Chapters_BookProjects FOREIGN KEY (BookProjectID)
        REFERENCES BookProjects(BookProjectID) ON DELETE CASCADE,
    CONSTRAINT UQ_Chapter_Per_Book UNIQUE (BookProjectID, ChapterNumber)
);
CREATE INDEX IX_Chapters_BookmlId ON Chapters(BookmlChapterId);

-- Paragraphs
CREATE TABLE Paragraphs (
    ParagraphID      INT           PRIMARY KEY IDENTITY(1,1),
    ChapterID        INT           NOT NULL,
    UniqueID         NVARCHAR(50)  NOT NULL,
    OrdinalPosition  INT           NOT NULL,
    ParagraphText    NVARCHAR(MAX) NOT NULL,
    CreatedDate      DATETIME      NOT NULL DEFAULT GETDATE(),
    LastModifiedDate DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Paragraphs_Chapters FOREIGN KEY (ChapterID)
        REFERENCES Chapters(ChapterID) ON DELETE CASCADE,
    CONSTRAINT UQ_UniqueID_Per_Chapter UNIQUE (ChapterID, UniqueID)
);
CREATE INDEX IX_Paragraphs_OrdinalPosition ON Paragraphs(ChapterID, OrdinalPosition);
CREATE INDEX IX_Paragraphs_UniqueID        ON Paragraphs(UniqueID);

-- MetaNotes
CREATE TABLE MetaNotes (
    MetaNoteID  INT           PRIMARY KEY IDENTITY(1,1),
    ParagraphID INT           NOT NULL,
    UniqueID    NVARCHAR(50)  NOT NULL,
    MetaText    NVARCHAR(MAX) NOT NULL,
    CONSTRAINT FK_MetaNotes_Paragraphs FOREIGN KEY (ParagraphID)
        REFERENCES Paragraphs(ParagraphID) ON DELETE CASCADE,
    CONSTRAINT UQ_MetaNote_Per_Paragraph UNIQUE (ParagraphID)
);
CREATE INDEX IX_MetaNotes_UniqueID ON MetaNotes(UniqueID);

-- EditNotes
CREATE TABLE EditNotes (
    EditNoteID       INT           PRIMARY KEY IDENTITY(1,1),
    ParagraphID      INT           NOT NULL,
    UniqueID         NVARCHAR(50)  NOT NULL,
    NoteText         NVARCHAR(MAX) NULL,
    LastModifiedDate DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_EditNotes_Paragraphs FOREIGN KEY (ParagraphID)
        REFERENCES Paragraphs(ParagraphID) ON DELETE CASCADE,
    CONSTRAINT UQ_EditNote_Per_Paragraph UNIQUE (ParagraphID)
);
CREATE INDEX IX_EditNotes_UniqueID ON EditNotes(UniqueID);

-- Drafts
CREATE TABLE Drafts (
    DraftID       INT           PRIMARY KEY IDENTITY(1,1),
    BookProjectID INT           NOT NULL,
    DraftNumber   INT           NOT NULL,
    Status        NVARCHAR(20)  NOT NULL DEFAULT 'in-progress',
    CreatedDate   DATETIME      NOT NULL DEFAULT GETDATE(),
    BasedOn       INT           NOT NULL DEFAULT 0,
    AuthorType    NVARCHAR(10)  NOT NULL,
    Author        NVARCHAR(200) NOT NULL,
    Label         NVARCHAR(200) NULL,
    ExportDate    DATETIME      NULL,
    DraftNote     NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Drafts_BookProjects FOREIGN KEY (BookProjectID)
        REFERENCES BookProjects(BookProjectID) ON DELETE CASCADE,
    CONSTRAINT UQ_Draft_Per_Book UNIQUE (BookProjectID, DraftNumber)
);
CREATE INDEX IX_Drafts_BookProject ON Drafts(BookProjectID);

-- ParagraphVersions (append-only; unique constraint scoped to chapter)
CREATE TABLE ParagraphVersions (
    VersionID    INT           PRIMARY KEY IDENTITY(1,1),
    Pid          NVARCHAR(50)  NOT NULL,
    ChapterID    INT           NOT NULL,
    DraftNumber  INT           NOT NULL,
    Seq          INT           NOT NULL,
    ParaType     NVARCHAR(20)  NOT NULL DEFAULT 'normal',
    Content      NVARCHAR(MAX) NOT NULL,
    DraftCreated INT           NOT NULL,
    DraftModified INT          NOT NULL,
    ModifiedBy   NVARCHAR(10)  NOT NULL,
    ModifiedDate DATETIME      NULL,
    ChangeType   NVARCHAR(20)  NULL,
    CONSTRAINT FK_ParaVersions_Chapters FOREIGN KEY (ChapterID)
        REFERENCES Chapters(ChapterID) ON DELETE CASCADE,
    CONSTRAINT UQ_PidVersion UNIQUE (ChapterID, Pid, DraftNumber)
);
CREATE INDEX IX_ParaVersions_Pid     ON ParagraphVersions(Pid);
CREATE INDEX IX_ParaVersions_Chapter ON ParagraphVersions(ChapterID, DraftNumber);

-- ImportLogs (includes SourceFileName)
CREATE TABLE ImportLogs (
    ImportLogID       INT           PRIMARY KEY IDENTITY(1,1),
    BookProjectID     INT           NOT NULL,
    ImportedAt        DATETIME      NOT NULL DEFAULT GETDATE(),
    Success           BIT           NOT NULL,
    DraftNumber       INT           NOT NULL DEFAULT 0,
    ChaptersProcessed INT           NOT NULL DEFAULT 0,
    ParagraphsAdded   INT           NOT NULL DEFAULT 0,
    ParagraphsUpdated INT           NOT NULL DEFAULT 0,
    ParagraphsRemoved INT           NOT NULL DEFAULT 0,
    VersionsRecorded  INT           NOT NULL DEFAULT 0,
    WarningCount      INT           NOT NULL DEFAULT 0,
    ErrorCount        INT           NOT NULL DEFAULT 0,
    FullLog           NVARCHAR(MAX) NULL,
    SourceFileName    NVARCHAR(255) NULL,
    CONSTRAINT FK_ImportLogs_BookProjects FOREIGN KEY (BookProjectID)
        REFERENCES BookProjects(BookProjectID)
);
CREATE INDEX IX_ImportLogs_Project ON ImportLogs(BookProjectID, ImportedAt DESC);

-- Characters
CREATE TABLE Characters (
    CharacterId           VARCHAR(40)   NOT NULL,
    BookProjectID         INT           NOT NULL,
    Name                  NVARCHAR(200) NOT NULL,
    Pronouns              NVARCHAR(100) NULL,
    AgeDisplay            NVARCHAR(50)  NULL,
    DateOfBirth           DATE          NULL,
    SpeciesType           NVARCHAR(100) NULL,
    StatusCode            VARCHAR(40)   NULL,
    StoryRoleCode         VARCHAR(40)   NOT NULL,
    ImportanceCode        VARCHAR(40)   NOT NULL,
    SpoilerLevelCode      VARCHAR(40)   NULL,
    IsPov                 BIT           NOT NULL DEFAULT 0,
    HomeLocation          NVARCHAR(200) NULL,
    CurrentLocation       NVARCHAR(200) NULL,
    Faction               NVARCHAR(200) NULL,
    Occupation            NVARCHAR(200) NULL,
    ClassStatus           NVARCHAR(200) NULL,
    Goal                  NVARCHAR(MAX) NULL,
    Need                  NVARCHAR(MAX) NULL,
    Motivation            NVARCHAR(MAX) NULL,
    Stakes                NVARCHAR(MAX) NULL,
    Fear                  NVARCHAR(MAX) NULL,
    Flaw                  NVARCHAR(MAX) NULL,
    Strength              NVARCHAR(MAX) NULL,
    LieTheyBelieve        NVARCHAR(MAX) NULL,
    CoreValue             NVARCHAR(MAX) NULL,
    InternalConflict      NVARCHAR(MAX) NULL,
    ExternalConflict      NVARCHAR(MAX) NULL,
    Secret                NVARCHAR(MAX) NULL,
    LineTheyWontCross     NVARCHAR(MAX) NULL,
    PhysicalDescription   NVARCHAR(MAX) NULL,
    DistinctiveFeatures   NVARCHAR(MAX) NULL,
    ClothingProps         NVARCHAR(MAX) NULL,
    VoicePattern          NVARCHAR(MAX) NULL,
    HabitsMannerisms      NVARCHAR(MAX) NULL,
    BodyLanguage          NVARCHAR(MAX) NULL,
    PublicPersona         NVARCHAR(MAX) NULL,
    PrivateSelf           NVARCHAR(MAX) NULL,
    EducationTraining     NVARCHAR(MAX) NULL,
    OriginBackground      NVARCHAR(MAX) NULL,
    ImportantPastEvents   NVARCHAR(MAX) NULL,
    FamilyNotes           NVARCHAR(MAX) NULL,
    WeaknessesLimitations NVARCHAR(MAX) NULL,
    HealthInjuries        NVARCHAR(MAX) NULL,
    StoryArcSummary       NVARCHAR(MAX) NULL,
    CharacterFunction     NVARCHAR(MAX) NULL,
    RelationshipSummary   NVARCHAR(MAX) NULL,
    ContinuityNotes       NVARCHAR(MAX) NULL,
    UnresolvedThreads     NVARCHAR(MAX) NULL,
    Notes                 NVARCHAR(MAX) NULL,
    ReferenceImageUri     NVARCHAR(500) NULL,
    CreatedAt             DATETIME2     NOT NULL DEFAULT GETDATE(),
    UpdatedAt             DATETIME2     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Characters         PRIMARY KEY (CharacterId),
    CONSTRAINT FK_Characters_BookProject FOREIGN KEY (BookProjectID)
        REFERENCES BookProjects(BookProjectID)
);
CREATE INDEX IX_Characters_BookProjectID ON Characters(BookProjectID);

-- CharacterAliases
CREATE TABLE CharacterAliases (
    CharacterAliasId VARCHAR(40)   NOT NULL,
    CharacterId      VARCHAR(40)   NOT NULL,
    Alias            NVARCHAR(200) NOT NULL,
    AliasTypeCode    VARCHAR(40)   NULL,
    SortOrder        INT           NULL,
    Notes            NVARCHAR(MAX) NULL,
    CONSTRAINT PK_CharacterAliases        PRIMARY KEY (CharacterAliasId),
    CONSTRAINT FK_CharAliases_Characters  FOREIGN KEY (CharacterId)
        REFERENCES Characters(CharacterId) ON DELETE CASCADE
);
CREATE INDEX IX_CharacterAliases_CharacterId ON CharacterAliases(CharacterId);

-- CharacterTags
CREATE TABLE CharacterTags (
    CharacterTagId VARCHAR(40)   NOT NULL,
    CharacterId    VARCHAR(40)   NOT NULL,
    Tag            NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_CharacterTags       PRIMARY KEY (CharacterTagId),
    CONSTRAINT FK_CharTags_Characters FOREIGN KEY (CharacterId)
        REFERENCES Characters(CharacterId) ON DELETE CASCADE
);
CREATE UNIQUE INDEX UX_CharacterTags_CharTag ON CharacterTags(CharacterId, Tag);
