# UniPortalDB - Future Enhancements (Comprehensive)

## 1. Faculty Enhancements
- Add **FacultyType** (e.g., Lecturer, Professor, Assistant Professor).
- Add **IsAdvisor** BIT – whether the faculty can advise students.

## 2. University / Institute Configuration
- Create **UniversityConfig** table to store:
  - Name, address, logo, contact info.
  - Academic settings, grading rules, semester start/end rules.
  - Used wherever university-specific info is needed in UI or reports.

## 3. Exams & Assessment Tracking
- `Exams` table for midterms, finals, quizzes.
- `ExamResults` table to store student marks.
- Link exams to `CourseOfferings`.

## 4. Attendance by Date
- Add `AttendanceDate` to `Attendances`.
- Ensure unique attendance per student per course per date.

## 5. Course Pre-requisites
- `CoursePrerequisites` table to enforce enrollment rules.

## 6. Backlogs / Retake Enrollment
- Allow students to enroll in courses outside their normal curriculum if they fail.
- Track via `EnrollmentType` in `Enrollments`:
  - e.g., `'Regular'` vs `'Backlog'` or `'Supplementary'`
- Common industry term: **Backlog** (widely understood in academic contexts).

## 7. Fees / Payments (Future)
- Tables for invoices, payments, scholarships.

## 8. Notifications / Messaging Enhancements
- Track email/SMS logs for notices.

## 9. Optional / Nice-to-have Enhancements
- `ProgramSettings` or `DepartmentSettings` for specific academic rules.
- `CourseMaterials` type categorization (PDF, Video, PPT) for filtering.

## Notes
- Authentication & roles handled by ASP.NET Identity.
- Course offering availability can be determined dynamically; no separate `CourseSlots` table needed.
- Conflict management for course offerings handled via application logic.
