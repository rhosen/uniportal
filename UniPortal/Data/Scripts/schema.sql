USE [UniPortalDB];
GO

-- =========================
-- Accounts Table
-- =========================
IF OBJECT_ID('dbo.Accounts', 'U') IS NOT NULL
    DROP TABLE dbo.Accounts;
GO

CREATE TABLE dbo.Accounts
(
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    FirstName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NULL,
    DateOfBirth DATETIME2 NULL,
    Email NVARCHAR(256) NOT NULL,
    Phone NVARCHAR(50) NULL,
    Address NVARCHAR(500) NULL,
    IdentityId NVARCHAR(450) NOT NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsActive BIT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Accounts_AspNetUsers FOREIGN KEY (IdentityId)
        REFERENCES dbo.AspNetUsers(Id)
        ON DELETE CASCADE,

    CONSTRAINT FK_Accounts_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
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
    Code NVARCHAR(200) NOT NULL,
    Name NVARCHAR(500) NULL,
    HeadId UNIQUEIDENTIFIER NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Departments_Accounts_Head FOREIGN KEY (HeadId)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Departments_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Students Table
-- =========================
IF OBJECT_ID('dbo.Students', 'U') IS NOT NULL
    DROP TABLE dbo.Students;
GO

CREATE TABLE dbo.Students
(
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    AccountId UNIQUEIDENTIFIER NOT NULL,
    StudentId NVARCHAR(100) NULL,
    BatchNumber NVARCHAR(50) NULL,
    Section NVARCHAR(50) NULL,
    DepartmentId UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
   
    CONSTRAINT FK_Students_Accounts FOREIGN KEY (AccountId)
        REFERENCES dbo.Accounts(Id)
        ON DELETE CASCADE,
    CONSTRAINT FK_Students_Departments FOREIGN KEY (DepartmentId)
        REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_Students_Accounts_Modified FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT UQ_Students_StudentId UNIQUE (StudentId)
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
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Semesters_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Subjects Table
-- =========================
IF OBJECT_ID('dbo.Subjects', 'U') IS NOT NULL
    DROP TABLE dbo.Subjects;
GO

CREATE TABLE dbo.Subjects (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Subjects_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
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
    SubjectId UNIQUEIDENTIFIER NOT NULL,
    DepartmentId UNIQUEIDENTIFIER NOT NULL,
    TeacherId UNIQUEIDENTIFIER NOT NULL,
    SemesterId UNIQUEIDENTIFIER NOT NULL,
    Credits INT NOT NULL DEFAULT 3,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Courses_Subjects FOREIGN KEY (SubjectId)
        REFERENCES dbo.Subjects(Id),
    CONSTRAINT FK_Courses_Departments FOREIGN KEY (DepartmentId)
        REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_Courses_Accounts_Teacher FOREIGN KEY (TeacherId)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Courses_Semesters FOREIGN KEY (SemesterId)
        REFERENCES dbo.Semesters(Id),
    CONSTRAINT FK_Courses_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Rooms Table
-- =========================
IF OBJECT_ID('dbo.Rooms', 'U') IS NOT NULL
    DROP TABLE dbo.Rooms;
GO

CREATE TABLE dbo.Rooms (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    RoomName NVARCHAR(50) NOT NULL,
    Capacity INT NOT NULL DEFAULT 30,
    Location NVARCHAR(100) NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Rooms_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Files Table
-- =========================
IF OBJECT_ID('dbo.Files', 'U') IS NOT NULL
    DROP TABLE dbo.Files;
GO

CREATE TABLE dbo.Files (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FileType NVARCHAR(50) NULL,
    UploadedBy UNIQUEIDENTIFIER NOT NULL,
    RelatedEntity NVARCHAR(50) NULL,
    RelatedEntityId UNIQUEIDENTIFIER NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Files_Accounts FOREIGN KEY (UploadedBy)
        REFERENCES Accounts(Id),
    CONSTRAINT FK_Files_Accounts_Modified FOREIGN KEY (ModifiedById)
        REFERENCES Accounts(Id)
);
GO

-- =========================
-- Notes Table
-- =========================
IF OBJECT_ID('dbo.Notes', 'U') IS NOT NULL
    DROP TABLE dbo.Notes;
GO

CREATE TABLE dbo.Notes (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CourseId UNIQUEIDENTIFIER NOT NULL,
    TeacherId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Notes_Courses FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id),
    CONSTRAINT FK_Notes_Accounts_Teacher FOREIGN KEY (TeacherId)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Notes_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
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
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Assignments_Courses FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id),
    CONSTRAINT FK_Assignments_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
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
    ModifiedById UNIQUEIDENTIFIER NULL,
    
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Enrollments_Students FOREIGN KEY (StudentId)
        REFERENCES dbo.Students(Id),
    
    CONSTRAINT FK_Enrollments_Courses FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id),
    
    CONSTRAINT FK_Enrollments_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
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
    
    GradeValue NVARCHAR(5) NOT NULL,
    Marks DECIMAL(5,2) NOT NULL,
    GPA DECIMAL(3,2) NOT NULL,   
    ModifiedById UNIQUEIDENTIFIER NULL,

    
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Grades_Students FOREIGN KEY (StudentId)
        REFERENCES dbo.Students(Id),
    
    CONSTRAINT FK_Grades_Courses FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id),
    
    CONSTRAINT FK_Grades_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id),
    
    CONSTRAINT UQ_Grade UNIQUE(StudentId, CourseId)
);
GO

