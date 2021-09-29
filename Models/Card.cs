using System.Collections.Generic;

namespace ProjectManagement.Models
{
    public class Card
    {
        public int CardID { get; set; }

        public string Name { get; set; }
        public List<CardAction> Actions { get; set; }
        public List<CheckList> Checklists { get; set; }
        public string Description { get; set; }
    }
}