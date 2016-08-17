using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LK.TMatch.XMLUtils
{
    public class XMLDocument
    {
        public List<Bucket> Buckets
        {
            get; set;
        }

        /// <summary>
        /// Creates a new XMLDocument
        /// </summary>
        public XMLDocument()
        {
            Buckets = new List<Bucket>();
        }

        /// <summary>
        /// Loads a XML document from the input stream
        /// </summary>
        /// <param name="input">Stream with the XML file</param>
        public void Load(Stream input)
        {
            XMLDataReader reader = new XMLDataReader();
            reader.BucketRead += new XMLBucketReadHandler(b => Buckets = b);

            reader.Read(input);
        }

        /// <summary>
        /// Loads a XML document from the input file
        /// </summary>
        /// <param name="filename">The path to the input file</param>
        public void Load(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                Load(fs);
            }
        }
    }
}
