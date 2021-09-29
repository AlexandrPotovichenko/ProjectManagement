using System.Collections.Generic;

namespace ProjectManagement.Models
{
    public class CheckList
    {
        public int ChecklistID { get; set; }

        public string Name { get; set; }
        public List<CheckListItem> ChecklistItems { get; set; }
    }
}