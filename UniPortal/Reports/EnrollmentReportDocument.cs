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

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
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
            container.PaddingBottom(10).Column(col =>
            {
                col.Spacing(4);

                col.Item().Row(row =>
                {
                    // Left column: Student info (without Advising Faculty)
                    row.RelativeItem().Column(left =>
                    {
                        left.Spacing(2);
                        left.Item().Text(_student.FullName).SemiBold().FontSize(12);
                        left.Item().Text($"Student ID: {_student.StudentNumber}");
                        left.Item().Text($"Program: {_student.ProgramName}");
                        left.Item().Text($"Batch: {_student.Batch}, Section: {_student.Section}");
                    });

                    // Right column: Title, Date, Advising Faculty
                    row.RelativeItem().Column(right =>
                    {
                        right.Spacing(2);
                        right.Item().AlignRight().Text("Enrollment Summary").Bold().FontSize(14);
                        right.Item().AlignRight().Text($"Date: {DateTime.Now:dd MMM yyyy}");
                        right.Item().AlignRight().Text($"Advising Faculty: {_advisingFaculty}").SemiBold();
                    });
                });

                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Table(table =>
                {
                    // Columns
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Course Title
                        columns.RelativeColumn(2); // Faculty
                        columns.RelativeColumn(1); // Credits
                        columns.RelativeColumn(3); // Schedule
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().PaddingVertical(3).Text("Course Title").SemiBold();
                        header.Cell().PaddingVertical(4).Text("Faculty").SemiBold();
                        header.Cell().PaddingVertical(4).Text("Credits").SemiBold();
                        header.Cell().PaddingVertical(4).Text("Schedule").SemiBold();
                    });

                    // Data rows
                    int totalCredits = 0;
                    foreach (var course in _courses)
                    {
                        totalCredits += course.CreditHours;

                        table.Cell().Element(c => TableDataCell(c, course.CourseTitle));
                        table.Cell().Element(c => TableDataCell(c, course.FacultyName));
                        table.Cell().Element(c => TableDataCell(c, course.CreditHours.ToString()));
                        table.Cell().Element(c => TableDataCell(c, course.Schedule));
                    }

                    // Total row
                    table.Cell().ColumnSpan(2).PaddingVertical(6).Text("Total").SemiBold();
                    table.Cell().PaddingVertical(6).Text(totalCredits.ToString()).SemiBold();
                    table.Cell(); // empty for schedule
                });
            });
        }

        private void TableDataCell(IContainer container, string text)
        {
            container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten4)
                .PaddingVertical(4)
                .Text(text);
        }
    }
}
