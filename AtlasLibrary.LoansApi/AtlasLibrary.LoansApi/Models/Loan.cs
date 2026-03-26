namespace AtlasLibrary.LoansApi.Models
{
    public class Loan
    {
        public int Id { get; set; }

        public int ItemId { get; set; }
        public string ItemTitle { get; set; } = string.Empty;

        public int UserId { get; set; }

        public int Quantity { get; set; }

        public DateTime LoanDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? ReturnedDate { get; set; }

        public string Status { get; set; } = "Pending";
    }


}
