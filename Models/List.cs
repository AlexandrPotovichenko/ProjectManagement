using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Models
{
    public class List
    {
        public int ListID { get; set; }

        public string Name { get; set; }
        public List<Card> Cards { get; set; }

    }
}
