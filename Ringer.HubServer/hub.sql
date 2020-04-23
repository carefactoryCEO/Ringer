Build started...
Build succeeded.
[40m[32minfo[39m[22m[49m: Microsoft.EntityFrameworkCore.Infrastructure[10403]
      Entity Framework Core 3.1.3 initialized 'RingerDbContext' using provider 'Microsoft.EntityFrameworkCore.SqlServer' with options: None
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

CREATE TABLE [Consulate] (
    [Id] int NOT NULL IDENTITY,
    [ConsulateType] nvarchar(max) NULL,
    [CountryName] nvarchar(max) NULL,
    [KoreanName] nvarchar(max) NULL,
    [LocalName] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [EmergencyPhoneNumber] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [Homepage] nvarchar(max) NULL,
    [Latitude] float NOT NULL,
    [Longitude] float NOT NULL,
    [GoogleMap] nvarchar(max) NULL,
    CONSTRAINT [PK_Consulate] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [Room] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(max) NULL,
    [IsClosed] bit NOT NULL,
    CONSTRAINT [PK_Room] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [User] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NULL,
    [UserType] nvarchar(max) NOT NULL DEFAULT N'Consumer',
    [BirthDate] datetime2 NOT NULL,
    [Gender] nvarchar(max) NOT NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Password] nvarchar(max) NULL,
    [PasswordHash] varbinary(max) NULL,
    [PasswordSalt] varbinary(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [Device] (
    [Id] nvarchar(450) NOT NULL,
    [DeviceType] nvarchar(max) NOT NULL,
    [IsOn] bit NOT NULL,
    [ConnectionId] nvarchar(max) NULL,
    [OwnerId] int NOT NULL,
    CONSTRAINT [PK_Device] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Device_User_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Enrollment] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [RoomId] nvarchar(450) NULL,
    CONSTRAINT [PK_Enrollment] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Enrollment_Room_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Room] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Enrollment_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Message] (
    [Id] int NOT NULL IDENTITY,
    [Body] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [SenderId] int NOT NULL,
    [RoomId] nvarchar(450) NULL,
    CONSTRAINT [PK_Message] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Message_Room_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Room] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Message_User_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BirthDate', N'CreatedAt', N'Email', N'Gender', N'Name', N'Password', N'PasswordHash', N'PasswordSalt', N'PhoneNumber', N'UpdatedAt', N'UserType') AND [object_id] = OBJECT_ID(N'[User]'))
    SET IDENTITY_INSERT [User] ON;
INSERT INTO [User] ([Id], [BirthDate], [CreatedAt], [Email], [Gender], [Name], [Password], [PasswordHash], [PasswordSalt], [PhoneNumber], [UpdatedAt], [UserType])
VALUES (1, '1976-07-21T00:00:00.0000000', '2020-02-28T13:46:50.0945620Z', NULL, N'Male', N'Admin', NULL, NULL, NULL, NULL, '0001-01-01T00:00:00.0000000', N'Admin'),
(2, '1976-07-21T00:00:00.0000000', '2020-02-28T13:46:50.0946360Z', NULL, N'Male', N'신모범', NULL, NULL, NULL, NULL, '0001-01-01T00:00:00.0000000', N'Consumer'),
(3, '1981-06-25T00:00:00.0000000', '2020-02-28T13:46:50.0946370Z', NULL, N'Female', N'김은미', NULL, NULL, NULL, NULL, '0001-01-01T00:00:00.0000000', N'Consumer'),
(4, '1980-07-04T00:00:00.0000000', '2020-02-28T13:46:50.0946380Z', NULL, N'Male', N'김순용', NULL, NULL, NULL, NULL, '0001-01-01T00:00:00.0000000', N'Consumer'),
(5, '1981-12-25T00:00:00.0000000', '2020-02-28T13:46:50.0946380Z', NULL, N'Female', N'함주희', NULL, NULL, NULL, NULL, '0001-01-01T00:00:00.0000000', N'Consumer');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BirthDate', N'CreatedAt', N'Email', N'Gender', N'Name', N'Password', N'PasswordHash', N'PasswordSalt', N'PhoneNumber', N'UpdatedAt', N'UserType') AND [object_id] = OBJECT_ID(N'[User]'))
    SET IDENTITY_INSERT [User] OFF;

GO

CREATE INDEX [IX_Device_OwnerId] ON [Device] ([OwnerId]);

GO

CREATE INDEX [IX_Enrollment_RoomId] ON [Enrollment] ([RoomId]);

GO

CREATE INDEX [IX_Enrollment_UserId] ON [Enrollment] ([UserId]);

GO

CREATE INDEX [IX_Message_RoomId] ON [Message] ([RoomId]);

GO

CREATE INDEX [IX_Message_SenderId] ON [Message] ([SenderId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200228134650_Initial-Create', N'3.1.3');

GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Consulate]') AND [c].[name] = N'CountryName');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Consulate] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Consulate] DROP COLUMN [CountryName];

GO

ALTER TABLE [Consulate] ADD [Country] nvarchar(max) NULL;

GO

UPDATE [User] SET [CreatedAt] = '2020-02-28T16:59:17.1483200Z'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-02-28T16:59:17.1483980Z'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-02-28T16:59:17.1483990Z'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-02-28T16:59:17.1484000Z'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-02-28T16:59:17.1484000Z'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200228165917_modify-contryname-to-country', N'3.1.3');

GO

ALTER TABLE [Consulate] ADD [CountryCode] nvarchar(450) NULL;

GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:13:38.1095680Z', [UserType] = N'Admin'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:13:38.1096520Z'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:13:38.1096540Z'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:13:38.1096550Z'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:13:38.1096550Z'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


GO

CREATE INDEX [IX_Consulate_CountryCode] ON [Consulate] ([CountryCode]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200325011338_Add-countryCode-to-Consulates-table', N'3.1.3');

GO

ALTER TABLE [Consulate] ADD [CountryCodeAndroid] nvarchar(max) NULL;

GO

ALTER TABLE [Consulate] ADD [CountryCodeiOS] nvarchar(max) NULL;

GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:26:31.4726950Z', [UserType] = N'Admin'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:26:31.4727810Z'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:26:31.4727830Z'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:26:31.4727840Z'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:26:31.4727840Z'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200325012631_Add-countryCodeiOS-and-countryCodeAndroid-to-Consulates-table', N'3.1.3');

GO

DROP INDEX [IX_Consulate_CountryCode] ON [Consulate];

GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Consulate]') AND [c].[name] = N'CountryCodeiOS');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Consulate] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Consulate] ALTER COLUMN [CountryCodeiOS] nvarchar(450) NULL;

GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Consulate]') AND [c].[name] = N'CountryCodeAndroid');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Consulate] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Consulate] ALTER COLUMN [CountryCodeAndroid] nvarchar(450) NULL;

GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:29:49.5206100Z', [UserType] = N'Admin'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:29:49.5206950Z'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:29:49.5206970Z'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:29:49.5206970Z'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-03-25T01:29:49.5206980Z'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


GO

CREATE INDEX [IX_Consulate_CountryCode_CountryCodeiOS_CountryCodeAndroid] ON [Consulate] ([CountryCode], [CountryCodeiOS], [CountryCodeAndroid]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200325012949_Add-index-on-countryCodeiOS-and-countryCodeAndroid-to-Consulates-table', N'3.1.3');

GO

CREATE TABLE [FootPrint] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Latitude] float NOT NULL,
    [Longitude] float NOT NULL,
    [Address] nvarchar(max) NULL,
    [CountryCode] nvarchar(max) NULL,
    [TimeStamp] datetime2 NOT NULL,
    CONSTRAINT [PK_FootPrint] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FootPrint_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);

GO

UPDATE [User] SET [CreatedAt] = '2020-04-21T07:24:09.7265870Z', [UserType] = N'Admin'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-04-21T07:24:09.7266630Z'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-04-21T07:24:09.7266640Z'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-04-21T07:24:09.7266640Z'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-04-21T07:24:09.7266650Z'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


GO

CREATE INDEX [IX_FootPrint_UserId] ON [FootPrint] ([UserId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200421072410_add-FootPrint', N'3.1.3');

GO

ALTER TABLE [Enrollment] ADD [EnrolledAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

GO

ALTER TABLE [Device] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);

GO

CREATE TABLE [Terms] (
    [Id] int NOT NULL IDENTITY,
    [IsCurrent] bit NOT NULL,
    [Title] nvarchar(max) NULL,
    [Body] nvarchar(max) NULL,
    [Version] nvarchar(max) NULL,
    [Type] int NOT NULL,
    [CreaetedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Terms] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [Agreement] (
    [Id] int NOT NULL IDENTITY,
    [AgreedAt] datetime2 NOT NULL,
    [TermsId] int NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_Agreement] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Agreement_Terms_TermsId] FOREIGN KEY ([TermsId]) REFERENCES [Terms] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Agreement_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);

GO

UPDATE [User] SET [CreatedAt] = '2020-04-22T09:10:59.4710010Z', [UserType] = N'Admin'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-04-22T09:10:59.4710810Z'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-04-22T09:10:59.4710830Z'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-04-22T09:10:59.4710830Z'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-04-22T09:10:59.4710840Z'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


GO

CREATE INDEX [IX_Agreement_TermsId] ON [Agreement] ([TermsId]);

GO

CREATE INDEX [IX_Agreement_UserId] ON [Agreement] ([UserId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200422091059_Add-Terms', N'3.1.3');

GO

ALTER TABLE [Terms] ADD [Required] bit NOT NULL DEFAULT CAST(0 AS bit);

GO

UPDATE [User] SET [CreatedAt] = '2020-04-22T10:33:20.3622820Z', [UserType] = N'Admin'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-04-22T10:33:20.3623680Z'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-04-22T10:33:20.3623700Z'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-04-22T10:33:20.3623700Z'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


GO

UPDATE [User] SET [CreatedAt] = '2020-04-22T10:33:20.3623710Z'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200422103320_Add-Required-Column-To-Terms-Table', N'3.1.3');

GO


