using ProjectManagement.Domain.Interfaces;
using System.Collections.Generic;

namespace ProjectManagement.Domain.Models
{
    public class Card : IEntity<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public ICollection<CardAction> Actions { get; set; }
        public ICollection<CheckList> CheckLists { get; set; }
        public ICollection<CardMember> CardMembers { get; set; }
        public int ListId { get; set; }
        public List List { get; set; }

        public string Description { get; set; }
    }
}