namespace SrgInfrastructure.Models
{
    public class TaskBoardViewModel
    {
        public List<SrgDomain.Model.Task> ToDo { get; set; }
        public List<SrgDomain.Model.Task> Completed { get; set; }
        public List<SrgDomain.Model.Task> Overdue { get; set; }
    }
}
