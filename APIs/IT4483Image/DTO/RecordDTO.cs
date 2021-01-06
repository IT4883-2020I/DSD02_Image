using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT4483Image.DTO
{
    public class RecordDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public int? Type { get; set; }
        public int? ProblemType { get; set; }
        public float? Longitude { get; set; }
        public float? Latitude { get; set; }
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }
        public string? MonitoredObjectId { get; set; }
        public string? IdSupervisedArea { get; set; }
        public string? IdDrone { get; set; }
        public string? IdFlightPath { get; set; }
        public string? IdCampaign { get; set; }

        public Boolean? IsTraining { get; set; }
        public string? MetaData { get; set; }

    }
}
