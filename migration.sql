IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Transactions] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TransactionType] nvarchar(max) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [QrImageUrl] nvarchar(max) NOT NULL,
    [BankInformation] nvarchar(max) NOT NULL,
    [Description] nvarchar(500) NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [LastUpdatedBy] nvarchar(max) NOT NULL,
    [ApprovedBy] nvarchar(max) NULL,
    [CreatedTime] datetimeoffset NOT NULL,
    [LastUpdatedTime] datetimeoffset NOT NULL,
    [ApprovedTime] datetimeoffset NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Transactions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Transactions_UserId] ON [Transactions] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250610013533_AddTransactionTable', N'9.0.2');

ALTER TABLE [Products] ADD [PurchasePrice] decimal(18,2) NOT NULL DEFAULT 0.0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250624025828_AddPurchasePriceToProduct', N'9.0.2');

ALTER TABLE [OrderDetails] DROP CONSTRAINT [FK_OrderDetails_ProductItems_ProductItemId];

ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_Users_UserId];

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'PurchasePrice');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Products] DROP COLUMN [PurchasePrice];

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vouchers]') AND [c].[name] = N'Status');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Vouchers] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Vouchers] ALTER COLUMN [Status] nvarchar(max) NULL;

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vouchers]') AND [c].[name] = N'Description');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Vouchers] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Vouchers] ALTER COLUMN [Description] nvarchar(max) NULL;

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vouchers]') AND [c].[name] = N'Code');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Vouchers] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Vouchers] ALTER COLUMN [Code] nvarchar(max) NULL;

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Variations]') AND [c].[name] = N'Name');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Variations] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Variations] ALTER COLUMN [Name] nvarchar(max) NULL;

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[VariationOptions]') AND [c].[name] = N'Value');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [VariationOptions] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [VariationOptions] ALTER COLUMN [Value] nvarchar(max) NULL;

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StatusChanges]') AND [c].[name] = N'Status');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [StatusChanges] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [StatusChanges] ALTER COLUMN [Status] nvarchar(max) NULL;

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SkinTypes]') AND [c].[name] = N'Name');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [SkinTypes] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [SkinTypes] ALTER COLUMN [Name] nvarchar(max) NULL;

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SkinTypes]') AND [c].[name] = N'Description');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [SkinTypes] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [SkinTypes] ALTER COLUMN [Description] nvarchar(max) NULL;

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Reviews]') AND [c].[name] = N'Comment');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Reviews] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [Reviews] ALTER COLUMN [Comment] nvarchar(max) NULL;

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ReviewImages]') AND [c].[name] = N'ImageUrl');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [ReviewImages] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [ReviewImages] ALTER COLUMN [ImageUrl] nvarchar(max) NULL;

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Replies]') AND [c].[name] = N'ReplyContent');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Replies] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [Replies] ALTER COLUMN [ReplyContent] nvarchar(max) NULL;

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[QuizSets]') AND [c].[name] = N'Name');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [QuizSets] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [QuizSets] ALTER COLUMN [Name] nvarchar(max) NULL;

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[QuizResults]') AND [c].[name] = N'Score');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [QuizResults] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [QuizResults] ALTER COLUMN [Score] nvarchar(max) NULL;

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[QuizQuestions]') AND [c].[name] = N'Value');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [QuizQuestions] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [QuizQuestions] ALTER COLUMN [Value] nvarchar(max) NULL;

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[QuizOptions]') AND [c].[name] = N'Value');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [QuizOptions] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [QuizOptions] ALTER COLUMN [Value] nvarchar(max) NULL;

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductStatuses]') AND [c].[name] = N'StatusName');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [ProductStatuses] DROP CONSTRAINT [' + @var16 + '];');
ALTER TABLE [ProductStatuses] ALTER COLUMN [StatusName] nvarchar(max) NULL;

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductStatuses]') AND [c].[name] = N'Description');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [ProductStatuses] DROP CONSTRAINT [' + @var17 + '];');
ALTER TABLE [ProductStatuses] ALTER COLUMN [Description] nvarchar(max) NULL;

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'UsageInstruction');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var18 + '];');
ALTER TABLE [Products] ALTER COLUMN [UsageInstruction] nvarchar(max) NULL;

DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Texture');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var19 + '];');
ALTER TABLE [Products] ALTER COLUMN [Texture] nvarchar(max) NULL;

DECLARE @var20 sysname;
SELECT @var20 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'StorageInstruction');
IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var20 + '];');
ALTER TABLE [Products] ALTER COLUMN [StorageInstruction] nvarchar(max) NULL;

