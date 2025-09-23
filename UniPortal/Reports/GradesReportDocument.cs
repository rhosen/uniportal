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
            container
                .PaddingBottom(10)
                .Column(col =>
                {
                    col.Spacing(4);

                    col.Item().Text("Grades Report").Bold().FontSize(16);
                    col.Item().Text(_student.FullName).SemiBold();
                    col.Item().Text($"Student ID: {_student.StudentNumber}");
                    col.Item().Text($"Program: {_student.ProgramName}, Batch: {_student.Batch}, Section: {_student.Section}");
                    col.Item().Text($"Department: {_student.DepartmentName}");
                    col.Item().Text($"Date: {DateTime.Now:dd MMM yyyy}");
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
                        table.Cell().ColumnSpan(5).PaddingVertical(6).AlignRight().Text("Semester GPA:").Bold();
                        table.Cell().PaddingVertical(6).Text(sem.Value.Average(x => x.GPA).ToString("0.##")).Bold();
                    });
                }
            });
        }

        private void TableDataCell(IContainer container, string text)
        {
            container
                .PaddingVertical(4)
                .Text(text);
        }
    }
}
