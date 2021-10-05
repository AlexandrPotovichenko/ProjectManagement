using System.Collections.Generic;

namespace ProjectManagement.Dto
{
    public class BoardDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<ListDto> Lists { get; set; }
        public ICollection<BoardMemberDto> BoardMembers { get; set; }
    }
}
