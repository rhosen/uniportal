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
    IdentityUserId NVARCHAR(450) NOT NULL,  -- Link to AspNetUsers
    CreatedById UNIQUEIDENTIFIER NULL,       -- tracks creator
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
    AccountId UNIQUEIDENTIFIER NOT NULL,         -- FK to Accounts
    StudentId NVARCHAR(100) NULL,                -- Assigned later by admin
    BatchNumber NVARCHAR(50) NULL,              -- Optional at registration
    Section NVARCHAR(50) NULL,                  -- Optional at registration
    DepartmentId UNIQUEIDENTIFIER NULL,         -- Nullable until assigned by admin
    IsDeleted BIT NOT NULL DEFAULT 0,           -- From IEntity
    DeletedAt DATETIME2 NULL,                   -- From IEntity
    CreatedById UNIQUEIDENTIFIER NULL,          -- From IEntity
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),  -- From IEntity
    UpdatedAt DATETIME2 NULL,                   -- From IEntity
   
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
    Code NVARCHAR(50) NOT NULL UNIQUE,            -- Unique code for the subject (e.g., "CS", "MATH")
    Name NVARCHAR(200) NOT NULL,                  -- Full name of the subject (e.g., "Computer Science", "Mathematics")
    CreatedById UNIQUEIDENTIFIER NULL,            -- Who created this subject
    IsDeleted BIT NOT NULL DEFAULT 0,             -- Logical delete flag
    DeletedAt DATETIME2 NULL,                     -- When the subject was deleted
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
    SubjectId UNIQUEIDENTIFIER NOT NULL,         -- Foreign key linking to the Subjects table
    DepartmentId UNIQUEIDENTIFIER NOT NULL,       -- Department offering the course
    TeacherId UNIQUEIDENTIFIER NOT NULL,          -- Teacher for this course
    SemesterId UNIQUEIDENTIFIER NOT NULL,         -- Semester when the course is offered
    Credits INT NOT NULL DEFAULT 3,               -- Credits for the course
    CreatedById UNIQUEIDENTIFIER NULL,            -- Who created this course
    IsDeleted BIT NOT NULL DEFAULT 0,             -- Logical delete flag
    DeletedAt DATETIME2 NULL,                     -- When the course was deleted
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Courses_Subject FOREIGN KEY (SubjectId)
        REFERENCES dbo.Subjects(Id),             -- Links course to subject via SubjectId
    CONSTRAINT FK_Courses_Department FOREIGN KEY (DepartmentId)
        REFERENCES dbo.Departments(Id),          -- Links course to department
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
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    EnrollmentDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    CONSTRAINT FK_Enrollments_Student FOREIGN KEY (StudentId)
        REFERENCES dbo.Accounts(Id),
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
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    CONSTRAINT FK_Grades_Student FOREIGN KEY (StudentId)
        REFERENCES dbo.Accounts(Id),
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
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Submissions_Assignment FOREIGN KEY (AssignmentId)
        REFERENCES dbo.Assignments(Id),
    CONSTRAINT FK_Submissions_Student FOREIGN KEY (StudentId)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Submissions_CreatedById FOREIGN KEY (CreatedById)
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
    AccountId UNIQUEIDENTIFIER NULL,                   -- Made nullable
    ActionType NVARCHAR(50) NOT NULL,                  -- Create, Update, Delete
    Action NVARCHAR(200) NOT NULL,                     -- Free-text description
    Entity NVARCHAR(100) NULL,                 
    EntityId UNIQUEIDENTIFIER NULL,            
    Details NVARCHAR(MAX) NULL,                        -- Optional JSON or extra info
    Timestamp DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Logs_Account FOREIGN KEY (AccountId)
        REFERENCES dbo.Accounts(Id)
);
CREATE INDEX IX_Logs_AccountId ON dbo.Logs(AccountId);
CREATE INDEX IX_Logs_Entity_EntityId ON dbo.Logs(Entity, EntityId);
CREATE INDEX IX_Logs_Timestamp ON dbo.Logs(Timestamp);
GO

-- =========================
-- NotificationTypes Table
-- =========================
IF OBJECT_ID('dbo.NotificationTypes', 'U') IS NOT NULL
    DROP TABLE dbo.NotificationTypes;
GO

CREATE TABLE dbo.NotificationTypes (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200) NULL,
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_NotificationTypes_CreatedById FOREIGN KEY (CreatedById)
        REFERENCES dbo.Accounts(Id)
);
GO


-- =========================
-- Notifications Table
-- =========================
IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL
    DROP TABLE dbo.Notifications;
GO

CREATE TABLE dbo.Notifications (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    CreatedById UNIQUEIDENTIFIER NOT NULL,
    NotificationTypeId UNIQUEIDENTIFIER NOT NULL,
    ReceiverId NVARCHAR(100) NULL,  -- stores Account IdentityNumber
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Notifications_CreatedById FOREIGN KEY (CreatedById)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Notifications_NotificationType FOREIGN KEY (NotificationTypeId)
        REFERENCES dbo.NotificationTypes(Id),
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
    DayOfWeek INT NOT NULL, -- 1 = Monday, 2 = Tuesday, etc.
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
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
-- CanceledClasses Table
-- =========================
IF OBJECT_ID('dbo.CanceledClasses', 'U') IS NOT NULL
    DROP TABLE dbo.CanceledClasses;
GO

CREATE TABLE dbo.CanceledClasses (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ClassScheduleId UNIQUEIDENTIFIER NOT NULL,
    Date DATE NOT NULL,
    Reason NVARCHAR(500) NULL,
    CreatedById UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT FK_CanceledClasses_ClassSchedule FOREIGN KEY (ClassScheduleId)
        REFERENCES dbo.ClassSchedules(Id),
    CONSTRAINT FK_CanceledClasses_CreatedById FOREIGN KEY (CreatedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Attendances Table
-- =========================
IF OBJECT_ID('dbo.Attendances', 'U') IS NOT NULL
    DROP TABLE dbo.Attendances;
GO

CREATE TABLE dbo.Attendances (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    StudentId UNIQUEIDENTIFIER NOT NULL,
    ClassScheduleId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Absent',
    CreatedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    RecordedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    
    CONSTRAINT FK_Attendances_Student FOREIGN KEY (StudentId)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Attendances_ClassSchedule FOREIGN KEY (ClassScheduleId)
        REFERENCES dbo.ClassSchedules(Id),
    CONSTRAINT FK_Attendances_CreatedById FOREIGN KEY (CreatedById)
        REFERENCES dbo.Accounts(Id)
);
GO
