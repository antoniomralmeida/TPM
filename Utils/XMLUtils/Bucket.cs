using System;

namespace Utils.XMLUtils
{
    public class Bucket
    {
        private string _name;
        /// <summary>
        /// Gets bucket name
        /// </summary>
        public string Name
        {
            get; set;
        }

        private DateTime _start;
        /// <summary>
        /// Gets bucket start time
        /// </summary>
        public DateTime Start
        {
            get; set;
        }

        private DateTime _end;
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