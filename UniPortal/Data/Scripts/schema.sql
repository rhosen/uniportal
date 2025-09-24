USE [UniPortalDB];
GO

-- =========================
-- 1. Accounts Table
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
    Gender NVARCHAR(10) NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsActive BIT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Accounts_AspNetUsers_IdentityId FOREIGN KEY (IdentityId)
        REFERENCES dbo.AspNetUsers(Id)
        ON DELETE CASCADE,
    CONSTRAINT FK_Accounts_Accounts_ModifiedById FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT CHK_Accounts_Gender CHECK (Gender IN ('Male','Female','Other'))
);
GO

-- =========================
-- 2. Departments Table
-- =========================
IF OBJECT_ID('dbo.Departments', 'U') IS NOT NULL
    DROP TABLE dbo.Departments;
GO

CREATE TABLE dbo.Departments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(200) NOT NULL,
    Name NVARCHAR(500) NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Departments_Accounts_ModifiedById FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
);
GO


-- =========================
-- 3. Degrees Table
-- =========================
IF OBJECT_ID('dbo.Degrees', 'U') IS NOT NULL
    DROP TABLE dbo.Degrees;
GO

CREATE TABLE dbo.Degrees (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL UNIQUE,       
    ModifiedById UNIQUEIDENTIFIER NULL,    
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Degrees_Accounts_ModifiedById FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- 4. Programs Table
-- =========================
IF OBJECT_ID('dbo.Programs', 'U') IS NOT NULL
    DROP TABLE dbo.Programs;
GO

CREATE TABLE dbo.Programs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    DepartmentId UNIQUEIDENTIFIER NOT NULL,
    DegreeId UNIQUEIDENTIFIER NOT NULL,
    TotalSemesters INT NOT NULL,
    Duration INT NOT NULL,
    TotalCreditsRequired INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,

    CONSTRAINT FK_Programs_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_Programs_Degrees FOREIGN KEY (DegreeId) REFERENCES dbo.Degrees(Id),
    CONSTRAINT FK_Programs_ModifiedBy FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- 5. Semesters Table
-- =========================
IF OBJECT_ID('dbo.Semesters', 'U') IS NOT NULL
    DROP TABLE dbo.Semesters;
GO

CREATE TABLE dbo.Semesters (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    SemesterType NVARCHAR(10) NOT NULL CHECK (SemesterType IN ('Fall','Spring','Summer')),
    AcademicYear NVARCHAR(9) NOT NULL,          -- e.g., "2025-2026"
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    IsCurrent BIT NOT NULL DEFAULT 0,           -- active semester
    ModifiedById UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,

    CONSTRAINT FK_Semesters_Accounts_ModifiedById FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT UQ_Semesters_Type_Year UNIQUE(SemesterType, AcademicYear)  -- ensures uniqueness
);
GO

-- =========================
-- 6. Batches Table
-- =========================
IF OBJECT_ID('dbo.Batches', 'U') IS NOT NULL
    DROP TABLE dbo.Batches;
GO

CREATE TABLE dbo.Batches (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Number NVARCHAR(50) NOT NULL,       -- e.g., "2025", "2025-2026"
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,

    CONSTRAINT FK_Batches_Accounts_ModifiedById FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- 7. Sections Table
-- =========================
IF OBJECT_ID('dbo.Sections', 'U') IS NOT NULL
    DROP TABLE dbo.Sections;
GO

CREATE TABLE dbo.Sections (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(10) NOT NULL,       -- e.g., "A", "B", "C"
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,

    CONSTRAINT UQ_Sections_Name UNIQUE (Name)
);
GO

-- =========================
-- 8. FacultyTypes Table
-- =========================
IF OBJECT_ID('dbo.FacultyTypes', 'U') IS NOT NULL
    DROP TABLE dbo.FacultyTypes;
GO

CREATE TABLE dbo.FacultyTypes (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,   -- e.g., Lecturer, Assistant Professor, Professor
    
    -- Audit Columns
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,

    CONSTRAINT FK_FacultyTypes_Accounts_ModifiedById FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id)
);

-- Unique index on Name (ignoring soft-deleted)
CREATE UNIQUE INDEX UQ_FacultyTypes_Name ON dbo.FacultyTypes(Name) WHERE IsDeleted = 0;
GO


-- =========================
-- Faculties Table 
-- =========================
IF OBJECT_ID('dbo.Faculties', 'U') IS NOT NULL
    DROP TABLE dbo.Faculties;
GO

CREATE TABLE dbo.Faculties (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    AccountId UNIQUEIDENTIFIER NOT NULL,
    DepartmentId UNIQUEIDENTIFIER NOT NULL,
    FacultyTypeId UNIQUEIDENTIFIER NOT NULL,   -- FK to FacultyTypes
    FacultyNumber NVARCHAR(50) NOT NULL,
    IsAdvisor BIT NOT NULL DEFAULT 0,          -- Whether this faculty can advise students
    
    -- Audit Columns
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,

    CONSTRAINT FK_Faculties_Accounts_AccountId FOREIGN KEY (AccountId) REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Faculties_Departments_DepartmentId FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_Faculties_FacultyTypes_FacultyTypeId FOREIGN KEY (FacultyTypeId) REFERENCES dbo.FacultyTypes(Id),
    CONSTRAINT FK_Faculties_Accounts_ModifiedById FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id),
    CONSTRAINT UQ_Faculties_FacultyNumber UNIQUE (FacultyNumber)
);
GO

