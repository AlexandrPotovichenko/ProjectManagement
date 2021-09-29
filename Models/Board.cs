using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Models
{
    public class Board
    {
        public int BoardID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<List> Lists { get; set; }


        public List<Member> Membership  { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Name { get; set; }
    }
}
