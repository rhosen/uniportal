USE [UniPortalDB];  -- Replace with your database name
GO

-- =========================
-- Accounts Table
-- =========================
IF OBJECT_ID('dbo.Accounts', 'U') IS NOT NULL
    DROP TABLE dbo.Accounts;
GO

CREATE TABLE dbo.Accounts (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    FirstName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NULL,
    DateOfBirth DATETIME2 NULL,
    Email NVARCHAR(256) NOT NULL,
    PhoneNumber NVARCHAR(50) NULL,
    Address NVARCHAR(500) NULL,
    IdentityUserId NVARCHAR(450) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Accounts_IdentityUser FOREIGN KEY (IdentityUserId)
        REFERENCES dbo.AspNetUsers(Id)
        ON DELETE CASCADE
);
GO

-- =========================
-- Departments Table
-- =========================
IF OBJECT_ID('dbo.Departments', 'U') IS NOT NULL
    DROP TABLE dbo.Departments;
GO

CREATE TABLE dbo.Departments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    HeadId UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Departments_Head FOREIGN KEY (HeadId)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Semesters Table
-- =========================
IF OBJECT_ID('dbo.Semesters', 'U') IS NOT NULL
    DROP TABLE dbo.Semesters;
GO

CREATE TABLE dbo.Semesters (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- Courses Table
-- =========================
IF OBJECT_ID('dbo.Courses', 'U') IS NOT NULL
    DROP TABLE dbo.Courses;
GO

CREATE TABLE dbo.Courses (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    DepartmentId UNIQUEIDENTIFIER NOT NULL,
    TeacherId UNIQUEIDENTIFIER NOT NULL,
    SemesterId UNIQUEIDENTIFIER NOT NULL,
    Credits INT NOT NULL DEFAULT 3,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Courses_Department FOREIGN KEY (DepartmentId)
        REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_Courses_Teacher FOREIGN KEY (TeacherId)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Courses_Semester FOREIGN KEY (SemesterId)
        REFERENCES dbo.Semesters(Id)
);
GO

-- =========================
-- Classrooms Table
-- =========================
IF OBJECT_ID('dbo.Classrooms', 'U') IS NOT NULL
    DROP TABLE dbo.Classrooms;
GO

CREATE TABLE dbo.Classrooms (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    RoomName NVARCHAR(50) NOT NULL,
    Capacity INT NOT NULL DEFAULT 30,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO


-- =========================
-- Attachments Table
-- =========================
IF OBJECT_ID('dbo.Attachments', 'U') IS NOT NULL
    DROP TABLE dbo.Attachments;
GO

CREATE TABLE dbo.Attachments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,       -- physical path or URL
    FileType NVARCHAR(50) NULL,            -- PDF, DOCX, MP4, etc.
    UploadedBy UNIQUEIDENTIFIER NOT NULL,  -- Teacher or Admin
    RelatedEntity NVARCHAR(50) NULL,       -- e.g., 'ClassNote', 'Assignment'
    RelatedEntityId UNIQUEIDENTIFIER NULL, -- Id of the related entity
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Attachments_User FOREIGN KEY (UploadedBy)
        REFERENCES Accounts(Id)
);
GO

-- =========================
-- ClassNotes Table
-- =========================
IF OBJECT_ID('dbo.ClassNotes', 'U') IS NOT NULL
    DROP TABLE dbo.ClassNotes;
GO

CREATE TABLE dbo.ClassNotes (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CourseId UNIQUEIDENTIFIER NOT NULL,
    TeacherId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    
    CONSTRAINT FK_ClassNotes_Course FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id),
    CONSTRAINT FK_ClassNotes_Teacher FOREIGN KEY (TeacherId)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Classrooms Table
-- =========================
IF OBJECT_ID('dbo.Classrooms', 'U') IS NOT NULL
    DROP TABLE dbo.Classrooms;
GO

CREATE TABLE dbo.Classrooms (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    RoomName NVARCHAR(50) NOT NULL,
    Capacity INT NOT NULL DEFAULT 30,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- Assignments Table
-- =========================
IF OBJECT_ID('dbo.Assignments', 'U') IS NOT NULL
    DROP TABLE dbo.Assignments;
GO

CREATE TABLE dbo.Assignments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    CourseId UNIQUEIDENTIFIER NOT NULL,
    AssignedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    DueDate DATETIME2 NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Assignments_Course FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id)
);
GO

-- =========================
-- Enrollments Table
-- =========================
IF OBJECT_ID('dbo.Enrollments', 'U') IS NOT NULL
    DROP TABLE dbo.Enrollments;
GO

CREATE TABLE dbo.Enrollments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    StudentId UNIQUEIDENTIFIER NOT NULL,
    CourseId UNIQUEIDENTIFIER NOT NULL,
    SemesterId UNIQUEIDENTIFIER NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    EnrollmentDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    CONSTRAINT FK_Enrollments_Student FOREIGN KEY (StudentId)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Enrollments_Course FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id),
    CONSTRAINT FK_Enrollments_Semester FOREIGN KEY (SemesterId)
        REFERENCES dbo.Semesters(Id),
    CONSTRAINT UQ_Enrollment UNIQUE(StudentId, CourseId, SemesterId)
);
GO

