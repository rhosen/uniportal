using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniPortal.Constants;
using UniPortal.Services.Accounts;
using UniPortal.ViewModels.Users;

namespace UniPortal.Pages.Users
{
    [Authorize(Roles = Roles.Root)]
    public class AdminModel : PageModel
    {
        private readonly AdminService _adminService;

        public AdminModel(AdminService adminService)
        {
            _adminService = adminService;
        }

        // List of admins for the table
        public List<Data.Entities.Account> Admins { get; set; } = new();

        // Bind properties for creating or editing admins
        [BindProperty] public Data.Entities.Account NewAdmin { get; set; } = new();
        [BindProperty] public Data.Entities.Account EditAdmin { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string EditAdminId { get; set; }

        // Pagination & Search
        [BindProperty(SupportsGet = true)] public string SearchTerm { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        // ------------------------
        // GET: Load Admins
        // ------------------------
        public async Task OnGetAsync()
        {
            // Fetch all admins (excluding super-admin check can be done inside service)
            var allAdmins = await _adminService.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allAdmins = allAdmins
                    .Where(a => a.FirstName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                             || a.LastName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                             || a.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Pagination
            TotalPages = (int)Math.Ceiling(allAdmins.Count / (double)PageSize);
            Admins = allAdmins
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        // ------------------------
        // POST: Create New Admin
        // ------------------------
        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Default password for new admins
            var password = "Admin@123!";

            // Only allow super-admin to create new admins
            if (!User.IsInRole(Roles.Root))
                return Forbid();

            await _adminService.CreateAsync(NewAdmin.Email, password, NewAdmin.FirstName, NewAdmin.LastName);

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        // ------------------------
        // POST: Edit Admin
        // ------------------------
        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            EditAdminId = id;
            var admin = await _adminService.GetByIdAsync(Guid.Parse(id));
            if (admin != null)
            {
                EditAdmin = new Data.Entities.Account
                {
                    Id = admin.Id,
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    Email = admin.Email
                };
            }

            await OnGetAsync();
            return Page();
        }

        // ------------------------
        // POST: Cancel Edit
        // ------------------------
        public IActionResult OnPostCancelEdit()
        {
            EditAdminId = null;
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        // ------------------------
        // POST: Save Edited Admin
        // ------------------------
        public async Task<IActionResult> OnPostSaveEditAsync(string id)
        {
            if (!ModelState.IsValid) return Page();

            // Only super-admin can update admins
            if (!User.IsInRole(Roles.Root))
                return Forbid();


            var profile = new AccountViewModel
            {
                AccountId = Guid.Parse(id),
                FirstName = EditAdmin.FirstName,
                LastName = EditAdmin.LastName,
                Email = EditAdmin.Email,
            };

            await _adminService.UpdateAsync(profile);

            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        // ------------------------
        // POST: Deactivate Admin
        // ------------------------
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (!User.IsInRole(Roles.Root))
                return Forbid();

            await _adminService.DeleteAsync(Guid.Parse(id));
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }

        // ------------------------
        // POST: Activate Admin
        // ------------------------
        public async Task<IActionResult> OnPostActivateAsync(string id)
        {
            if (!User.IsInRole(Roles.Root))
                return Forbid();

            await _adminService.ActivateAsync(Guid.Parse(id));
            return RedirectToPage(new { CurrentPage, SearchTerm });
        }
    }
}
