namespace CCSWebKySearch.Dtos
{
    public class MarriageLicenseDto
    {
        public int ID { get; set; }
        public string? GroomSurname { get; set; }
        public string? GroomGiven { get; set; }
        public string? BrideSurname { get; set; }
        public string? BrideGiven { get; set; }
        public string? Date { get; set; }
        public string? License { get; set; }
        public string? Book { get; set; }
        public string? Page { get; set; }
        public string? GroomRace { get; set; }
        public string? BrideRace { get; set; }
        public string? Official { get; set; }
        public string? Extra { get; set; }
        public string? Witness1 { get; set; }
        public string? Witness2 { get; set; }
        public string? IssueDate { get; set; }
        public string? ReturnedDate { get; set; }
    }
}