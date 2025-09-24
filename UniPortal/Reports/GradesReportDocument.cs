using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UniPortal.Dtos;
using UniPortal.Services.Academics.Configs;

namespace UniPortal.Reports
{
    public class GradesReportDocument : BaseReportDocument
    {
        private StudentDto? _student;
        private Dictionary<string, List<StudenGradeDto>>? _grades;

        public GradesReportDocument(InstitutionService institutionService)
            : base(institutionService)
        {
        }

        public void SetData(StudentDto student, Dictionary<string, List<StudenGradeDto>> grades)
        {
            _student = student;
            _grades = grades;
        }

        public override void Compose(IDocumentContainer container)
        {
            if (_student == null || _grades == null)
                throw new InvalidOperationException("Report data not set. Call SetData() before generating.");

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
            var allGrades = _grades!.SelectMany(s => s.Value).ToList();
            var cgpa = allGrades.Any() ? allGrades.Average(g => g.GPA) : 0m;

            container.Column(col =>
            {
                col.Spacing(4);

                // Institution Header from base class
                col.Item().Element(ComposeInstitutionHeader);

                // Report Title (centered)
                col.Item().AlignCenter()
                   .Text("Academic Transcript")
                   .Bold()
                   .FontSize(12);

                // Student info + CGPA
                col.Item().PaddingTop(10).Column(infoCol =>
                {
                    infoCol.Spacing(3);

                    infoCol.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Spacing(2);
                            left.Item().Text($"Date: {DateTime.Now:dd MMM yyyy}").FontSize(10);
                            left.Item().Text($"Student: {_student!.StudentNumber} - {_student.FullName}").FontSize(10);
                            left.Item().Text($"Program: {_student.ProgramName}, Batch: {_student.Batch}, Sec: {_student.Section}").FontSize(10);
                            left.Item().Text($"CGPA: {cgpa:0.00}").SemiBold().FontSize(10);
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
                foreach (var semester in _grades!)
                {
                    col.Item().PaddingTop(10)
                           .Text(semester.Key)
                           .Bold()
                           .FontSize(12);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Course Code
                            columns.RelativeColumn(4); // Course Title
                            columns.RelativeColumn(3); // Faculty
                            columns.RelativeColumn(1); // Grade
                            columns.RelativeColumn(1); // Marks
                            columns.RelativeColumn(1); // GPA
                        });

                        table.Header(header =>
                        {
                            header.Cell().PaddingVertical(3).Text("Code").SemiBold().FontSize(10);
                            header.Cell().PaddingVertical(3).Text("Course").SemiBold().FontSize(10);
                            header.Cell().PaddingVertical(3).Text("Faculty").SemiBold().FontSize(10);
                            header.Cell().PaddingVertical(3).Text("Grade").SemiBold().FontSize(10);
                            header.Cell().PaddingVertical(3).Text("Marks").SemiBold().FontSize(10);
                            header.Cell().PaddingVertical(3).Text("GPA").SemiBold().FontSize(10);
                        });

                        int count = semester.Value.Count;
                        foreach (var g in semester.Value)
                        {
                            table.Cell().Element(c => TableDataCell(c, g.CourseCode));
                            table.Cell().Element(c => TableDataCell(c, g.CourseTitle));
                            table.Cell().Element(c => TableDataCell(c, g.FacultyName));
                            table.Cell().Element(c => TableDataCell(c, g.Grade));
                            table.Cell().Element(c => TableDataCell(c, g.Marks.ToString("0.##")));
                            table.Cell().Element(c => TableDataCell(c, g.GPA.ToString("0.##")));
                        }

                        // Semester CGPA row
                        table.Cell().ColumnSpan(5).PaddingVertical(6).Text("Semester CGPA").SemiBold().FontSize(10);
                        table.Cell().PaddingVertical(6)
                             .Text((count > 0 ? semester.Value.Average(x => x.GPA).ToString("0.##") : "0"))
                             .SemiBold()
                             .FontSize(10);
                    });
                }
            });
        }

        private void TableDataCell(IContainer container, string text)
        {
            container.BorderBottom(1)
                     .BorderColor(Colors.Grey.Lighten4)
                     .PaddingVertical(4)
                     .Text(text)
                     .FontSize(10);
        }
    }
}
