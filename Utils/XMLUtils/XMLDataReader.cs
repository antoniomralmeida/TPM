using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Utils.XMLUtils
{
    public class XMLDataReader : IXMLDataReader
    {
        XmlReader _xmlReader;

        /// <summary>
        /// Reads data from the xml file
        /// </summary>
        /// <param name="xmlFile">Path to the xml file.</param>
        public void Read(string xmlFile)
        {
            using (FileStream fs = new FileStream(xmlFile, FileMode.Open))
            {
                this.Read(fs);
            }
        }

        /// <summary>
        /// Reads data from a stream
        /// </summary>
        /// <param name="stream">The stram to read data from</param>
        public void Read(Stream stream)
        {

            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.IgnoreProcessingInstructions = true;
            xmlReaderSettings.IgnoreWhitespace = true;

            try
            {
                _xmlReader = XmlTextReader.Create(stream, xmlReaderSettings);

                _xmlReader.Read();
                while (false == _xmlReader.EOF)
                {


                    switch (_xmlReader.NodeType)
                    {
                        case XmlNodeType.XmlDeclaration:
                            _xmlReader.Read();
                            continue;

                        case XmlNodeType.Element:
                            if (_xmlReader.Name != "time-config")
                                throw new XmlException("Invalid xml root element. Expected <time-config>.");

                            ReadXMLTag();
                            return;

                        default:
                            throw new XmlException();
                    }
                }
            }
            finally
            {
                _xmlReader.Close();
                _xmlReader = null;
            }
        }

        /// <summary>
        /// Reads content of the root xml element
        /// </summary>
        private void ReadXMLTag()
        {
            _xmlReader.Read();

            while (_xmlReader.NodeType != XmlNodeType.EndElement)
            {
                switch (_xmlReader.Name)
                {
                    case "buckets":
                        ReadBuckets();
                        break;
                    default:
                        _xmlReader.Skip();
                        break;
                }
            }
        }

        /// <summary>
        /// Reads a point from gpx document
        /// </summary>
        private Bucket ReadBucket()
        {
            // name attribute
            string name = _xmlReader.GetAttribute("name");

            if (string.IsNullOrEmpty(name))
            {
                throw new XmlException("Attribute 'name' is missing.");
            }

            // start attribute
            string start = _xmlReader.GetAttribute("start");

            if (string.IsNullOrEmpty(start))
            {
                throw new XmlException("Attribute 'start' is missing.");
            }
            DateTime startTime = DateTime.ParseExact(start, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);

            // end attribute
            string end = _xmlReader.GetAttribute("end");

            if (string.IsNullOrEmpty(end))
            {
                throw new XmlException("Attribute 'end' is missing.");
            }
            DateTime endTime = DateTime.ParseExact(end, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);

            Bucket parsedBucket = new Bucket(name, startTime, endTime);

            _xmlReader.Skip();
            return parsedBucket;
        }

        /// <summary>
        /// Reads bucket from the xml
        /// </summary>
        private void ReadBuckets()
        {
            List<Bucket> parsedBuckets = new List<Bucket>();

            if (_xmlReader.IsEmptyElement == false)
            {
                _xmlReader.Read();

                while (_xmlReader.NodeType != XmlNodeType.EndElement)
                {
                    if (_xmlReader.NodeType == XmlNodeType.Element)
                    {
                        switch (_xmlReader.Name)
                        {
                            case "bucket":
                                parsedBuckets.Add(ReadBucket());
                                break;
                            default:
                                _xmlReader.Skip();
                                break;
                        }
                    }
                    else
                    {
                        _xmlReader.Skip();
                    }
                }
            }

            _xmlReader.Skip();
            OnBucketRead(parsedBuckets);
        }

        #region event handling

        /// <summary>
        /// Occurs when a track is read from xml
        /// </summary>
        public event XMLBucketReadHandler BucketRead;

        /// <summary>
        /// Raises the TrackRead event
        /// </summary>
        /// <param name="node">The track read from the xml</param>
        protected void OnBucketRead(List<Bucket> parsedBuckets)
        {
            XMLBucketReadHandler temp = BucketRead;
            if (temp != null)
            {
                temp(parsedBuckets);
            }
        }

        #endregion
    }
}
