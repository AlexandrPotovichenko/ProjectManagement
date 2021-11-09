using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Models
{
    public class WebAppException : Exception
    {
        public int Status { get; set; } 
        public WebAppException() : base()
        {
        }
        public WebAppException(string message) : base(message)
        {
        }
        public WebAppException(int status,string message) : base(message)
        {
            Status = status;
        }
        public WebAppException(string message, params object[] args) : base(String.Format(CultureInfo.CurrentCulture, message, args))
        {
        }
    }
}
