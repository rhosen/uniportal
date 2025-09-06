namespace UniPortal.Data.Entities
{
    using global::UniPortal.Constants;
    using System;

    namespace UniPortal.Data.Entities
    {
        public class Log
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public Guid? AccountId { get; set; }
            public ActionType ActionType { get; set; }    // Use enum instead of string
            public string Action { get; set; } = string.Empty;
            public string? Entity { get; set; }
            public Guid? EntityId { get; set; }
            public string? Details { get; set; }         // Optional JSON payload
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        }
    }

}
