using System.ComponentModel.DataAnnotations;

namespace AtlasLibrary.Models
{
    public class CreateLoanViewModel
    {
        [Required]
        public int UserId { get; set; }

        public string? UserName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Lånedatum")]
        public DateTime LoanDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Förfallodatum")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

        public List<CreateLoanItemViewModel> Items { get; set; } = new();
        public List<CartItemViewModel> CartItems { get; set; } = new();
    }

    public class CreateLoanItemViewModel
    {
        public int ItemId { get; set; }
        public string ItemTitle { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
