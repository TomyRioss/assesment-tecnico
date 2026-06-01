using AssesmentTecnico.Models;

namespace AssesmentTecnico.Services
{
    public class ReminderService
    {
        private readonly List<Reminder> _reminders;

        public ReminderService(List<Reminder> reminders)
        {
            _reminders = reminders;
        }

        public List<Reminder> GetOverdue()
        {
            return _reminders
                .Where(r => r.ExpiryDate < DateTime.Now)
                .OrderBy(r => r.ExpiryDate)
                .ToList();
        }

        public List<Reminder> GetUpcoming(int days = 7) // Expires within 7 days.
        {
            var now = DateTime.Now;
            return _reminders
                .Where(r => r.Status == ReminderStatus.Pending &&
                            r.ExpiryDate > now &&
                            r.ExpiryDate <= now.AddDays(days) &&
                            r.Priority != Priority.High)
                .OrderBy(r => r.ExpiryDate)
                .ToList();
        }

        public List<Reminder> GetCritical() // Expires within 7 days and high priority = Critical.
        {
            var now = DateTime.Now;
            return _reminders
                .Where(r => r.Status == ReminderStatus.Pending &&
                           (r.ExpiryDate < now ||
                           (r.ExpiryDate <= now.AddDays(7) && r.Priority == Priority.High)))
                .OrderBy(r => r.ExpiryDate)
                .ToList();
        }
    }
}
