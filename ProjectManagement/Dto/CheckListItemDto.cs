﻿namespace ProjectManagement.Dto
{
    public class CheckListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDone { get; set; }
        public int CheckListId { get; set; }
    }
}
