using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LK.GPXTime
{
    public class DataPoint
    {
        public double Height { get; set; }
        public double Weight { get; set; }
        public int Cluster { get; set; }
        public DataPoint(double height, double weight)
        {
            Height = height;
            Weight = weight;
            Cluster = 0;
        }

        public DataPoint()
        {

        }

        public override string ToString()
        {
            return string.Format("{{{0},{1}}}", Height.ToString("f" + 1), Weight.ToString("f" + 1));
        }
    }
}
