using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LK.FDI.XMLUtils
{
    public delegate void XMLBucketReadHandler(List<Bucket> parsedBuckets);

    public interface IXMLDataReader
    {
        event XMLBucketReadHandler BucketRead;
    }
}