DECLARE @var21 sysname;
SELECT @var21 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'SkinIssues');
IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var21 + '];');
ALTER TABLE [Products] ALTER COLUMN [SkinIssues] nvarchar(max) NULL;

DECLARE @var22 sysname;
SELECT @var22 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Name');
IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var22 + '];');
ALTER TABLE [Products] ALTER COLUMN [Name] nvarchar(max) NULL;

DECLARE @var23 sysname;
SELECT @var23 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'MainFunction');
IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var23 + '];');
ALTER TABLE [Products] ALTER COLUMN [MainFunction] nvarchar(max) NULL;

DECLARE @var24 sysname;
SELECT @var24 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'KeyActiveIngredients');
IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var24 + '];');
ALTER TABLE [Products] ALTER COLUMN [KeyActiveIngredients] nvarchar(max) NULL;

DECLARE @var25 sysname;
SELECT @var25 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'ExpiryDate');
IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var25 + '];');
ALTER TABLE [Products] ALTER COLUMN [ExpiryDate] nvarchar(max) NULL;

DECLARE @var26 sysname;
SELECT @var26 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'EnglishName');
IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var26 + '];');
ALTER TABLE [Products] ALTER COLUMN [EnglishName] nvarchar(max) NULL;

DECLARE @var27 sysname;
SELECT @var27 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'DetailedIngredients');
IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var27 + '];');
ALTER TABLE [Products] ALTER COLUMN [DetailedIngredients] nvarchar(max) NULL;

DECLARE @var28 sysname;
SELECT @var28 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'Description');
IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT [' + @var28 + '];');
ALTER TABLE [Products] ALTER COLUMN [Description] nvarchar(max) NULL;

DECLARE @var29 sysname;
SELECT @var29 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductItems]') AND [c].[name] = N'ImageUrl');
IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [ProductItems] DROP CONSTRAINT [' + @var29 + '];');
ALTER TABLE [ProductItems] ALTER COLUMN [ImageUrl] nvarchar(max) NULL;

ALTER TABLE [ProductItems] ADD [PurchasePrice] decimal(18,2) NOT NULL DEFAULT 0.0;

DECLARE @var30 sysname;
SELECT @var30 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductImages]') AND [c].[name] = N'ImageUrl');
IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [ProductImages] DROP CONSTRAINT [' + @var30 + '];');
ALTER TABLE [ProductImages] ALTER COLUMN [ImageUrl] nvarchar(max) NULL;

DECLARE @var31 sysname;
SELECT @var31 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductCategories]') AND [c].[name] = N'CategoryName');
IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [ProductCategories] DROP CONSTRAINT [' + @var31 + '];');
ALTER TABLE [ProductCategories] ALTER COLUMN [CategoryName] nvarchar(max) NULL;

DECLARE @var32 sysname;
SELECT @var32 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentMethods]') AND [c].[name] = N'PaymentType');
IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [PaymentMethods] DROP CONSTRAINT [' + @var32 + '];');
ALTER TABLE [PaymentMethods] ALTER COLUMN [PaymentType] nvarchar(max) NULL;

DECLARE @var33 sysname;
SELECT @var33 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentMethods]') AND [c].[name] = N'ImageUrl');
IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [PaymentMethods] DROP CONSTRAINT [' + @var33 + '];');
ALTER TABLE [PaymentMethods] ALTER COLUMN [ImageUrl] nvarchar(max) NULL;

DECLARE @var34 sysname;
SELECT @var34 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Orders]') AND [c].[name] = N'Status');
IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT [' + @var34 + '];');
ALTER TABLE [Orders] ALTER COLUMN [Status] nvarchar(max) NULL;

DECLARE @var35 sysname;
SELECT @var35 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Countries]') AND [c].[name] = N'CountryName');
IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [Countries] DROP CONSTRAINT [' + @var35 + '];');
ALTER TABLE [Countries] ALTER COLUMN [CountryName] nvarchar(max) NULL;

DECLARE @var36 sysname;
SELECT @var36 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Countries]') AND [c].[name] = N'CountryCode');
IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [Countries] DROP CONSTRAINT [' + @var36 + '];');
ALTER TABLE [Countries] ALTER COLUMN [CountryCode] nvarchar(max) NULL;

DECLARE @var37 sysname;
SELECT @var37 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CartItems]') AND [c].[name] = N'Quantity');
IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [CartItems] DROP CONSTRAINT [' + @var37 + '];');

DECLARE @var38 sysname;
SELECT @var38 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CancelReasons]') AND [c].[name] = N'RefundRate');
IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [CancelReasons] DROP CONSTRAINT [' + @var38 + '];');
ALTER TABLE [CancelReasons] ALTER COLUMN [RefundRate] decimal(18,2) NOT NULL;

