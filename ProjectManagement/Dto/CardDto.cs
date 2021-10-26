using System.Collections.Generic;

namespace ProjectManagement.Dto
{
    public class CardDto
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<CardActionOnCardDto> Actions { get; set; }
        public ICollection<CheckListOnCardDto> Checklists { get; set; }
        public ICollection<CardMemberDto> CardMembers { get; set; }
        public int ListId { get; set; }
        public string ListName { get; set; }

    }
}