using ProjectManagement.Domain.Interfaces;
using System.Collections.Generic;

namespace ProjectManagement.Domain.Models
{
    public class CheckList : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CardId { get; set; }
        public Card Card { get; set; }
        public ICollection<CheckListItem> ChecklistItems { get; set; }
        public CheckList(string name)
        {
            Name = name;
            ChecklistItems = new List<CheckListItem>();
        }
    }
}