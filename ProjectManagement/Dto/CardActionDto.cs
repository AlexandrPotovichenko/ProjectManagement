using System;

namespace ProjectManagement.Dto
{
    public class CardActionDto
    {
        public int Id { get; set; }
        public int CardMemberId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public bool IsComment { get; set; }
    }
}