-- =========================
-- 9. Students Table
-- =========================
IF OBJECT_ID('dbo.Students', 'U') IS NOT NULL
    DROP TABLE dbo.Students;
GO

CREATE TABLE dbo.Students (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    AccountId UNIQUEIDENTIFIER NOT NULL,
    StudentNumber NVARCHAR(100) NOT NULL,
    BatchId UNIQUEIDENTIFIER NOT NULL,
    SectionId UNIQUEIDENTIFIER NOT NULL,
    ProgramId UNIQUEIDENTIFIER NOT NULL,
    CurrentSemester INT NULL DEFAULT 1,
    GraduationDate DATE NULL,                  -- new column
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Students_Accounts_AccountId FOREIGN KEY (AccountId)
        REFERENCES dbo.Accounts(Id)
        ON DELETE CASCADE,
    CONSTRAINT FK_Students_Programs_ProgramId FOREIGN KEY (ProgramId)
        REFERENCES dbo.Programs(Id),
    CONSTRAINT FK_Students_Batches_BatchId FOREIGN KEY (BatchId)
        REFERENCES dbo.Batches(Id),
    CONSTRAINT FK_Students_Sections_SectionId FOREIGN KEY (SectionId)
        REFERENCES dbo.Sections(Id),
    CONSTRAINT FK_Students_Accounts_ModifiedById FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT UQ_Students_StudentNumber UNIQUE (StudentNumber)
);
GO

CREATE INDEX IX_Students_Program_Batch_Section ON dbo.Students (ProgramId, BatchId, SectionId);
GO

-- =========================
-- 10. CourseTypes Table
-- =========================
IF OBJECT_ID('dbo.CourseTypes', 'U') IS NOT NULL
    DROP TABLE dbo.CourseTypes;
GO

CREATE TABLE dbo.CourseTypes (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(50) NOT NULL UNIQUE,  -- e.g., Core, Elective, Optional
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,

    CONSTRAINT FK_CourseTypes_ModifiedBy FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- 11. Courses Table (fixed)
-- =========================
IF OBJECT_ID('dbo.Courses', 'U') IS NOT NULL
    DROP TABLE dbo.Courses;
GO

CREATE TABLE dbo.Courses (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Title NVARCHAR(200) NOT NULL,
    CreditHours INT NOT NULL,
    DepartmentId UNIQUEIDENTIFIER NOT NULL,
    CourseTypeId UNIQUEIDENTIFIER NOT NULL,       -- new column
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Courses_Accounts_ModifiedById FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Courses_Departments_DepartmentId FOREIGN KEY (DepartmentId)
        REFERENCES dbo.Departments(Id),
    CONSTRAINT FK_Courses_CourseTypes_CourseTypeId FOREIGN KEY (CourseTypeId)
        REFERENCES dbo.CourseTypes(Id)
);
GO


-- =========================
-- 12. Curriculums Table
-- =========================
IF OBJECT_ID('dbo.Curriculums', 'U') IS NOT NULL
    DROP TABLE dbo.Curriculums;
GO

CREATE TABLE dbo.Curriculums (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ProgramId UNIQUEIDENTIFIER NOT NULL,
    SemesterNumber INT NOT NULL,            -- e.g., 1st semester, 2nd semester
    CourseId UNIQUEIDENTIFIER NOT NULL,
    Sequence INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,

    CONSTRAINT FK_Curriculums_Programs FOREIGN KEY (ProgramId) REFERENCES dbo.Programs(Id),
    CONSTRAINT FK_Curriculums_Courses FOREIGN KEY (CourseId) REFERENCES dbo.Courses(Id),
    CONSTRAINT FK_Curriculums_Accounts_ModifiedById FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id),
    CONSTRAINT UQ_Curriculums_Program_Semester_Course UNIQUE (ProgramId, SemesterNumber, CourseId)
);
GO

