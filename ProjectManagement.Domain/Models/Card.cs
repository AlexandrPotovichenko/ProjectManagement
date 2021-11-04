using ProjectManagement.Domain.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
        public Card()
        {
            Actions = new List<CardAction>();
            CheckLists = new List<CheckList>();
            CardMembers = new List<CardMember>();
        }
        public Card(string name, string description, CardMember cardMember, int listId) : this()
        {
            Name = name;
            Description = description;
            CardMembers.Add(cardMember);
            ListId = listId;
        }

    }
}