using System;
using System.Collections.Generic;

namespace Utils.XMLUtils
{
    public class Bucket
    {
        /// <summary>
        /// Gets bucket name
        /// </summary>
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Gets bucket start time
        /// </summary>
        public DateTime Start
        {
            get; set;
        }

        /// <summary>
        /// Gets bucket end time
        /// </summary>
        public DateTime End
        {
            get; set;
        }

        public Bucket(string name, DateTime start, DateTime end)
        {
            Name = name;
            Start = start;
            End = end;
        }

        public Bucket()
        {
        }
    }
}