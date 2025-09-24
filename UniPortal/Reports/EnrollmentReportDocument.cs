using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Configs;

namespace UniPortal.Reports
{
    public class EnrollmentReportDocument : BaseReportDocument
    {
        private StudentDto? _student;
        private List<EnrollmentCourseDto>? _courses;
        private string? _advisingFaculty;

        // Only inject services here
        public EnrollmentReportDocument(InstitutionService institutionService)
            : base(institutionService)
        {
        }

        // Set dynamic data before generating report
        public void SetData(StudentDto student, List<EnrollmentCourseDto> courses, string advisingFaculty)
        {
            _student = student;
            _courses = courses;
            _advisingFaculty = advisingFaculty;
        }

        public override void Compose(IDocumentContainer container)
        {
            if (_student == null || _courses == null || _advisingFaculty == null)
                throw new InvalidOperationException("Report data not set. Call SetData() before generating.");

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                // Institution Header from base class
                col.Item().Element(ComposeInstitutionHeader);

                // Report Title (full width, centered)
                col.Item().AlignCenter()
                   .Text("Enrollment Summary")
                   .Bold()
                   .FontSize(12);

                col.Item().PaddingTop(10).Column(infoCol =>
                {
                    infoCol.Spacing(3); 

                    infoCol.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Spacing(2);
                            left.Item().Text($"Date: {DateTime.Now:dd MMM yyyy}").FontSize(10);
                            left.Item().Text($"Advisor: {_advisingFaculty}").FontSize(10);
                            left.Item().Text($"Student: {_student.StudentNumber} - {_student!.FullName} ").FontSize(10);
                            left.Item().Text($"Program: {_student.ProgramName}, Batch: {_student.Batch}, Sec: {_student.Section}").FontSize(10);
                        });
                    });

                    // Horizontal line separator
                    infoCol.Item().PaddingTop(5)
                           .LineHorizontal(1)
                           .LineColor(Colors.Grey.Lighten2);
                });
            });
        }


        private void ComposeContent(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4); // Course Title
                        columns.RelativeColumn(2); // Faculty
                        columns.RelativeColumn(1); // Credits
                        columns.RelativeColumn(3); // Schedule
                    });

                    table.Header(header =>
                    {
                        header.Cell().PaddingVertical(3).Text("Course Title").SemiBold().FontSize(10);
                        header.Cell().PaddingVertical(3).Text("Faculty").SemiBold().FontSize(10);
                        header.Cell().PaddingVertical(3).Text("Credits").SemiBold().FontSize(10);
                        header.Cell().PaddingVertical(3).Text("Schedule").SemiBold().FontSize(10);
                    });

                    int totalCredits = 0;
                    foreach (var course in _courses!)
                    {
                        totalCredits += course.CreditHours;

                        table.Cell().Element(c => TableDataCell(c, course.CourseTitle));
                        table.Cell().Element(c => TableDataCell(c, course.FacultyName));
                        table.Cell().Element(c => TableDataCell(c, course.CreditHours.ToString()));
                        table.Cell().Element(c => TableDataCell(c, course.Schedule));
                    }

                    // Total row
                    table.Cell().ColumnSpan(2).PaddingVertical(6).Text("Total").SemiBold().FontSize(10);
                    table.Cell().PaddingVertical(6).Text(totalCredits.ToString()).SemiBold().FontSize(10);
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
                .Text(text)
                .FontSize(10);
        }
    }
}
