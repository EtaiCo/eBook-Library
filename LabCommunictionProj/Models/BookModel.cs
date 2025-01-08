using System.ComponentModel.DataAnnotations;

namespace LabCommunictionProj.Models
{
    public class BookModel
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author name is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Author name must be at least 2 chars and at most 20 chars")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Publisher name is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Publisher name must be at least 2 chars and at most 20 chars")]
        public string Publisher { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required")]
        [RegularExpression(@"^(1[4-9]\d{2}|20[0-2]\d|2025)$", ErrorMessage = "Year must be between 1445-2025")]
        public string Year { get; set; } = string.Empty;

        [Required(ErrorMessage = "Buy price is required")]
        [Range(1, 150, ErrorMessage = "Buy Price must be between 1 and 150")]
        public float BuyPrice { get; set; }

        [Required(ErrorMessage = "Borrow Price is required")]
        [Range(1, 150, ErrorMessage = "Borrow Price must be between 1 and 150")]
        public float BorrowPrice { get; set; }

        [Required(ErrorMessage = "Number of copies is required")]
        [Range(1, 3, ErrorMessage = "Number of copies must be between 1 and 3")]
        public int CopiesNum { get; set; }

        [Required(ErrorMessage = "Age limit is required")]
        [Range(1, 99, ErrorMessage = "Age limit must be between 1 and 99")]
        public int AgeLimit { get; set; }

        [Required(ErrorMessage = "Genre is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Genre name must be at least 2 chars and at most 20 chars")]
        public string Genre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Is For Sale field is required.")]
        [RegularExpression("^(yes|no)$", ErrorMessage = "Is For Sale must be either 'yes' or 'no'.")]
        public string IsForSale { get; set; }

        [Required(ErrorMessage = "Cover field is required.")]
        [Url(ErrorMessage = "Cover must be a valid URL.")]
        public string? Cover { get; set; }

        [Required(ErrorMessage = "ISBN field is required.")]
        [StringLength(13, MinimumLength = 5, ErrorMessage = "ISBN must be must be at least 3 digits and at most 13 digits")]
        public string? Isbn { get; set; }
        public string? Type { get; set; } = string.Empty;
        public float? Price { get; set; } = 0;

    }
}