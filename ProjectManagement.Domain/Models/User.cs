using System.Collections.Generic;
using ProjectManagement.Domain.Interfaces;

namespace ProjectManagement.Domain.Models
{
    public class User : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //string Password

    }
}