-- =========================
-- 13. Rooms Table
-- =========================
IF OBJECT_ID('dbo.Rooms', 'U') IS NOT NULL
    DROP TABLE dbo.Rooms;
GO

CREATE TABLE dbo.Rooms (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    RoomName NVARCHAR(50) NOT NULL,
    Capacity INT NOT NULL DEFAULT 30,
    Location NVARCHAR(100) NULL,
    IsClassroom BIT NOT NULL DEFAULT 1,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Rooms_Accounts_ModifiedById FOREIGN KEY (ModifiedById)
        REFERENCES dbo.Accounts(Id)
);
GO


-- =========================
-- 14. CourseOfferings Table (FK naming convention fixed)
-- =========================
IF OBJECT_ID('dbo.CourseOfferings', 'U') IS NOT NULL
    DROP TABLE dbo.CourseOfferings;
GO

CREATE TABLE dbo.CourseOfferings (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CurriculumId UNIQUEIDENTIFIER NOT NULL,         
    CourseId UNIQUEIDENTIFIER NOT NULL,            
    SemesterId UNIQUEIDENTIFIER NOT NULL,           
    SemesterNumber INT NOT NULL, 
    ProgramId UNIQUEIDENTIFIER NOT NULL,           
    BatchId UNIQUEIDENTIFIER NOT NULL,
    SectionId UNIQUEIDENTIFIER NOT NULL,
    FacultyId UNIQUEIDENTIFIER NOT NULL,
    CreditHours INT NOT NULL,                      
    Sequence INT NOT NULL,                     
    MaxEnrollment INT NOT NULL,
    CurrentEnrollment INT NOT NULL DEFAULT 0,

    -- Schedule Snapshot
    Mon BIT NOT NULL DEFAULT 0,
    Tue BIT NOT NULL DEFAULT 0,
    Wed BIT NOT NULL DEFAULT 0,
    Thu BIT NOT NULL DEFAULT 0,
    Fri BIT NOT NULL DEFAULT 0,
    Sat BIT NOT NULL DEFAULT 0,
    Sun BIT NOT NULL DEFAULT 0,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    RoomId UNIQUEIDENTIFIER NOT NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,

    -- Foreign Keys
    CONSTRAINT FK_CourseOfferings_Faculties_FacultyId FOREIGN KEY (FacultyId) REFERENCES dbo.Faculties(Id),
    CONSTRAINT FK_CourseOfferings_Batches_BatchId FOREIGN KEY (BatchId) REFERENCES dbo.Batches(Id),
    CONSTRAINT FK_CourseOfferings_Sections_SectionId FOREIGN KEY (SectionId) REFERENCES dbo.Sections(Id),
    CONSTRAINT FK_CourseOfferings_Courses_CourseId FOREIGN KEY (CourseId) REFERENCES dbo.Courses(Id),
    CONSTRAINT FK_CourseOfferings_Programs_ProgramId FOREIGN KEY (ProgramId) REFERENCES dbo.Programs(Id),
    CONSTRAINT FK_CourseOfferings_Semesters_SemesterId FOREIGN KEY (SemesterId) REFERENCES dbo.Semesters(Id),
    CONSTRAINT FK_CourseOfferings_Rooms_RoomId FOREIGN KEY (RoomId) REFERENCES dbo.Rooms(Id),

    -- Unique constraint
    CONSTRAINT UQ_CourseOfferings_Batch_Section_Course UNIQUE (BatchId, SectionId, CourseId)
);
GO

-- Indexes for performance
CREATE INDEX IX_CourseOfferings_Batch ON dbo.CourseOfferings (BatchId);
CREATE INDEX IX_CourseOfferings_Section ON dbo.CourseOfferings (SectionId);
CREATE INDEX IX_CourseOfferings_Faculty ON dbo.CourseOfferings (FacultyId);
CREATE INDEX IX_CourseOfferings_Batch_Section ON dbo.CourseOfferings (BatchId, SectionId);
GO