-- =========================
-- Grades Table
-- =========================
IF OBJECT_ID('dbo.Grades', 'U') IS NOT NULL
    DROP TABLE dbo.Grades;
GO

CREATE TABLE dbo.Grades (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    StudentId UNIQUEIDENTIFIER NOT NULL,
    CourseId UNIQUEIDENTIFIER NOT NULL,
    SemesterId UNIQUEIDENTIFIER NOT NULL,
    Grade NVARCHAR(5) NULL,
    Marks DECIMAL(5,2) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    CONSTRAINT FK_Grades_Student FOREIGN KEY (StudentId)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Grades_Course FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id),
    CONSTRAINT FK_Grades_Semester FOREIGN KEY (SemesterId)
        REFERENCES dbo.Semesters(Id),
    CONSTRAINT UQ_Grade UNIQUE(StudentId, CourseId, SemesterId)
);
GO

-- =========================
-- AssignmentSubmissions Table
-- =========================
IF OBJECT_ID('dbo.AssignmentSubmissions', 'U') IS NOT NULL
    DROP TABLE dbo.AssignmentSubmissions;
GO

CREATE TABLE dbo.AssignmentSubmissions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    AssignmentId UNIQUEIDENTIFIER NOT NULL,
    StudentId UNIQUEIDENTIFIER NOT NULL,
    SubmittedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    MarksAwarded DECIMAL(5,2) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Submissions_Assignment FOREIGN KEY (AssignmentId)
        REFERENCES dbo.Assignments(Id),
    CONSTRAINT FK_Submissions_Student FOREIGN KEY (StudentId)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT UQ_Submission UNIQUE(AssignmentId, StudentId)
);
GO

-- =========================
-- Logs Table
-- =========================
IF OBJECT_ID('dbo.Logs', 'U') IS NOT NULL
    DROP TABLE dbo.Logs;
GO

CREATE TABLE dbo.Logs (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    AccountId UNIQUEIDENTIFIER NULL,
    Action NVARCHAR(200) NOT NULL,
    Entity NVARCHAR(100) NULL,
    EntityId UNIQUEIDENTIFIER NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    CONSTRAINT FK_Logs_Account FOREIGN KEY (AccountId)
        REFERENCES dbo.Accounts(Id)
);
GO


-- =========================
-- NotificationTypes Table
-- =========================
CREATE TABLE NotificationTypes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,      -- e.g., 'Student', 'Department', 'All'
    Description NVARCHAR(200) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- Notifications Table
-- =========================
IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL
    DROP TABLE dbo.Notifications;
GO

-- 2️⃣ Notifications table
CREATE TABLE dbo.Notifications (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,          -- Admin or Teacher
    NotificationTypeId INT NOT NULL,             -- FK to NotificationTypes
    TargetId UNIQUEIDENTIFIER NULL,              -- StudentId, DepartmentId, or NULL for All
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Notifications_CreatedBy FOREIGN KEY (CreatedBy)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Notifications_NotificationType FOREIGN KEY (NotificationTypeId)
        REFERENCES dbo.NotificationTypes(Id)
);
GO

-- =========================
-- ClassSchedules / Timetable Table
-- =========================
IF OBJECT_ID('dbo.ClassSchedules', 'U') IS NOT NULL
    DROP TABLE dbo.ClassSchedules;
GO

CREATE TABLE dbo.ClassSchedules (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CourseId UNIQUEIDENTIFIER NOT NULL,
    ClassroomId UNIQUEIDENTIFIER NOT NULL,
    TeacherId UNIQUEIDENTIFIER NOT NULL,
    SemesterId UNIQUEIDENTIFIER NOT NULL,
    DayOfWeek INT NOT NULL,  -- 1 = Monday, 2 = Tuesday, etc.
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_ClassSchedules_Course FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id),
    CONSTRAINT FK_ClassSchedules_Classroom FOREIGN KEY (ClassroomId)
        REFERENCES dbo.Classrooms(Id),
    CONSTRAINT FK_ClassSchedules_Teacher FOREIGN KEY (TeacherId)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_ClassSchedules_Semester FOREIGN KEY (SemesterId)
        REFERENCES dbo.Semesters(Id)
);
GO


-- =========================
-- Attendance Table
-- =========================
IF OBJECT_ID('dbo.Attendance', 'U') IS NOT NULL
    DROP TABLE dbo.Attendance;
GO

CREATE TABLE dbo.Attendance (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    StudentId UNIQUEIDENTIFIER NOT NULL,
    ClassScheduleId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Absent',
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    RecordedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    CONSTRAINT FK_Attendance_Student FOREIGN KEY (StudentId)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Attendance_ClassSchedule FOREIGN KEY (ClassScheduleId)
        REFERENCES dbo.ClassSchedules(Id)
);
GO