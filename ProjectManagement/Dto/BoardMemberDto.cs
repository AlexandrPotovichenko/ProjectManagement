using ProjectManagement.Domain.Models;

namespace ProjectManagement.Dto
{
    public class BoardMemberDto
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Role Role { get; set; }
        public int BoardId { get; set; }

    }
}