-- =========================
-- 15. Assignments Table
-- =========================
IF OBJECT_ID('dbo.Assignments', 'U') IS NOT NULL
    DROP TABLE dbo.Assignments;
GO

CREATE TABLE dbo.Assignments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    CourseOfferingId UNIQUEIDENTIFIER NOT NULL,
    AssignedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    DueDate DATETIME2 NOT NULL,
    FilePath NVARCHAR(500) NULL,      -- file for assignment
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Assignments_CourseOffering FOREIGN KEY (CourseOfferingId) REFERENCES dbo.CourseOfferings(Id),
    CONSTRAINT FK_Assignments_Accounts_ModifiedById FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- 16. CourseMaterials Table (Teacher uploads)
-- =========================
IF OBJECT_ID('dbo.CourseMaterials', 'U') IS NOT NULL
    DROP TABLE dbo.CourseMaterials;
GO

CREATE TABLE dbo.CourseMaterials (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CourseOfferingId UNIQUEIDENTIFIER NOT NULL,
    FacultyId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    FilePath NVARCHAR(500) NOT NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,

    CONSTRAINT FK_CourseMaterials_CourseOffering FOREIGN KEY (CourseOfferingId) REFERENCES dbo.CourseOfferings(Id),
    CONSTRAINT FK_CourseMaterials_Faculties_FacultyId FOREIGN KEY (FacultyId) REFERENCES dbo.Faculties(Id),
    CONSTRAINT FK_CourseMaterials_Accounts_ModifiedById FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- 17. AssignmentSubmissions Table
-- =========================
IF OBJECT_ID('dbo.AssignmentSubmissions', 'U') IS NOT NULL
    DROP TABLE dbo.AssignmentSubmissions;
GO

CREATE TABLE dbo.AssignmentSubmissions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    AssignmentId UNIQUEIDENTIFIER NOT NULL,
    StudentId UNIQUEIDENTIFIER NOT NULL,
    SubmittedDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FilePath NVARCHAR(500) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    MarksAwarded DECIMAL(5,2) NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,

    CONSTRAINT FK_Submissions_Assignments FOREIGN KEY (AssignmentId) REFERENCES dbo.Assignments(Id),
    CONSTRAINT FK_Submissions_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(Id),
    CONSTRAINT FK_Submissions_Accounts_ModifiedById FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id),
    CONSTRAINT UQ_Submission UNIQUE(AssignmentId, StudentId),
    CONSTRAINT CK_Submissions_Status CHECK (Status IN ('Pending', 'Submitted', 'Graded'))
);
GO


-- =========================
-- 18. Enrollments Table
-- =========================
IF OBJECT_ID('dbo.Enrollments', 'U') IS NOT NULL
    DROP TABLE dbo.Enrollments;
GO

CREATE TABLE dbo.Enrollments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    StudentId UNIQUEIDENTIFIER NOT NULL,
    CourseOfferingId UNIQUEIDENTIFIER NOT NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Enrollments_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(Id),
    CONSTRAINT FK_Enrollments_CourseOfferings FOREIGN KEY (CourseOfferingId) REFERENCES dbo.CourseOfferings(Id),
    CONSTRAINT FK_Enrollments_Accounts_ModifiedById FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id),
    CONSTRAINT UQ_Enrollment UNIQUE(StudentId, CourseOfferingId)
);
GO

-- =========================
-- 19. Grades Table
-- =========================
IF OBJECT_ID('dbo.Grades', 'U') IS NOT NULL
    DROP TABLE dbo.Grades;
GO

CREATE TABLE dbo.Grades (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    StudentId UNIQUEIDENTIFIER NOT NULL,
    CourseOfferingId UNIQUEIDENTIFIER NOT NULL,
    GradeValue NVARCHAR(5) NOT NULL,
    Marks DECIMAL(5,2) NOT NULL,
    GPA DECIMAL(3,2) NOT NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Grades_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(Id),
    CONSTRAINT FK_Grades_CourseOfferings FOREIGN KEY (CourseOfferingId) REFERENCES dbo.CourseOfferings(Id),
    CONSTRAINT FK_Grades_Accounts_ModifiedById FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id),
    CONSTRAINT UQ_Grade UNIQUE(StudentId, CourseOfferingId)
);
GO

-- =========================
-- 20. GradeScales Table
-- =========================
IF OBJECT_ID('dbo.GradeScales', 'U') IS NOT NULL
    DROP TABLE dbo.GradeScales;
