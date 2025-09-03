using System.Diagnostics;

namespace UniPortal.Data.Entities
{
    public class Course : IEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public Guid TeacherId { get; set; }
        public Guid SemesterId { get; set; }
        public int Credits { get; set; } = 3;
    }

}
