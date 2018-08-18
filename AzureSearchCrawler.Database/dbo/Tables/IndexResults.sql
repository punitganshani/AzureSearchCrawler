CREATE TABLE [dbo].[IndexResults] (
    [Id]               NVARCHAR (20)  NOT NULL,
    [Url]              NVARCHAR (MAX) NOT NULL,
    [Title]            NVARCHAR (200) NULL,
    [TextContent]      NVARCHAR (MAX) NULL,
    [Keywords]         NVARCHAR (400) NULL,
    [Description]      NVARCHAR (400) NULL,
    [LastModifiedDate] DATETIME2 (7)  NOT NULL,
    [Host]             NVARCHAR (100) NOT NULL,
    [Path]             NVARCHAR (400) NOT NULL,
    [Query]            NVARCHAR (MAX) NULL,
    [IsActive]         BIT            CONSTRAINT [DF_IndexResults_IsActive] DEFAULT ((1)) NOT NULL,
    [BreadCrump]       NVARCHAR (400) NULL,
    [Category]         NVARCHAR (100) NULL,
    [SubCategory]      NVARCHAR (100) NULL,
    [Hash]             NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_IndexResults] PRIMARY KEY CLUSTERED ([Id] ASC)
);