DECLARE @var39 sysname;
SELECT @var39 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CancelReasons]') AND [c].[name] = N'Description');
IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [CancelReasons] DROP CONSTRAINT [' + @var39 + '];');
ALTER TABLE [CancelReasons] ALTER COLUMN [Description] nvarchar(max) NULL;

DECLARE @var40 sysname;
SELECT @var40 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Brands]') AND [c].[name] = N'Title');
IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [Brands] DROP CONSTRAINT [' + @var40 + '];');
ALTER TABLE [Brands] ALTER COLUMN [Title] nvarchar(max) NULL;

DECLARE @var41 sysname;
SELECT @var41 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Brands]') AND [c].[name] = N'Name');
IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [Brands] DROP CONSTRAINT [' + @var41 + '];');
ALTER TABLE [Brands] ALTER COLUMN [Name] nvarchar(max) NULL;

DECLARE @var42 sysname;
SELECT @var42 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Brands]') AND [c].[name] = N'ImageUrl');
IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [Brands] DROP CONSTRAINT [' + @var42 + '];');
ALTER TABLE [Brands] ALTER COLUMN [ImageUrl] nvarchar(max) NULL;

DECLARE @var43 sysname;
SELECT @var43 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Brands]') AND [c].[name] = N'Description');
IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [Brands] DROP CONSTRAINT [' + @var43 + '];');
ALTER TABLE [Brands] ALTER COLUMN [Description] nvarchar(max) NULL;

DECLARE @var44 sysname;
SELECT @var44 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Blogs]') AND [c].[name] = N'Title');
IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [Blogs] DROP CONSTRAINT [' + @var44 + '];');
ALTER TABLE [Blogs] ALTER COLUMN [Title] nvarchar(max) NULL;

DECLARE @var45 sysname;
SELECT @var45 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Blogs]') AND [c].[name] = N'Thumbnail');
IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [Blogs] DROP CONSTRAINT [' + @var45 + '];');
ALTER TABLE [Blogs] ALTER COLUMN [Thumbnail] nvarchar(max) NULL;

DECLARE @var46 sysname;
SELECT @var46 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Blogs]') AND [c].[name] = N'LastUpdatedBy');
IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [Blogs] DROP CONSTRAINT [' + @var46 + '];');
ALTER TABLE [Blogs] ALTER COLUMN [LastUpdatedBy] nvarchar(max) NULL;

DECLARE @var47 sysname;
SELECT @var47 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Blogs]') AND [c].[name] = N'Description');
IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [Blogs] DROP CONSTRAINT [' + @var47 + '];');
ALTER TABLE [Blogs] ALTER COLUMN [Description] nvarchar(max) NULL;

DECLARE @var48 sysname;
SELECT @var48 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Blogs]') AND [c].[name] = N'DeletedBy');
IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [Blogs] DROP CONSTRAINT [' + @var48 + '];');
ALTER TABLE [Blogs] ALTER COLUMN [DeletedBy] nvarchar(max) NULL;

DECLARE @var49 sysname;
SELECT @var49 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Blogs]') AND [c].[name] = N'CreatedBy');
IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [Blogs] DROP CONSTRAINT [' + @var49 + '];');
ALTER TABLE [Blogs] ALTER COLUMN [CreatedBy] nvarchar(max) NULL;

CREATE TABLE [ChatHistories] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [MessageContent] nvarchar(max) NOT NULL,
    [SenderType] nvarchar(10) NOT NULL,
    [Timestamp] datetimeoffset NOT NULL,
    [SessionId] nvarchar(100) NOT NULL,
    [CreatedBy] nvarchar(100) NOT NULL,
    [LastUpdatedBy] nvarchar(100) NOT NULL,
    [DeletedBy] nvarchar(100) NOT NULL,
    [CreatedTime] datetimeoffset NULL,
    [LastUpdatedTime] datetimeoffset NULL,
    [DeletedTime] datetimeoffset NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ChatHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatHistories_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE INDEX [IX_ChatHistories_SessionId] ON [ChatHistories] ([SessionId]);

CREATE INDEX [IX_ChatHistories_UserId] ON [ChatHistories] ([UserId]);

ALTER TABLE [OrderDetails] ADD CONSTRAINT [FK_OrderDetails_ProductItems_ProductItemId] FOREIGN KEY ([ProductItemId]) REFERENCES [ProductItems] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250701132253_AddChatHistoryTable', N'9.0.2');

COMMIT;
GO

