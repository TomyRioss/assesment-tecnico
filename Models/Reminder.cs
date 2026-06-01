namespace AssesmentTecnico.Models
{
    public enum Priority
    {
        Low,
        Medium,
        High
    }

    public enum ReminderStatus
    {
        Pending,
        Notified,
        Resolved,
        Overdue
    }

    public class Reminder
    {
        public int Id { get; set; }
        public string ExpiryType { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public int CondoId { get; set; }
        public ReminderStatus Status { get; set; }
        public Priority Priority { get; set; }
    }
}
