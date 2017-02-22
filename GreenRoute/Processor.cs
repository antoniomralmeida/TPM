using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LK.GreenRoute
{
    class Processor
    {

        public List<TrafficLight> trafficLights;
        public List<Job> jobs { get; set; }
        private static int counter = 0;
        private readonly int instanceId;

        public Processor()
        {
            jobs = new List<Job>();
            trafficLights = new List<TrafficLight>();
            this.instanceId = counter++;
        }

        public int UniqueId
        {
            get { return this.instanceId; }
        }
    }
}
