using ProjectManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Domain.Models
{
    public class List : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int BoardId { get; set; }
        public Board Board { get; set; }
        public ICollection<Card> Cards { get; set; }
    }
}
