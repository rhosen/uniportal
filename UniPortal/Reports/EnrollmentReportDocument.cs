using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UniPortal.Dtos;

namespace UniPortal.Reports
{
    public class EnrollmentReportDocument : IDocument
    {
        private readonly StudentDto _student;
        private readonly List<EnrollmentCourseDto> _courses;
        private readonly string _advisingFaculty;

        public EnrollmentReportDocument(StudentDto student, List<EnrollmentCourseDto> courses, string advisingFaculty)
        {
            _student = student;
            _courses = courses;
            _advisingFaculty = advisingFaculty;
        }

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                });
        }

        private void ComposeHeader(IContainer container)
        {
            container
                .PaddingBottom(10)
                .Column(col =>
                {
                    col.Spacing(4);

                    col.Item().Text(_student.FullName).Bold().FontSize(16);
                    col.Item().Text($"Student ID: {_student.StudentNumber}");
                    col.Item().Text($"Program: {_student.ProgramName}, Batch: {_student.Batch}, Section: {_student.Section}");
                    col.Item().Text($"Current Semester: {_student.CurrentSemesterName}");
                    col.Item().Text($"Advising Faculty: {_advisingFaculty}");
                    col.Item().Text($"Date: {DateTime.Now:dd MMM yyyy}");
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);


                });
        }

        private void ComposeContent(IContainer container)
        {
            int totalCredits = 0;

            container
                .Table(table =>
                {
                    // Columns
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Course Title
                        columns.RelativeColumn(2); // Teacher
                        columns.RelativeColumn(1); // Credits
                        columns.RelativeColumn(3); // Schedule
                    });

                    // Header row
                    table.Header(header =>
                    {
                        header.Cell().PaddingVertical(4).Text("Course Title").SemiBold();
                        header.Cell().PaddingVertical(4).Text("Faculty").SemiBold();
                        header.Cell().PaddingVertical(4).Text("Credits").SemiBold();
                        header.Cell().PaddingVertical(4).Text("Schedule").SemiBold();
                    });

                    // Data rows
                    foreach (var course in _courses)
                    {
                        totalCredits += course.CreditHours;

                        table.Cell().Element(c => TableDataCell(c, course.CourseTitle));
                        table.Cell().Element(c => TableDataCell(c, course.FacultyName));
                        table.Cell().Element(c => TableDataCell(c, course.CreditHours.ToString()));
                        table.Cell().Element(c => TableDataCell(c, course.Schedule));
                    }

                    // Total row
                    table.Cell().ColumnSpan(2).PaddingVertical(6).Text("Total").Bold();
                    table.Cell().PaddingVertical(6).Text(totalCredits.ToString()).Bold();
                    table.Cell(); // empty for Schedule
                });
        }

        private void TableDataCell(IContainer container, string text)
        {
            container
                .PaddingVertical(4)
                .Text(text);
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    }

}