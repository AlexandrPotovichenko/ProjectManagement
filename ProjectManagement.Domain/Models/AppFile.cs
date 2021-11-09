using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagement.Domain.Models
{
    public class AppFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
        public  AppFile(string name, string contentType, byte[] content)
        {
            Name = name;
            Content = content;
            ContentType = contentType;
        }
    }

}
