using ProjectManagement.Domain.Interfaces;
using System;

namespace ProjectManagement.Domain.Models
{
    public class CardAction : IEntity<int>
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public Card Card { get; set; }
        public int MemberId { get; set; }
        public CardMember CardMember { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public bool IsComment { get; set; }
        public CardAction()
        {
        }
        public CardAction(int cardMemberId,string description,bool isComment=false)
        {
            MemberId = cardMemberId;
            Description = description;
            Date = DateTime.Now;
            IsComment = isComment;
        }
    }
}