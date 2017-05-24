using LK.GeoUtils;
using LK.GPXUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LK.GreenRoute
{

    class HotRoute : GPXTrack
    {
        private int _yellowTime;
        private int _globalRedTime;
        private int _minGreenTime;
        private int _maxGreenTime;
        private int _greenTime;
        public int _cycleTime;
        private int _redTime;
        private static int counter = 0;
        private readonly int instanceId;

        // new public List<GPXTrackSegment> _segments;
        public HashSet<TrafficLight> _trafficLight;
        public List<Convoy> _convoys;

        public HotRoute()
        {
            //_segments = new List<GPXTrackSegment>();
            _trafficLight = new HashSet<TrafficLight>();
            _convoys = new List<Convoy>();
            this.instanceId = ++counter;
        }

        public int UniqueId
        {
            get { return this.instanceId; }
        }

        public void Webster()
        {
            int maxSpeed = MaxSpeed();

            if (maxSpeed >= 80)
            {
                _yellowTime = 5;
                _globalRedTime = 1;
                _minGreenTime = 17;
            }
            else if (maxSpeed >= 60)
            {
                _yellowTime = 4;
                _globalRedTime = 1;
                _minGreenTime = 15;
            }
            else
            {
                _yellowTime = 3;
                _globalRedTime = 2;
                _minGreenTime = 12;
            }

            _maxGreenTime = (120 - _yellowTime - _globalRedTime) / 2;
            _greenTime = _minGreenTime;
            _redTime = _yellowTime + _globalRedTime + _greenTime;

            _cycleTime = _yellowTime + _globalRedTime + _greenTime + _redTime;

        }

        public int MaxSpeed()
        {
            int maxSpeed = 0;
            foreach (GPXTrackSegment s in _segments)
            {
                if (s.Speed >= maxSpeed)
                    maxSpeed = (int)s.Speed;
            }
            return maxSpeed;
        }

        public void NextGreenTime()
        {
            if (_greenTime < _maxGreenTime)
                _greenTime++;
        }


        public double getTotalTimeRoute()
        {
            double timeAvgSegments = 0;

            foreach (var s in _segments)
            {
                GPXPoint firstPoint = s.Nodes.First();
                GPXPoint lastPoint = s.Nodes.Last();

                var distance = Calculations.GetDistance2D(firstPoint, lastPoint);
                if (s.AvgSpeed != 0)
                {
                    double timeAvgSegment = distance / s.AvgSpeed;
                    timeAvgSegments += timeAvgSegment;
                }
            }

            return timeAvgSegments;
        }

        public void makeTrafficLights(List<Processor> _processors)
        {
            List<TrafficLight> list = null;
            foreach (var segment in _segments)
            {
                foreach (var node in segment.Nodes)
                {
                    Boolean found = false;
                    if (node.TrafficSignal)
                    {
                        TrafficLight t = new TrafficLight();
                        t.Id = node.Id;
                        t.Latitude = node.Latitude;
                        t.Longitude = node.Longitude;
                        _trafficLight.Add(t);
                        
                        foreach (Processor p in _processors)
                        {
                            list = new List<TrafficLight>();
                            list.AddRange(p.trafficLights);
                            foreach (TrafficLight tl in list)
                            {
                                if (tl.Id == t.Id)
                                    found = true;     
                                else if (t.Distance(tl) < 30)
                                {
                                    found = true;
                                    p.trafficLights.Add(t);
                                }
                            }
                        }

                        if(!found)
                        {
                            Processor p = new Processor();
                            p.trafficLights.Add(t);
                            _processors.Add(p);
                        }
                            
                    }
                }
            }
        }

        public void makeConvoys()
        {
            Webster();
            //Console.WriteLine("GreenTime: " + _greenTime);
            int nconvoys = ((int) getTotalTimeRoute() / _cycleTime);
            for (int i = 0; i < nconvoys; i++)
            {
                Convoy c = new Convoy();
                c.hotRoute = this;
                c.ProcessingTime = _greenTime;
                c.timeinRH = i * _cycleTime;
                _convoys.Add(c);
            }
        }


        public double getTimeInRH(TrafficLight t)
        {
            double timeAvgSegments = 0;

            foreach (var segment in _segments)
            {
                GPXPoint firstPoint = segment.Nodes.First();
                GPXPoint lastPoint = segment.Nodes.Last();
                if (segment.Nodes.Contains(t)) {
                    if (firstPoint == t)
                    {
                        break;
                    }
                }
                var distance = Calculations.GetDistance2D(firstPoint, lastPoint);
                double timeAvgSegment = distance / segment.AvgSpeed;
                timeAvgSegments += timeAvgSegment;
                if (segment.Nodes.Contains(t))
                   break;

            }

            return timeAvgSegments;
        }

        public void makeJobs(List<Processor> _processors)
        {
            foreach (var p in _processors)
            {
                foreach (TrafficLight t in p.trafficLights)
                    if (_trafficLight.Contains(t))
                    {

                        t.timeinRH = (int)getTimeInRH(t);
                        foreach (Convoy c in _convoys)
                        {
                            if (c.timeinRH < t.timeinRH)
                            {
                                Job j = new Job();
                                j.convoy = c;
                                p.jobs.Add(j);
                            }
                        }
                    }
            }
                
        }
    }
}
