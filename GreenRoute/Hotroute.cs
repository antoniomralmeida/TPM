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
        private int _cycleTime;
        private int _redTime;

        protected List<GPXTrackSegment> _segments { get; set; }
        protected List<TrafficLight> _trafficLight;
        protected List<Convoy> _convoys;


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

            foreach (var segment in _segments)
            {
                GPXPoint firstPoint = segment.Nodes.First();
                GPXPoint lastPoint = segment.Nodes.Last();

                var distance = Calculations.GetDistance2D(firstPoint, lastPoint);

                double timeAvgSegment = distance / segment.AvgSpeed;
                timeAvgSegments += timeAvgSegment;
            }

            return timeAvgSegments;
        }

        public void setTrafficLights(List<Processor> _processors)
        {
            foreach (var segment in _segments)
            {
                foreach (var node in segment.Nodes)
                {
                    if (node.TrafficSignal)
                    {
                        TrafficLight t = (TrafficLight)node;
                        _trafficLight.Add(t);
                        Boolean achou = false;
                        foreach (Processor p in _processors)
                            if (p.trafficLight == t)
                                achou = true;
                        if(!achou)
                        {
                            Processor p = new Processor();
                            p.trafficLight = t;
                            _processors.Add(p);
                        }
                            
                    }
                }
            }
        }

        public void makeConvoys()
        {
            Webster();
            int nconvoys = ((int)getTotalTimeRoute() / _cycleTime);

            for (int i = 0; i < nconvoys; i++)
            {
                Convoy c = new Convoy();
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

        public void calcTimeInRH()
        {
            foreach (var t in _trafficLight)
                t.timeinRH = (int)getTimeInRH(t);
                    
        }
    }
}
