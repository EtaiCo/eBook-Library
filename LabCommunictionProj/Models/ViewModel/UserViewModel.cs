namespace LabCommunictionProj.Models.ViewModel
{
    public class UserViewModel
    {   
        public UserModel User { get; set; }
        public List<UserModel> Users { get; set; }
        public string SearchTerm { get; set; }
    }
}
