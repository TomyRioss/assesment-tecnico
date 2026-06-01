namespace AssesmentTecnico.Models
{
    public class AssignedPriority
    {
        public int Id { get; set; }
        public Priority Priority { get; set; }
    }

    public class AiResult
    {
        public string Summary { get; set; } = string.Empty;
        public List<AssignedPriority> Priorities { get; set; } = new();
    }
}
