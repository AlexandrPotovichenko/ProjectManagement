using System;

namespace ProjectManagement.Models
{
    public class CardAction
    {
        public int CardActionID { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public bool IsComment { get; set; }
    }
}