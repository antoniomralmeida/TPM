using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LK.OSMUtils.OSMDatabase;
using LK.FDI;

namespace LK.HRT
{
    public class HotRoute
    {

        public List<Connection> Segments
        {
            get; set;
        }

        public HotRoute(Connection start)
        {
            Segments = new List<Connection>();
            Segments.Add(start);
        }
    }
}