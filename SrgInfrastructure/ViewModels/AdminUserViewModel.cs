namespace SrgInfrastructure.ViewModels
{
    public class AdminUserViewModel
    {
        public string Id { get; set; } // Identity User Id
        public string Email { get; set; }
        public string CurrentRole { get; set; }
        public string SelectedRole { get; set; }
        public int? DepartmentId { get; set; }
    }
}
