using System.Collections.Generic;

namespace ProjectManagement.Dto
{
    public class CheckListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CardId { get; set; }
        public ICollection<CheckListItemDto> ChecklistItems { get; set; }
    }
}