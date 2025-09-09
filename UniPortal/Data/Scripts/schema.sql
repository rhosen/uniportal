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
    IdentityUserId NVARCHAR(450) NOT NULL,
    CreatedById UNIQUEIDENTIFIER NULL,
    IsActive BIT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Accounts_IdentityUser FOREIGN KEY (IdentityUserId)
        REFERENCES dbo.AspNetUsers(Id)
        ON DELETE CASCADE,

    CONSTRAINT FK_Accounts_CreatedBy FOREIGN KEY (CreatedById)
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
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Departments_Head FOREIGN KEY (HeadId)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Departments_CreatedById FOREIGN KEY (CreatedById)
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
    StudentId NVARCHAR(100) NULL,    -- Human-readable ID for display
    BatchNumber NVARCHAR(50) NULL,
    Section NVARCHAR(50) NULL,
    DepartmentId UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedById UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
   
    CONSTRAINT FK_Students_Account FOREIGN KEY (AccountId)
        REFERENCES dbo.Accounts(Id)
        ON DELETE CASCADE,
    CONSTRAINT FK_Students_Department FOREIGN KEY (DepartmentId)
        REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_Students_CreatedBy FOREIGN KEY (CreatedById)
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
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Semesters_CreatedById FOREIGN KEY (CreatedById)
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
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Subjects_CreatedById FOREIGN KEY (CreatedById)
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
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Courses_Subject FOREIGN KEY (SubjectId)
        REFERENCES dbo.Subjects(Id),
    CONSTRAINT FK_Courses_Department FOREIGN KEY (DepartmentId)
        REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_Courses_Teacher FOREIGN KEY (TeacherId)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Courses_Semester FOREIGN KEY (SemesterId)
        REFERENCES dbo.Semesters(Id),
    CONSTRAINT FK_Courses_CreatedById FOREIGN KEY (CreatedById)
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
    Location NVARCHAR(100) NOT NULL,
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Classrooms_CreatedById FOREIGN KEY (CreatedById)
        REFERENCES dbo.Accounts(Id)
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
    FilePath NVARCHAR(500) NOT NULL,
    FileType NVARCHAR(50) NULL,
    UploadedBy UNIQUEIDENTIFIER NOT NULL,
    RelatedEntity NVARCHAR(50) NULL,
    RelatedEntityId UNIQUEIDENTIFIER NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Attachments_User FOREIGN KEY (UploadedBy)
        REFERENCES Accounts(Id),
    CONSTRAINT FK_Attachments_CreatedById FOREIGN KEY (CreatedById)
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
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    
    CONSTRAINT FK_ClassNotes_Course FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id),
    CONSTRAINT FK_ClassNotes_Teacher FOREIGN KEY (TeacherId)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_ClassNotes_CreatedById FOREIGN KEY (CreatedById)
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
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Assignments_Course FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id),
    CONSTRAINT FK_Assignments_CreatedById FOREIGN KEY (CreatedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Enrollments Table (FK -> Students.Id)
-- =========================
IF OBJECT_ID('dbo.Enrollments', 'U') IS NOT NULL
    DROP TABLE dbo.Enrollments;
GO

CREATE TABLE dbo.Enrollments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    
    StudentId UNIQUEIDENTIFIER NOT NULL,   -- FK to Students.Id
    CourseId UNIQUEIDENTIFIER NOT NULL,
    SemesterId UNIQUEIDENTIFIER NOT NULL,
    CreatedById UNIQUEIDENTIFIER NULL,
    
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    EnrollmentDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    CONSTRAINT FK_Enrollments_Student FOREIGN KEY (StudentId)
        REFERENCES dbo.Students(Id),
    
    CONSTRAINT FK_Enrollments_Course FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id),
    
    CONSTRAINT FK_Enrollments_Semester FOREIGN KEY (SemesterId)
        REFERENCES dbo.Semesters(Id),
    
    CONSTRAINT FK_Enrollments_CreatedById FOREIGN KEY (CreatedById)
        REFERENCES dbo.Accounts(Id),
    
    CONSTRAINT UQ_Enrollment UNIQUE(StudentId, CourseId, SemesterId)
);
GO

-- =========================
-- Grades Table (FK -> Students.Id)
-- =========================
IF OBJECT_ID('dbo.Grades', 'U') IS NOT NULL
    DROP TABLE dbo.Grades;
GO

CREATE TABLE dbo.Grades (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    
    StudentId UNIQUEIDENTIFIER NOT NULL,   -- FK to Students.Id
    CourseId UNIQUEIDENTIFIER NOT NULL,
    SemesterId UNIQUEIDENTIFIER NOT NULL,
    
    Grade NVARCHAR(5) NULL,
    Marks DECIMAL(5,2) NULL,
    CreatedById UNIQUEIDENTIFIER NULL,
    
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    CONSTRAINT FK_Grades_Student FOREIGN KEY (StudentId)
        REFERENCES dbo.Students(Id),
    
    CONSTRAINT FK_Grades_Course FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id),
    
    CONSTRAINT FK_Grades_Semester FOREIGN KEY (SemesterId)
        REFERENCES dbo.Semesters(Id),
    
    CONSTRAINT FK_Grades_CreatedById FOREIGN KEY (CreatedById)
        REFERENCES dbo.Accounts(Id),
    
    CONSTRAINT UQ_Grade UNIQUE(StudentId, CourseId, SemesterId)
);
GO

