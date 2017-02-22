using LK.GPXUtils;
using LK.GreenRoute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LK.GreenRoute
{
    class TrafficLight : GPXPoint
    {
        private List<Job> _jobs = new List<Job>();
        public int timeinRH { get; set; }

        public void makeJobs(List<Job> _convoys, int _greentime)
        {
            foreach (Job c in _convoys)
            {
                if (c.timeinRH < timeinRH)
                {
                    Job j = new Job();
                    j.convoy = c;
                    j.TimetoStart = timeinRH - c.timeinRH - _greentime;
                    _jobs.Add(j);
                }
            }

        }

        public double Distance(TrafficLight t)
        {
            return GeoUtils.Calculations.GetDistance2D(this, t);
        }
    }
}
