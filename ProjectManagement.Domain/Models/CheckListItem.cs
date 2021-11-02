using ProjectManagement.Domain.Interfaces;

namespace ProjectManagement.Domain.Models
{
    public class CheckListItem : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDone { get; set; } = false;
        public int CheckListId { get; set; } 
        public CheckList CheckList { get; set; }  
        public CheckListItem(string name)
        {
            Name = name;
            IsDone = false; 
        }
    }
}