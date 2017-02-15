using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LK.GreenRoute
{
    class Processor
    {
        public TrafficLight trafficLight { get; set; }
        public List<Job> jobs { get; set; }

        public Processor()
        {
            jobs = new List<Job>();
        }
    }
}
