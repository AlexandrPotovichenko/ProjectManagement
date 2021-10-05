using ProjectManagement.Domain.Interfaces;

namespace ProjectManagement.Domain.Models
{
    public class CardMember :IEntity<int>
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Role Role { get; set; }
        public int CardId { get; set; }
        public Card Card { get; set; }
    }
}