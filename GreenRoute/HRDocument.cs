using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using LK.GPXUtils.GPXDataSource;
using LK.GPXUtils;

namespace LK.GreenRoute
{
    class HRDocument
    {
        private List<GPXPoint> _waypoins;
        private List<Processor> _processor;
        private List<HotRoute> _hotRoutes;
        /// <summary>
        /// Gets the list of waypoints
        /// </summary>
        public List<GPXPoint> Waypoints
        {
            get { return _waypoins; }
        }

        private List<GPXRoute> _routes;
        /// <summary>
        /// Gets the list od routes
        /// </summary>
        public List<GPXRoute> Routes
        {
            get { return _routes; }
        }

        private List<GPXTrack> _tracks;
        /// <summary>
        /// Gets the list of tracks
        /// </summary>
        public List<GPXTrack> Tracks
        {
            get { return _tracks; }
            set { this._tracks = value; }
        }

        public List<Processor> Processors { get; set; }

        /// <summary>
        /// Creates a new GPXDocument
        /// </summary>
        public HRDocument()
        {
            _waypoins = new List<GPXPoint>();
            _routes = new List<GPXRoute>();
            _tracks = new List<GPXTrack>();
            _processor = new List<Processor>();
        }

        /// <summary>
        /// Loads a GPX document from the input stream
        /// </summary>
        /// <param name="input">Stream with the GPX file</param>
        public void Load(Stream input)
        {
            
            GPXXmlDataReader reader = new GPXXmlDataReader();
            reader.WaypointRead += new GPXWaypointReadHandler(waypoint => _waypoins.Add(waypoint));
            reader.RouteRead += new GPXRouteReadHandler(route => _routes.Add(route));
            reader.TrackRead += new GPXTrackReadHandler(track => _tracks.Add(track));
            reader.Read(input);
        }

        /// <summary>
        /// Loads a GPX document from the input file
        /// </summary>
        /// <param name="filename">The path to the input file</param>
        public void Load(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                Load(fs);
            }
        }

        /// <summary>
        /// Saves content of this GPXDocument to the output steam
        /// </summary>
        /// <param name="output">The output stram</param>
        public void Save(Stream output)
        {
            using (GPXXmlDataWriter writer = new GPXXmlDataWriter(output))
            {
                foreach (GPXPoint waypoint in Waypoints)
                {
                    writer.WriteWaypoint(waypoint);
                }

                foreach (GPXRoute route in Routes)
                {
                    writer.WriteRoute(route);
                }

                foreach (GPXTrack track in Tracks)
                {
                    writer.WriteTrack(track);
                }

                writer.Close();
            }
        }

        /// <summary>
        /// Saves content of this GPXDocument into the output file
        /// </summary>
        /// <param name="path">The path to the output file</param>
        /// <remarks>If output file already exists, it's overwritten</remarks>
        public void Save(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                Save(fs);
            }
        }

        public void Webster()
        {
            _hotRoutes = new List<HotRoute>();
            foreach (GPXTrack t in _tracks)
            {
                /*foreach (var tr in t.Segments)
                    Console.WriteLine("AvgSpeed=" + tr.AvgSpeed);*/
                HotRoute hotRoute = new HotRoute();
                hotRoute.Segments = t.Segments;
                hotRoute.makeConvoys();
                hotRoute.makeTrafficLights(_processor);
                //Console.WriteLine("Number Processors: " + _processor.Count());
                //Console.WriteLine("Number Traffic Ligth: " + hotRoute._trafficLight.Count());
                hotRoute.makeJobs(_processor);
                _hotRoutes.Add(hotRoute);
            }
        }

        public List<List<int>> getListProcessor()
        {
            List<List<int>> returnList = new List<List<int>>();

            foreach (var p in _processor)
            {
                List<int> pList = new List<int>();
                foreach (var j in p.jobs)
                {
                    pList.Add(j.UniqueId);
                }
                returnList.Add(pList);
            }
            return returnList;
        }

        public List<List<int>> getJobTime()
        {

            List<List<int>> returnList = new List<List<int>>();
            foreach (var p in _processor)
            {
                List<int> pList = new List<int>();
                foreach (var j in p.jobs)
                {
                    pList.Add(j.convoy.ProcessingTime);
                }
                returnList.Add(pList);
            }
            return returnList;
        }

    }
}
