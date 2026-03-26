namespace AtlasLibrary.LoansApi.Models
{
    public class LoanReportDto
    {
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public int ReturnedLoans { get; set; }
        public int OverdueLoans { get; set; }
        public int DueSoonLoans { get; set; }
    }
}