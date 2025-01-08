namespace LabCommunictionProj.Models.ViewModel
{
    public class UserAndBooksViewModel
    {
        public UserModel User { get; set; }
        public List <BookModel> purchasedBooks { get; set; }
        public List<BookModel> borrowedBooks { get; set; }

    }
}
