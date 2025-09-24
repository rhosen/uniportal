using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UniPortal.Dtos;

namespace UniPortal.Reports
{
    public class GradesReportDocument : IDocument
    {
        private readonly StudentDto _student;
        private readonly Dictionary<string, List<StudenGradeDto>> _grades;

        public GradesReportDocument(StudentDto student, Dictionary<string, List<StudenGradeDto>> grades)
        {
            _student = student;
            _grades = grades;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

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
            var allGrades = _grades.SelectMany(s => s.Value).ToList();
            var cgpa = allGrades.Any() ? allGrades.Average(g => g.GPA) : 0m;

            container
                .PaddingBottom(10)
                .Column(col =>
                {
                    col.Spacing(4);

                    // Row: left = student info, right = title/date/cgpa
                    col.Item().Row(row =>
                    {
                        // Left column (student info)
                        row.RelativeItem().Column(left =>
                        {
                            left.Spacing(2);
                            left.Item().Text(_student.FullName).SemiBold().FontSize(12);
                            left.Item().Text($"Student ID: {_student.StudentNumber}");
                            left.Item().Text($"Program: {_student.ProgramName}");
                            left.Item().Text($"Batch: {_student.Batch}, Section: {_student.Section}");
                            left.Item().Text($"Department: {_student.DepartmentName}");
                        });

                        // Right column (title, date, CGPA), right-aligned
                        row.RelativeItem().Column(right =>
                        {
                            right.Spacing(2);
                            right.Item().AlignRight().Text("Academic Transcript").Bold().FontSize(14);
                            right.Item().AlignRight().Text($"Date: {DateTime.Now:dd MMM yyyy}");
                            if (allGrades.Any())
                                right.Item().AlignRight()
                                             .Text($"CGPA: {cgpa:0.00}")
                                             .Bold()
                                             .FontColor(Colors.Blue.Darken2);
                        });
                    });

                    // Horizontal line after header
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });
        }


        private void ComposeContent(IContainer container)
        {
            container.Column(col =>
            {
                foreach (var sem in _grades)
                {
                    col.Item().PaddingTop(10).Text(sem.Key).Bold().FontSize(14).FontColor(Colors.Blue.Medium);

                    col.Item().Table(table =>
                    {
                        // Columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Course Code
                            columns.RelativeColumn(4); // Course Title
                            columns.RelativeColumn(3); // Faculty
                            columns.RelativeColumn(1); // Grade
                            columns.RelativeColumn(1); // Marks
                            columns.RelativeColumn(1); // GPA
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().PaddingVertical(4).Text("Code").SemiBold();
                            header.Cell().PaddingVertical(4).Text("Course").SemiBold();
                            header.Cell().PaddingVertical(4).Text("Faculty").SemiBold();
                            header.Cell().PaddingVertical(4).Text("Grade").SemiBold();
                            header.Cell().PaddingVertical(4).Text("Marks").SemiBold();
                            header.Cell().PaddingVertical(4).Text("GPA").SemiBold();
                        });

                        // Data rows
                        foreach (var g in sem.Value)
                        {
                            table.Cell().Element(c => TableDataCell(c, g.CourseCode));
                            table.Cell().Element(c => TableDataCell(c, g.CourseTitle));
                            table.Cell().Element(c => TableDataCell(c, g.FacultyName));
                            table.Cell().Element(c => TableDataCell(c, g.Grade));
                            table.Cell().Element(c => TableDataCell(c, g.Marks.ToString("0.##")));
                            table.Cell().Element(c => TableDataCell(c, g.GPA.ToString("0.##")));

                        }
                        // Semester GPA row
                        table.Cell().ColumnSpan(5).PaddingVertical(6).Text("CGPA").SemiBold();
                        table.Cell().PaddingVertical(6).Text(sem.Value.Average(x => x.GPA).ToString("0.##")).SemiBold();
                    });
                }
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
