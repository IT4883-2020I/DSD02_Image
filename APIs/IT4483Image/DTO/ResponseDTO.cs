using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT4483Image.DTO
{
    public class ResponseDTO
    {
        public string message { get; set; }
        public int status { get; set; }
        public object result { get; set; }
        public int total { get; set; }

        public ResponseDTO(string message, int status, object result, int total)
        {
            this.message = message;
            this.status = status;
            this.result = result;
            this.total = total;
        }
        public ResponseDTO(string message, int status, object result)
        {
            this.message = message;
            this.status = status;
            this.result = result;
        }
        public ResponseDTO(string message, int status)
        {
            this.message = message;
            this.status = status;
        }
    }
}