GO

CREATE TABLE dbo.GradeScales (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Grade NVARCHAR(5) NOT NULL,
    MinMarks DECIMAL(5,2) NOT NULL,
    MaxMarks DECIMAL(5,2) NOT NULL,
    GPA DECIMAL(3,2) NOT NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_GradeScales_Accounts_ModifiedById FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id),
    CONSTRAINT UQ_GradeScale_Grade UNIQUE(Grade)
);
GO

-- =========================
-- 21. Logs Table
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

    CONSTRAINT FK_Logs_Accounts_AccountId FOREIGN KEY (AccountId) REFERENCES dbo.Accounts(Id)
);
GO

CREATE INDEX IX_Logs_AccountId ON dbo.Logs(AccountId);
CREATE INDEX IX_Logs_Entity_EntityId ON dbo.Logs(Entity, EntityId);
CREATE INDEX IX_Logs_Timestamp ON dbo.Logs(Timestamp);
GO

-- =========================
-- 22. Recipients Table
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

    CONSTRAINT FK_Recipients_Accounts_ModifiedById FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- 23. Notices Table
-- =========================
IF OBJECT_ID('dbo.Notices', 'U') IS NOT NULL
    DROP TABLE dbo.Notices;
GO

CREATE TABLE dbo.Notices (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,    -- supports formatted text
    FilePath NVARCHAR(500) NULL,       -- optional attachment
    ModifiedById UNIQUEIDENTIFIER NOT NULL,
    RecipientId UNIQUEIDENTIFIER NOT NULL,
    TargetId NVARCHAR(100) NULL,       -- can be StudentNumber, FacultyId, etc.
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Notices_Accounts_ModifiedById FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id),
    CONSTRAINT FK_Notices_Recipients_RecipientId FOREIGN KEY (RecipientId) REFERENCES dbo.Recipients(Id)
);
GO

-- =========================
-- 24. Attendances Table (Updated)
-- =========================
IF OBJECT_ID('dbo.Attendances', 'U') IS NOT NULL
    DROP TABLE dbo.Attendances;
GO

CREATE TABLE dbo.Attendances (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    
    StudentId UNIQUEIDENTIFIER NOT NULL,
    CourseOfferingId UNIQUEIDENTIFIER NOT NULL,
    AttendanceDate DATE NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Absent',
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Attendances_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(Id),
    CONSTRAINT FK_Attendances_CourseOfferings FOREIGN KEY (CourseOfferingId) REFERENCES dbo.CourseOfferings(Id),
    CONSTRAINT FK_Attendances_Accounts_ModifiedById FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id),

    CONSTRAINT CHK_Attendances_Status CHECK (Status IN ('Present', 'Absent', 'Late'))
);
GO

-- =========================
-- 25. ClassCancellations Table (Updated)
-- =========================
IF OBJECT_ID('dbo.ClassCancellations', 'U') IS NOT NULL
    DROP TABLE dbo.ClassCancellations;
GO

CREATE TABLE dbo.ClassCancellations (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CourseOfferingId UNIQUEIDENTIFIER NOT NULL,
    CancellationDate DATE NOT NULL,
    Reason NVARCHAR(500) NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_ClassCancellations_CourseOfferings FOREIGN KEY (CourseOfferingId) REFERENCES dbo.CourseOfferings(Id),
    CONSTRAINT FK_ClassCancellations_Accounts_ModifiedById FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id)
);
GO

-- =========================
-- Institution Table
-- =========================
IF OBJECT_ID('dbo.Institutions', 'U') IS NOT NULL
    DROP TABLE dbo.Institutions;
GO

CREATE TABLE dbo.Institutions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    
    Name NVARCHAR(200) NOT NULL,         -- Institution full name
    Address NVARCHAR(500) NULL,          -- Optional
    Email NVARCHAR(100) NULL,            -- Contact email
    Phone NVARCHAR(50) NULL,             -- Contact phone
    LogoUrl NVARCHAR(500) NULL,          -- Logo URL

    IsDeleted BIT NOT NULL DEFAULT 0,    -- Soft delete
    DeletedAt DATETIME2 NULL,            -- When deleted

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    ModifiedById UNIQUEIDENTIFIER NULL,  -- References the account who modified

    CONSTRAINT FK_Institution_Accounts_ModifiedById
        FOREIGN KEY (ModifiedById) REFERENCES dbo.Accounts(Id)
);
GO

