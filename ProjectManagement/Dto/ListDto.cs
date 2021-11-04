using System.Collections.Generic;

namespace ProjectManagement.Dto
{
    public class ListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<CardOnListDto> Cards { get; set; }

    }
}