-- =========================
-- AssignmentSubmissions Table (FK -> Students.Id)
-- =========================
IF OBJECT_ID('dbo.AssignmentSubmissions', 'U') IS NOT NULL
    DROP TABLE dbo.AssignmentSubmissions;
GO

CREATE TABLE dbo.AssignmentSubmissions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    AssignmentId UNIQUEIDENTIFIER NOT NULL,
    StudentId UNIQUEIDENTIFIER NOT NULL,  -- FK to Students.Id
    SubmittedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    MarksAwarded DECIMAL(5,2) NULL,
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,

    CONSTRAINT FK_Submissions_Assignment FOREIGN KEY (AssignmentId)
        REFERENCES dbo.Assignments(Id),
    CONSTRAINT FK_Submissions_Student FOREIGN KEY (StudentId)
        REFERENCES dbo.Students(Id),
    CONSTRAINT FK_Submissions_CreatedBy FOREIGN KEY (CreatedById)
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

    CONSTRAINT FK_Logs_Account FOREIGN KEY (AccountId)
        REFERENCES dbo.Accounts(Id)
);
CREATE INDEX IX_Logs_AccountId ON dbo.Logs(AccountId);
CREATE INDEX IX_Logs_Entity_EntityId ON dbo.Logs(Entity, EntityId);
CREATE INDEX IX_Logs_Timestamp ON dbo.Logs(Timestamp);
GO

-- =========================
-- RecipientTypes Table
-- =========================
IF OBJECT_ID('dbo.RecipientTypes', 'U') IS NOT NULL
    DROP TABLE dbo.RecipientTypes;
GO

CREATE TABLE dbo.RecipientTypes (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200) NULL,
    CreatedById UNIQUEIDENTIFIER NULL,  -- audit column
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_RecipientTypes_CreatedById FOREIGN KEY (CreatedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Notices Table
-- =========================
CREATE TABLE dbo.Notices (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    CreatedById UNIQUEIDENTIFIER NOT NULL,  -- audit column
    RecipientTypeId UNIQUEIDENTIFIER NOT NULL,
    RecipientId NVARCHAR(100) NULL,          -- target student, nullable
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Notices_CreatedById FOREIGN KEY (CreatedById)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Notices_RecipientType FOREIGN KEY (RecipientTypeId)
        REFERENCES dbo.RecipientTypes(Id),
    CONSTRAINT FK_Notices_RecipientStudent FOREIGN KEY (RecipientId)
        REFERENCES dbo.Students(StudentId)
);
GO


-- =========================
-- Create Master Table: ClassSchedules
-- =========================
IF OBJECT_ID('dbo.ClassSchedules', 'U') IS NOT NULL
    DROP TABLE dbo.ClassSchedules;
GO

CREATE TABLE dbo.ClassSchedules (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CourseId UNIQUEIDENTIFIER NOT NULL,
    ClassroomId UNIQUEIDENTIFIER NOT NULL,
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_ClassSchedules_Course FOREIGN KEY (CourseId)
        REFERENCES dbo.Courses(Id),
    CONSTRAINT FK_ClassSchedules_Classroom FOREIGN KEY (ClassroomId)
        REFERENCES dbo.Classrooms(Id),
    CONSTRAINT FK_ClassSchedules_CreatedBy FOREIGN KEY (CreatedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Create Detail Table: ClassScheduleEntries
-- =========================
IF OBJECT_ID('dbo.ClassScheduleEntries', 'U') IS NOT NULL
    DROP TABLE dbo.ClassScheduleEntries;
GO

CREATE TABLE dbo.ClassScheduleEntries (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ScheduleId UNIQUEIDENTIFIER NOT NULL, -- FK → ClassSchedules
    DayOfWeek INT NOT NULL,               -- 1 = Monday, 7 = Sunday
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_ClassScheduleEntries_Schedule FOREIGN KEY (ScheduleId)
        REFERENCES dbo.ClassSchedules(Id),
    CONSTRAINT FK_ClassScheduleEntries_CreatedBy FOREIGN KEY (CreatedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- CanceledClasses Table
-- =========================
IF OBJECT_ID('dbo.CanceledClasses', 'U') IS NOT NULL
    DROP TABLE dbo.CanceledClasses;
GO

CREATE TABLE dbo.CanceledClasses (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ClassScheduleEntryId UNIQUEIDENTIFIER NOT NULL, -- changed FK to detail table
    Date DATE NOT NULL,
    Reason NVARCHAR(500) NULL,
    CreatedById UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_CanceledClasses_ClassScheduleEntry FOREIGN KEY (ClassScheduleEntryId)
        REFERENCES dbo.ClassScheduleEntries(Id),
    CONSTRAINT FK_CanceledClasses_CreatedById FOREIGN KEY (CreatedById)
        REFERENCES dbo.Accounts(Id)
);
GO


-- =========================
-- Attendances Table (FK -> Students.Id)
-- =========================
IF OBJECT_ID('dbo.Attendances', 'U') IS NOT NULL
    DROP TABLE dbo.Attendances;
GO

CREATE TABLE dbo.Attendances (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    
    StudentId UNIQUEIDENTIFIER NOT NULL,    -- FK to Students.Id
    ClassScheduleId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Absent',
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Attendances_Student FOREIGN KEY (StudentId)
        REFERENCES dbo.Students(Id),
    
    CONSTRAINT FK_Attendances_ClassSchedule FOREIGN KEY (ClassScheduleId)
        REFERENCES dbo.ClassSchedules(Id),
    
    CONSTRAINT FK_Attendances_CreatedById FOREIGN KEY (CreatedById)
        REFERENCES dbo.Accounts(Id)
);
GO
