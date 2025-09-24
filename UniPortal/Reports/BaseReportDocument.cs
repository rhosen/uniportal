using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using UniPortal.Data.Entities;
using UniPortal.Services.Academics.Configs;

namespace UniPortal.Reports
{
    public abstract class BaseReportDocument : IDocument
    {
        protected readonly Institution Institution;

        protected BaseReportDocument(InstitutionService institutionService)
        {
            // Ensure we always have one institution
            Institution = institutionService.GetAllAsync().Result.FirstOrDefault()
                          ?? throw new InvalidOperationException("No institution found.");
        }

        public abstract void Compose(IDocumentContainer container);

        public virtual DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        protected void ComposeInstitutionHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Spacing(4);

                col.Item().Row(row =>
                {
                    // Logo
                    var logoPath = GetLogoPath();
                    if (!string.IsNullOrEmpty(logoPath))
                    {
                        row.ConstantItem(80).Height(80)
                           .Image(logoPath, ImageScaling.FitArea);
                    }
                    else
                    {
                        row.ConstantItem(80).Height(80)
                           .AlignMiddle().AlignCenter()
                           .Text("LOGO").FontSize(10);
                    }

                    // Add some spacing between logo and text
                    row.Spacing(10); // <-- adds horizontal spacing in the row

                    // Institution Info
                    row.RelativeItem().Column(info =>
                    {
                        info.Item().Text(Institution.Name).Bold().FontSize(14);
                        info.Item().Text(Institution.Address).FontSize(10);

                        var contact = $"Email: {Institution.Email}";
                        if (!string.IsNullOrWhiteSpace(Institution.Phone))
                            contact += $" | Phone: {Institution.Phone}";

                        info.Item().Text(contact).FontSize(10);
                    });
                });
            });
        }


        // Utility method to resolve logo path
        private string? GetLogoPath()
        {
            if (string.IsNullOrWhiteSpace(Institution.LogoUrl))
                return null;

            // Convert relative paths (like "/img/logo.png") to physical file path
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var relativePath = Institution.LogoUrl.TrimStart('/')
                                .Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(basePath, relativePath);

            return File.Exists(fullPath) ? fullPath : null;
        }
    }
}
