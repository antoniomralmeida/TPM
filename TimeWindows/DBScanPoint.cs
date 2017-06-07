using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LK.TimeWindows
{
    class DBScanPoint
    {
        public bool visited;
        public double value;
        public int id;

        public DBScanPoint(double p)
        {
            this.id = 0; // Not classified
            this.visited = false;
            this.value = p;
        }
    }
}
