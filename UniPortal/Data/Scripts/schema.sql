USE [UniPortalDB];  -- Replace with your database name
GO

-- Drop table if it exists
IF OBJECT_ID('dbo.Accounts', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Accounts;
END
GO

-- Create Accounts table
CREATE TABLE [dbo].[Accounts] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [FirstName] NVARCHAR(100) NULL,
    [LastName] NVARCHAR(100) NULL,
    [DateOfBirth] DATETIME2 NULL,
    [Email] NVARCHAR(256) NOT NULL,
    [PhoneNumber] NVARCHAR(50) NULL,
    [Address] NVARCHAR(500) NULL,
    [IdentityUserId] NVARCHAR(450) NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 0,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NULL,
    [DeletedAt] DATETIME2 NULL,
    
    CONSTRAINT FK_Accounts_IdentityUser FOREIGN KEY (IdentityUserId)
        REFERENCES [dbo].[AspNetUsers] (Id)
        ON DELETE CASCADE
);
GO
