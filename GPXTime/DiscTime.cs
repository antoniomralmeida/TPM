using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace LK.TimeWindows
{
    class DiscTime
    {

            public static int[] DiscretizationRoundTime(double[][] rawData, out int numClusters, out double[][] means, out int[] clusterCounts, out double withinss, string type)
        {

            int[] bestClustering = new int[rawData.Length];
           

            Dictionary<String, int> Clusters = new Dictionary<String, int>();
            int l = -1;
            for (int d = 0; d < rawData.Length; d++) {
                String key;
                if (type == "H")
                    key = String.Format("{0:00}:{1:00}", (int)(rawData[d][0] * 24), 0);
                else    
                    key = String.Format("{0:00}:{1:00}", (int)(rawData[d][0] * 24), (int)(rawData[d][0] * 24 * 60) % 24);
                
                if (Clusters.ContainsKey(key))
                    Clusters[key]++;
                else
                {
                    Clusters.Add(key, 1);
                    l++;
                }
                
                bestClustering[d] = l;
                }
            numClusters = Clusters.Count();
            clusterCounts = new int[numClusters];
            means = new double[numClusters][];
            l = 0;
            foreach(var pair in Clusters)
            //for (int c = 0;c<numClusters;c++)
            {
                DateTime dt = DateTime.ParseExact(pair.Key, "HH:mm", System.Globalization.CultureInfo.CurrentCulture);
                clusterCounts[l] = pair.Value;
                means[l++] = new double[] { Utils.Time2Double(dt) };             
            }

            withinss = Utils.sumSquaresError(rawData, bestClustering, means);
            return bestClustering;
        }



    }
}