IF OBJECT_ID('dbo.GradeScales', 'U') IS NOT NULL
    DROP TABLE dbo.GradeScales;
GO

CREATE TABLE dbo.GradeScales (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    
    Grade NVARCHAR(5) NOT NULL,         -- e.g., A+, A, B+
    MinMarks DECIMAL(5,2) NOT NULL,     -- minimum marks for this grade
    MaxMarks DECIMAL(5,2) NOT NULL,     -- maximum marks for this grade
    GPA DECIMAL(3,2) NOT NULL,          -- grade points for GPA calculation

    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT UQ_GradeScale_Grade UNIQUE(Grade)
);
GO


-- =========================
-- Submissions Table
-- =========================
IF OBJECT_ID('dbo.Submissions', 'U') IS NOT NULL
    DROP TABLE dbo.Submissions;
GO

CREATE TABLE dbo.Submissions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    AssignmentId UNIQUEIDENTIFIER NOT NULL,
    StudentId UNIQUEIDENTIFIER NOT NULL,
    SubmittedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    MarksAwarded DECIMAL(5,2) NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,

    CONSTRAINT FK_Submissions_Assignments FOREIGN KEY (AssignmentId)
        REFERENCES dbo.Assignments(Id),
    CONSTRAINT FK_Submissions_Students FOREIGN KEY (StudentId)
        REFERENCES dbo.Students(Id),
    CONSTRAINT FK_Submissions_Accounts FOREIGN KEY (ModifiedById)
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
    ActionType NVARCHAR(50) NOT NULL,
    Action NVARCHAR(200) NOT NULL,
    Entity NVARCHAR(100) NULL,
    EntityId UNIQUEIDENTIFIER NULL,
    Details NVARCHAR(MAX) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Logs_Accounts FOREIGN KEY (AccountId)
        REFERENCES dbo.Accounts(Id)
);
CREATE INDEX IX_Logs_AccountId ON dbo.Logs(AccountId);
CREATE INDEX IX_Logs_Entity_EntityId ON dbo.Logs(Entity, EntityId);
CREATE INDEX IX_Logs_Timestamp ON dbo.Logs(Timestamp);
GO

-- =========================
-- Recipients Table
-- =========================
IF OBJECT_ID('dbo.Recipients', 'U') IS NOT NULL
    DROP TABLE dbo.Recipients;
GO

CREATE TABLE dbo.Recipients (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200) NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Recipients_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Notices Table
-- =========================
IF OBJECT_ID('dbo.Notices', 'U') IS NOT NULL
    DROP TABLE dbo.Notices;
GO

CREATE TABLE dbo.Notices (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    ModifiedById UNIQUEIDENTIFIER NOT NULL,
    RecipientId UNIQUEIDENTIFIER NOT NULL,
    StudentId NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Notices_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Notices_Recipients FOREIGN KEY (RecipientId)
        REFERENCES dbo.Recipients(Id),
    CONSTRAINT FK_Notices_Students FOREIGN KEY (StudentId)
        REFERENCES dbo.Students(StudentId)
);
GO

-- =========================
-- Schedules Table
-- =========================
IF OBJECT_ID('dbo.Schedules', 'U') IS NOT NULL
    DROP TABLE dbo.Schedules;
GO

CREATE TABLE dbo.Schedules (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CourseId UNIQUEIDENTIFIER NOT NULL,
    RoomId UNIQUEIDENTIFIER NOT NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Schedules_Courses FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id),
    CONSTRAINT FK_Schedules_Rooms FOREIGN KEY (RoomId)
        REFERENCES dbo.Rooms(Id),
    CONSTRAINT FK_Schedules_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Sessions Table
-- =========================
IF OBJECT_ID('dbo.Sessions', 'U') IS NOT NULL
    DROP TABLE dbo.Sessions;
GO

CREATE TABLE dbo.Sessions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ScheduleId UNIQUEIDENTIFIER NOT NULL,
    DayOfWeek INT NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Sessions_Schedules FOREIGN KEY (ScheduleId)
        REFERENCES dbo.Schedules(Id),
    CONSTRAINT FK_Sessions_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Cancellations Table
-- =========================
IF OBJECT_ID('dbo.Cancellations', 'U') IS NOT NULL
    DROP TABLE dbo.Cancellations;
GO

CREATE TABLE dbo.Cancellations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SessionId UNIQUEIDENTIFIER NOT NULL,
    Date DATE NOT NULL,
    Reason NVARCHAR(500) NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Cancellations_Sessions FOREIGN KEY (SessionId)
        REFERENCES dbo.Sessions(Id),
    CONSTRAINT FK_Cancellations_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Attendance Table
-- =========================
IF OBJECT_ID('dbo.Attendances', 'U') IS NOT NULL
    DROP TABLE dbo.Attendances;
GO

CREATE TABLE dbo.Attendances (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    
    StudentId UNIQUEIDENTIFIER NOT NULL,
    ScheduleId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Absent',
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Attendance_Students FOREIGN KEY (StudentId)
        REFERENCES dbo.Students(Id),
    
    CONSTRAINT FK_Attendance_Schedules FOREIGN KEY (ScheduleId)
        REFERENCES dbo.Schedules(Id),
    
    CONSTRAINT FK_Attendance_Accounts FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
);
GO
