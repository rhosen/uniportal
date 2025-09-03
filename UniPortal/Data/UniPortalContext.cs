using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniPortal.Data.Entities;

namespace UniPortal.Data
{
    public class UniPortalContext : IdentityDbContext<IdentityUser>
    {
        public UniPortalContext(DbContextOptions<UniPortalContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }    
    }
}
