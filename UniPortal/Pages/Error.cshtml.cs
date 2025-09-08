using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UniPortal.Pages
{
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? TicketId { get; set; }

        public void OnGet(string? ticketId = null)
        {
            TicketId = ticketId;
        }
    }
}
