
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 11/09/2012 03:22:25
-- Generated from EDMX file: C:\projects\TDS\discussions\DbModel\Model.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [disc3];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_TopicArgPoint]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ArgPoint] DROP CONSTRAINT [FK_TopicArgPoint];
GO
IF OBJECT_ID(N'[dbo].[FK_DiscussionTopic]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Topic] DROP CONSTRAINT [FK_DiscussionTopic];
GO
IF OBJECT_ID(N'[dbo].[FK_ArgPointComment]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comment] DROP CONSTRAINT [FK_ArgPointComment];
GO
IF OBJECT_ID(N'[dbo].[FK_ArgPointAttachment]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Attachment] DROP CONSTRAINT [FK_ArgPointAttachment];
GO
IF OBJECT_ID(N'[dbo].[FK_TopicPerson_Topic]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TopicPerson] DROP CONSTRAINT [FK_TopicPerson_Topic];
GO
IF OBJECT_ID(N'[dbo].[FK_TopicPerson_Person]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TopicPerson] DROP CONSTRAINT [FK_TopicPerson_Person];
GO
IF OBJECT_ID(N'[dbo].[FK_PersonArgPoint]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ArgPoint] DROP CONSTRAINT [FK_PersonArgPoint];
GO
IF OBJECT_ID(N'[dbo].[FK_PersonComment]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comment] DROP CONSTRAINT [FK_PersonComment];
GO
IF OBJECT_ID(N'[dbo].[FK_PersonAttachment]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Attachment] DROP CONSTRAINT [FK_PersonAttachment];
GO
IF OBJECT_ID(N'[dbo].[FK_DiscussionAttachment]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Attachment] DROP CONSTRAINT [FK_DiscussionAttachment];
GO
IF OBJECT_ID(N'[dbo].[FK_PersonGeneralSide]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GeneralSide] DROP CONSTRAINT [FK_PersonGeneralSide];
GO
IF OBJECT_ID(N'[dbo].[FK_DiscussionGeneralSide]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GeneralSide] DROP CONSTRAINT [FK_DiscussionGeneralSide];
GO
IF OBJECT_ID(N'[dbo].[FK_RichTextSource]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Source] DROP CONSTRAINT [FK_RichTextSource];
GO
IF OBJECT_ID(N'[dbo].[FK_DiscussionRichText]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RichText] DROP CONSTRAINT [FK_DiscussionRichText];
GO
IF OBJECT_ID(N'[dbo].[FK_ArgPointRichText]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RichText] DROP CONSTRAINT [FK_ArgPointRichText];
GO
IF OBJECT_ID(N'[dbo].[FK_PersonScreenshot]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Annotation] DROP CONSTRAINT [FK_PersonScreenshot];
GO
IF OBJECT_ID(N'[dbo].[FK_DiscussionAnnotation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Annotation] DROP CONSTRAINT [FK_DiscussionAnnotation];
GO
IF OBJECT_ID(N'[dbo].[FK_PersonAttachment1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Attachment] DROP CONSTRAINT [FK_PersonAttachment1];
GO
IF OBJECT_ID(N'[dbo].[FK_PersonSeat]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Person] DROP CONSTRAINT [FK_PersonSeat];
GO
IF OBJECT_ID(N'[dbo].[FK_PersonSession]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Person] DROP CONSTRAINT [FK_PersonSession];
GO
IF OBJECT_ID(N'[dbo].[FK_AttachmentMediaData]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[MediaDataSet] DROP CONSTRAINT [FK_AttachmentMediaData];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Person]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Person];
GO
IF OBJECT_ID(N'[dbo].[Discussion]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Discussion];
GO
IF OBJECT_ID(N'[dbo].[ArgPoint]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ArgPoint];
GO
IF OBJECT_ID(N'[dbo].[Topic]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Topic];
GO
IF OBJECT_ID(N'[dbo].[Comment]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Comment];
GO
IF OBJECT_ID(N'[dbo].[Attachment]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Attachment];
GO
IF OBJECT_ID(N'[dbo].[GeneralSide]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GeneralSide];
GO
IF OBJECT_ID(N'[dbo].[Source]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Source];
GO
IF OBJECT_ID(N'[dbo].[RichText]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RichText];
GO
IF OBJECT_ID(N'[dbo].[Annotation]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Annotation];
GO
IF OBJECT_ID(N'[dbo].[StatsEvent]', 'U') IS NOT NULL
    DROP TABLE [dbo].[StatsEvent];
GO
IF OBJECT_ID(N'[dbo].[Session]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Session];
GO
IF OBJECT_ID(N'[dbo].[Seat]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Seat];
GO
IF OBJECT_ID(N'[dbo].[MediaDataSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MediaDataSet];
GO
IF OBJECT_ID(N'[dbo].[TopicPerson]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TopicPerson];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Person'
CREATE TABLE [dbo].[Person] (
    [Name] nvarchar(max)  NOT NULL,
    [Email] nchar(255)  NOT NULL,
    [Id] int IDENTITY(1,1) NOT NULL,
    [Color] int  NOT NULL,
    [Online] bit  NOT NULL,
    [SeatId] int  NULL,
    [OnlineDevType] int  NOT NULL,
    [SessionId] int  NULL
);
GO

-- Creating table 'Discussion'
CREATE TABLE [dbo].[Discussion] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Subject] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'ArgPoint'
CREATE TABLE [dbo].[ArgPoint] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Point] nvarchar(max)  NOT NULL,
    [SideCode] int  NOT NULL,
    [SharedToPublic] bit  NOT NULL,
    [RecentlyEnteredSource] nvarchar(max)  NOT NULL,
    [RecentlyEnteredMediaUrl] nvarchar(max)  NOT NULL,
    [ChangesPending] bit  NOT NULL,
    [OrderNumber] int  NOT NULL,
    [Topic_Id] int  NULL,
    [Person_Id] int  NULL
);
GO

-- Creating table 'Topic'
CREATE TABLE [dbo].[Topic] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Annotation] varbinary(max)  NULL,
    [Running] bit  NOT NULL,
    [CumulativeDuration] int  NOT NULL,
    [Discussion_Id] int  NULL
);
GO

-- Creating table 'Comment'
CREATE TABLE [dbo].[Comment] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Text] nvarchar(max)  NOT NULL,
    [ArgPoint_Id] int  NULL,
    [Person_Id] int  NULL
);
GO

-- Creating table 'Attachment'
CREATE TABLE [dbo].[Attachment] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Format] int  NOT NULL,
    [VideoThumbURL] nvarchar(max)  NULL,
    [VideoEmbedURL] nvarchar(max)  NULL,
    [VideoLinkURL] nvarchar(max)  NULL,
    [Link] nvarchar(max)  NOT NULL,
    [Title] nvarchar(max)  NOT NULL,
    [Thumb] varbinary(max)  NULL,
    [OrderNumber] int  NOT NULL,
    [ArgPoint_Id] int  NULL,
    [Person_Id] int  NULL,
    [Discussion_Id] int  NULL,
    [PersonWithAvatar_Id] int  NULL
);
GO

-- Creating table 'GeneralSide'
CREATE TABLE [dbo].[GeneralSide] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Side] int  NOT NULL,
    [Person_Id] int  NULL,
    [Discussion_Id] int  NULL
);
GO

-- Creating table 'Source'
CREATE TABLE [dbo].[Source] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Text] nvarchar(max)  NOT NULL,
    [OrderNumber] int  NOT NULL,
    [RichText_Id] int  NULL
);
GO

-- Creating table 'RichText'
CREATE TABLE [dbo].[RichText] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Text] nvarchar(max)  NOT NULL,
    [Discussion_Id] int  NULL,
    [ArgPoint_Id] int  NULL
);
GO

-- Creating table 'Annotation'
CREATE TABLE [dbo].[Annotation] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Bg] varbinary(max)  NULL,
    [VectGraphics] varbinary(max)  NOT NULL,
    [Thumb] varbinary(max)  NOT NULL,
    [CanvWidth] int  NOT NULL,
    [CanvHeight] int  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Person_Id] int  NULL,
    [Discussion_Id] int  NULL
);
GO

-- Creating table 'StatsEvent'
CREATE TABLE [dbo].[StatsEvent] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Event] int  NOT NULL,
    [DiscussionId] int  NOT NULL,
    [DiscussionName] nvarchar(max)  NOT NULL,
    [TopicId] int  NOT NULL,
    [TopicName] nvarchar(max)  NOT NULL,
    [UserId] int  NOT NULL,
    [UserName] nvarchar(max)  NOT NULL,
    [Time] datetime  NOT NULL,
    [DeviceType] int  NOT NULL
);
GO

-- Creating table 'Session'
CREATE TABLE [dbo].[Session] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [EstimatedDateTime] datetime  NOT NULL,
    [EstimatedTimeSlot] int  NOT NULL,
    [EstimatedEndDateTime] datetime  NOT NULL
);
GO

-- Creating table 'Seat'
CREATE TABLE [dbo].[Seat] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Color] int  NOT NULL,
    [SeatName] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'MediaDataSet'
CREATE TABLE [dbo].[MediaDataSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Data] varbinary(max)  NULL,
    [Attachment_Id] int  NULL
);
GO

-- Creating table 'TopicPerson'
CREATE TABLE [dbo].[TopicPerson] (
    [Topic_Id] int  NOT NULL,
    [Person_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Person'
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [PK_Person]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Discussion'
ALTER TABLE [dbo].[Discussion]
ADD CONSTRAINT [PK_Discussion]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ArgPoint'
ALTER TABLE [dbo].[ArgPoint]
ADD CONSTRAINT [PK_ArgPoint]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Topic'
ALTER TABLE [dbo].[Topic]
ADD CONSTRAINT [PK_Topic]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Comment'
ALTER TABLE [dbo].[Comment]
ADD CONSTRAINT [PK_Comment]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Attachment'
ALTER TABLE [dbo].[Attachment]
ADD CONSTRAINT [PK_Attachment]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GeneralSide'
ALTER TABLE [dbo].[GeneralSide]
ADD CONSTRAINT [PK_GeneralSide]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Source'
ALTER TABLE [dbo].[Source]
ADD CONSTRAINT [PK_Source]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'RichText'
ALTER TABLE [dbo].[RichText]
ADD CONSTRAINT [PK_RichText]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Annotation'
ALTER TABLE [dbo].[Annotation]
ADD CONSTRAINT [PK_Annotation]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'StatsEvent'
ALTER TABLE [dbo].[StatsEvent]
ADD CONSTRAINT [PK_StatsEvent]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Session'
ALTER TABLE [dbo].[Session]
ADD CONSTRAINT [PK_Session]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Seat'
ALTER TABLE [dbo].[Seat]
ADD CONSTRAINT [PK_Seat]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'MediaDataSet'
ALTER TABLE [dbo].[MediaDataSet]
ADD CONSTRAINT [PK_MediaDataSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Topic_Id], [Person_Id] in table 'TopicPerson'
ALTER TABLE [dbo].[TopicPerson]
ADD CONSTRAINT [PK_TopicPerson]
    PRIMARY KEY NONCLUSTERED ([Topic_Id], [Person_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Topic_Id] in table 'ArgPoint'
ALTER TABLE [dbo].[ArgPoint]
ADD CONSTRAINT [FK_TopicArgPoint]
    FOREIGN KEY ([Topic_Id])
    REFERENCES [dbo].[Topic]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TopicArgPoint'
CREATE INDEX [IX_FK_TopicArgPoint]
ON [dbo].[ArgPoint]
    ([Topic_Id]);
GO

-- Creating foreign key on [Discussion_Id] in table 'Topic'
ALTER TABLE [dbo].[Topic]
ADD CONSTRAINT [FK_DiscussionTopic]
    FOREIGN KEY ([Discussion_Id])
    REFERENCES [dbo].[Discussion]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DiscussionTopic'
CREATE INDEX [IX_FK_DiscussionTopic]
ON [dbo].[Topic]
    ([Discussion_Id]);
GO

-- Creating foreign key on [ArgPoint_Id] in table 'Comment'
ALTER TABLE [dbo].[Comment]
ADD CONSTRAINT [FK_ArgPointComment]
    FOREIGN KEY ([ArgPoint_Id])
    REFERENCES [dbo].[ArgPoint]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ArgPointComment'
CREATE INDEX [IX_FK_ArgPointComment]
ON [dbo].[Comment]
    ([ArgPoint_Id]);
GO

-- Creating foreign key on [ArgPoint_Id] in table 'Attachment'
ALTER TABLE [dbo].[Attachment]
ADD CONSTRAINT [FK_ArgPointAttachment]
    FOREIGN KEY ([ArgPoint_Id])
    REFERENCES [dbo].[ArgPoint]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ArgPointAttachment'
CREATE INDEX [IX_FK_ArgPointAttachment]
ON [dbo].[Attachment]
    ([ArgPoint_Id]);
GO

-- Creating foreign key on [Topic_Id] in table 'TopicPerson'
ALTER TABLE [dbo].[TopicPerson]
ADD CONSTRAINT [FK_TopicPerson_Topic]
    FOREIGN KEY ([Topic_Id])
    REFERENCES [dbo].[Topic]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Person_Id] in table 'TopicPerson'
ALTER TABLE [dbo].[TopicPerson]
ADD CONSTRAINT [FK_TopicPerson_Person]
    FOREIGN KEY ([Person_Id])
    REFERENCES [dbo].[Person]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TopicPerson_Person'
CREATE INDEX [IX_FK_TopicPerson_Person]
ON [dbo].[TopicPerson]
    ([Person_Id]);
GO

-- Creating foreign key on [Person_Id] in table 'ArgPoint'
ALTER TABLE [dbo].[ArgPoint]
ADD CONSTRAINT [FK_PersonArgPoint]
    FOREIGN KEY ([Person_Id])
    REFERENCES [dbo].[Person]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PersonArgPoint'
CREATE INDEX [IX_FK_PersonArgPoint]
ON [dbo].[ArgPoint]
    ([Person_Id]);
GO

-- Creating foreign key on [Person_Id] in table 'Comment'
ALTER TABLE [dbo].[Comment]
ADD CONSTRAINT [FK_PersonComment]
    FOREIGN KEY ([Person_Id])
    REFERENCES [dbo].[Person]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PersonComment'
CREATE INDEX [IX_FK_PersonComment]
ON [dbo].[Comment]
    ([Person_Id]);
GO

-- Creating foreign key on [Person_Id] in table 'Attachment'
ALTER TABLE [dbo].[Attachment]
ADD CONSTRAINT [FK_PersonAttachment]
    FOREIGN KEY ([Person_Id])
    REFERENCES [dbo].[Person]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PersonAttachment'
CREATE INDEX [IX_FK_PersonAttachment]
ON [dbo].[Attachment]
    ([Person_Id]);
GO

-- Creating foreign key on [Discussion_Id] in table 'Attachment'
ALTER TABLE [dbo].[Attachment]
ADD CONSTRAINT [FK_DiscussionAttachment]
    FOREIGN KEY ([Discussion_Id])
    REFERENCES [dbo].[Discussion]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DiscussionAttachment'
CREATE INDEX [IX_FK_DiscussionAttachment]
ON [dbo].[Attachment]
    ([Discussion_Id]);
GO

-- Creating foreign key on [Person_Id] in table 'GeneralSide'
ALTER TABLE [dbo].[GeneralSide]
ADD CONSTRAINT [FK_PersonGeneralSide]
    FOREIGN KEY ([Person_Id])
    REFERENCES [dbo].[Person]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PersonGeneralSide'
CREATE INDEX [IX_FK_PersonGeneralSide]
ON [dbo].[GeneralSide]
    ([Person_Id]);
GO

-- Creating foreign key on [Discussion_Id] in table 'GeneralSide'
ALTER TABLE [dbo].[GeneralSide]
ADD CONSTRAINT [FK_DiscussionGeneralSide]
    FOREIGN KEY ([Discussion_Id])
    REFERENCES [dbo].[Discussion]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DiscussionGeneralSide'
CREATE INDEX [IX_FK_DiscussionGeneralSide]
ON [dbo].[GeneralSide]
    ([Discussion_Id]);
GO

-- Creating foreign key on [RichText_Id] in table 'Source'
ALTER TABLE [dbo].[Source]
ADD CONSTRAINT [FK_RichTextSource]
    FOREIGN KEY ([RichText_Id])
    REFERENCES [dbo].[RichText]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RichTextSource'
CREATE INDEX [IX_FK_RichTextSource]
ON [dbo].[Source]
    ([RichText_Id]);
GO

-- Creating foreign key on [Discussion_Id] in table 'RichText'
ALTER TABLE [dbo].[RichText]
ADD CONSTRAINT [FK_DiscussionRichText]
    FOREIGN KEY ([Discussion_Id])
    REFERENCES [dbo].[Discussion]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DiscussionRichText'
CREATE INDEX [IX_FK_DiscussionRichText]
ON [dbo].[RichText]
    ([Discussion_Id]);
GO

-- Creating foreign key on [ArgPoint_Id] in table 'RichText'
ALTER TABLE [dbo].[RichText]
ADD CONSTRAINT [FK_ArgPointRichText]
    FOREIGN KEY ([ArgPoint_Id])
    REFERENCES [dbo].[ArgPoint]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ArgPointRichText'
CREATE INDEX [IX_FK_ArgPointRichText]
ON [dbo].[RichText]
    ([ArgPoint_Id]);
GO

-- Creating foreign key on [Person_Id] in table 'Annotation'
ALTER TABLE [dbo].[Annotation]
ADD CONSTRAINT [FK_PersonScreenshot]
    FOREIGN KEY ([Person_Id])
    REFERENCES [dbo].[Person]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PersonScreenshot'
CREATE INDEX [IX_FK_PersonScreenshot]
ON [dbo].[Annotation]
    ([Person_Id]);
GO

-- Creating foreign key on [Discussion_Id] in table 'Annotation'
ALTER TABLE [dbo].[Annotation]
ADD CONSTRAINT [FK_DiscussionAnnotation]
    FOREIGN KEY ([Discussion_Id])
    REFERENCES [dbo].[Discussion]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DiscussionAnnotation'
CREATE INDEX [IX_FK_DiscussionAnnotation]
ON [dbo].[Annotation]
    ([Discussion_Id]);
GO

-- Creating foreign key on [PersonWithAvatar_Id] in table 'Attachment'
ALTER TABLE [dbo].[Attachment]
ADD CONSTRAINT [FK_PersonAttachment1]
    FOREIGN KEY ([PersonWithAvatar_Id])
    REFERENCES [dbo].[Person]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PersonAttachment1'
CREATE INDEX [IX_FK_PersonAttachment1]
ON [dbo].[Attachment]
    ([PersonWithAvatar_Id]);
GO

-- Creating foreign key on [SeatId] in table 'Person'
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [FK_PersonSeat]
    FOREIGN KEY ([SeatId])
    REFERENCES [dbo].[Seat]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PersonSeat'
CREATE INDEX [IX_FK_PersonSeat]
ON [dbo].[Person]
    ([SeatId]);
GO

-- Creating foreign key on [SessionId] in table 'Person'
ALTER TABLE [dbo].[Person]
ADD CONSTRAINT [FK_PersonSession]
    FOREIGN KEY ([SessionId])
    REFERENCES [dbo].[Session]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PersonSession'
CREATE INDEX [IX_FK_PersonSession]
ON [dbo].[Person]
    ([SessionId]);
GO

-- Creating foreign key on [Attachment_Id] in table 'MediaDataSet'
ALTER TABLE [dbo].[MediaDataSet]
ADD CONSTRAINT [FK_AttachmentMediaData]
    FOREIGN KEY ([Attachment_Id])
    REFERENCES [dbo].[Attachment]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_AttachmentMediaData'
CREATE INDEX [IX_FK_AttachmentMediaData]
ON [dbo].[MediaDataSet]
    ([Attachment_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------