using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IT4483Image.Models
{
    public class Stream
    {
        public long Id { get; set; }
        public string Title { get; set; }

        public string SessionDescription { get; set; }
        public int? ProblemType { get; set; }
        public string Link { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public string MetaData { get; set; }

    }
}
