using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniPortal.Data.Entities
{
    public class Account: IEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string IdentityUserId { get; set; } = string.Empty;


        //[ForeignKey(nameof(IdentityUserId))]
        //public IdentityUser IdentityUser { get; set; } // One-to-one
        //public ICollection<Course> Courses { get; set; } // Courses taught by this teacher
        //public ICollection<ClassNote> ClassNotes { get; set; }
        //public ICollection<ClassSchedule> ClassSchedules { get; set; }
        //public ICollection<Assignment> Assignments { get; set; } // optional if teacher assigns assignments
        //public ICollection<Attachment> Attachments { get; set; }
        //public ICollection<Notification> CreatedNotifications { get; set; }
        //public ICollection<Enrollment> Enrollments { get; set; } // if you want student's enrollments
        //public ICollection<Grade> Grades { get; set; } // student grades if teacher assigned
    }
}
