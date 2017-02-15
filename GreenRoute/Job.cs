using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LK.GreenRoute
{
    class Job : Convoy
    {
        public Convoy convoy;
        public int TimetoStart;
        private static int counter = 0;
        private readonly int instanceId;

        public Job()
        {
            this.instanceId = ++counter;
        }

        public int UniqueId
        {
            get { return this.instanceId; }
        }
    }
}
