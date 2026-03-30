namespace oop_s2_2_mvc_79189.Models
{
    public class DashboardViewModel
    {
        // Counts
        public int InspectionsThisMonth { get; set; }
        public int FailedInspectionsThisMonth { get; set; }
        public int OverdueFollowUps { get; set; }

        // Filter values the user selected
        public string? SelectedTown { get; set; }
        public string? SelectedRiskRating { get; set; }

        // Dropdown options
        public List<string> Towns { get; set; } = new();
        public List<string> RiskRatings { get; set; } = new() { "Low", "Medium", "High" };

        // The filtered inspections list shown in the table
        public List<InspectionRow> Inspections { get; set; } = new();
    }

    public class InspectionRow
    {
        public string PremisesName { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public string RiskRating { get; set; } = string.Empty;
        public DateTime InspectionDate { get; set; }
        public int Score { get; set; }
        public string Outcome { get; set; } = string.Empty;
    }
}