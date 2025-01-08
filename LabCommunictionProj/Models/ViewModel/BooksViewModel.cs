namespace LabCommunictionProj.Models.ViewModel
{
    public class BooksViewModel
    {
        public List<BookModel> Books { get; set; }
        public BookModel BookModel { get; set; }
        public string SearchTerm { get; set; } = string.Empty;

    }
}
