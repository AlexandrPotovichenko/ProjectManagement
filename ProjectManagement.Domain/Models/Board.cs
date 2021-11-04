using ProjectManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Domain.Models
{
    public class Board : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<List> Lists { get; set; }
        public ICollection<BoardMember> BoardMembers { get; set; }

        public Board()
        {
            BoardMembers = new List<BoardMember>();
            Lists = new List<List>();
        }
        public Board(string name, string description, BoardMember boardMember):this()
        {
            Name = name;
            Description = description;
            Lists.Add(new List() { Name = "To Do" });
            Lists.Add(new List() { Name = "In Progress" });
            Lists.Add(new List() { Name = "Done" });
            BoardMembers.Add(boardMember);
        }
    }
}
