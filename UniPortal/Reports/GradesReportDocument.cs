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
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeColumn().Column(col =>
                {
                    col.Item().Text($"Grades Report").Bold().FontSize(16);
                    col.Item().Text($"{_student.FullName} ({_student.StudentNumber})").SemiBold();
                });

                row.ConstantColumn(150).Column(col =>
                {
                    col.Item().Text($"Program: {_student.ProgramName}");
                    col.Item().Text($"Department: {_student.DepartmentName}");
                    col.Item().Text($"Batch: {_student.Batch}, Section: {_student.Section}");
                });
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
                        // Define columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80); // Course Code
                            columns.RelativeColumn();   // Course Title
                            columns.RelativeColumn();   // Faculty
                            columns.ConstantColumn(50); // Grade
                            columns.ConstantColumn(50); // Marks
                            columns.ConstantColumn(50); // GPA
                        });

                        // Header row
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Code");
                            header.Cell().Element(CellStyle).Text("Course");
                            header.Cell().Element(CellStyle).Text("Faculty");
                            header.Cell().Element(CellStyle).Text("Grade");
                            header.Cell().Element(CellStyle).Text("Marks");
                            header.Cell().Element(CellStyle).Text("GPA");
                        });

                        // Data rows
                        foreach (var g in sem.Value)
                        {
                            table.Cell().Element(CellStyle).Text(g.CourseCode);
                            table.Cell().Element(CellStyle).Text(g.CourseTitle);
                            table.Cell().Element(CellStyle).Text(g.FacultyName);
                            table.Cell().Element(CellStyle).Text(g.Grade);
                            table.Cell().Element(CellStyle).Text(g.Marks.ToString("0.##"));
                            table.Cell().Element(CellStyle).Text(g.GPA.ToString("0.##"));
                        }

                        // Footer row: Semester GPA
                        table.Footer(footer =>
                        {
                            footer.Cell().ColumnSpan(5).AlignRight().Text("Semester GPA:").Bold();
                            footer.Cell().Text(sem.Value.Average(x => x.GPA).ToString("0.##")).Bold();
                        });
                    });
                }
            });
        }

        private IContainer CellStyle(IContainer container)
        {
            return container.PaddingVertical(3).PaddingHorizontal(5);
        }
    }

}
