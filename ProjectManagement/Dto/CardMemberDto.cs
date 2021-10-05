using ProjectManagement.Domain.Models;

namespace ProjectManagement.Dto
{
    public class CardMemberDto
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Role Role { get; set; }
        public int CardId { get; set; }
        public CardDto Card { get; set; }
    }
}