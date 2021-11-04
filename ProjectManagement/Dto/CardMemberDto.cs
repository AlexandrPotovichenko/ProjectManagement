using ProjectManagement.Domain.Models;

namespace ProjectManagement.Dto
{
    public class CardMemberDto
    {
        public int Id { get; set; }
        public int userId { get; set; }
        public string Role { get; set; }
    }
}