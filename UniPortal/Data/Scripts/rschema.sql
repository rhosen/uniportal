USE [UniPortalDB];
GO

-- Drop tables in order of dependency
-- 1️⃣ Dependent / Transaction tables first
IF OBJECT_ID('dbo.Attendance', 'U') IS NOT NULL DROP TABLE dbo.Attendance;
IF OBJECT_ID('dbo.AssignmentSubmissions', 'U') IS NOT NULL DROP TABLE dbo.AssignmentSubmissions;
IF OBJECT_ID('dbo.Grades', 'U') IS NOT NULL DROP TABLE dbo.Grades;
IF OBJECT_ID('dbo.Enrollments', 'U') IS NOT NULL DROP TABLE dbo.Enrollments;
IF OBJECT_ID('dbo.ClassSchedules', 'U') IS NOT NULL DROP TABLE dbo.ClassSchedules;
IF OBJECT_ID('dbo.Assignments', 'U') IS NOT NULL DROP TABLE dbo.Assignments;
IF OBJECT_ID('dbo.ClassNotes', 'U') IS NOT NULL DROP TABLE dbo.ClassNotes;
IF OBJECT_ID('dbo.Attachments', 'U') IS NOT NULL DROP TABLE dbo.Attachments;

-- 2️⃣ System tables
IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL DROP TABLE dbo.Notifications;
IF OBJECT_ID('dbo.NotificationTypes', 'U') IS NOT NULL DROP TABLE dbo.NotificationTypes;
IF OBJECT_ID('dbo.Logs', 'U') IS NOT NULL DROP TABLE dbo.Logs;

-- 3️⃣ Reference / Configuration tables
IF OBJECT_ID('dbo.Classrooms', 'U') IS NOT NULL DROP TABLE dbo.Classrooms;
IF OBJECT_ID('dbo.Courses', 'U') IS NOT NULL DROP TABLE dbo.Courses;
IF OBJECT_ID('dbo.Semesters', 'U') IS NOT NULL DROP TABLE dbo.Semesters;
IF OBJECT_ID('dbo.Departments', 'U') IS NOT NULL DROP TABLE dbo.Departments;
IF OBJECT_ID('dbo.Accounts', 'U') IS NOT NULL DROP TABLE dbo.Accounts;

GO
