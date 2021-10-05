using ProjectManagement.Domain.Interfaces;
using System;

namespace ProjectManagement.Domain.Models
{
    public class CardAction : IEntity<int>
    {
        public int Id { get; set; }
        public CardMember Member { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public bool IsComment { get; set; }
    }
}