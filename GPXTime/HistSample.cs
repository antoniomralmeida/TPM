using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LK.TimeWindows
{
    public class HistSample
    {

        public static int[] Histogram(double[][] rawData, out int numClusters, out double[][] means, out int[] clusterCounts, out double withinss)
  {


            double xMax = double.MinValue;
            double xMin = double.MaxValue;

            for (int d = 0; d < rawData.Length; d++)
            {
                if (rawData[d][0] < xMin)
                    xMin = rawData[d][0];

                if (rawData[d][0] > xMax)
                    xMax = rawData[d][0];
            }

            int[] bestClustering = new int[rawData.Length];
            double optimalBinWidth = 0;

            int MAX_BINS;
            int MIN_BINS;
            Utils.Sturges(rawData.Length, out MIN_BINS, out MAX_BINS);

            int[] minKi = null;
            var minOffset = double.MaxValue;

            foreach (var n in Enumerable.Range(MIN_BINS, MAX_BINS- MIN_BINS+1).Select(v => v))
            {
                var d = (xMax - xMin) / n;
                var ki = CountBin(rawData, n, xMin, d);
                var ki2 = ki.Skip(1).Take(ki.Length - 2).ToArray();

                var mean = ki2.Average();
                var variance = ki2.Select(v => Math.Pow(v - mean, 2)).Sum() / n;

                var offset = (2 * mean - variance) / Math.Pow(d, 2);

                if (offset < minOffset)
                {
                    minKi = ki;
                    minOffset = offset;
                    optimalBinWidth = d;
                }
            }
            numClusters = minKi.Length;
            clusterCounts = new int[numClusters];
            means = new double[numClusters][];
            for (int i = 0; i < numClusters; i++)
                clusterCounts[i] = minKi[i];
            for (int d = 0; d < rawData.Length; d++)
            {
                for (int i = 0; i < numClusters; i++)
                    if (rawData[d][0] < xMin + (i + 1) * optimalBinWidth)
                    {
                        bestClustering[d] = i;
                        break;
                    }
            }
            for (int i = 0; i < numClusters; i++)
            {
                means[i] = new double[] { xMin + (i + 1) * optimalBinWidth};
            }

            withinss = Utils.sumSquaresError(rawData, bestClustering, means);
            return bestClustering;
        }

        private static int[] CountBin(double[][] rawData, int count, double xMin, double d)
        {
            var histogram = new int[count];
            for (int i =0;i< rawData.Length;i++)
            {
                var bucket = (int)Math.Truncate((rawData[i][0] - xMin) / d);
                if (count == bucket) //fix xMax
                    bucket--;
                histogram[bucket]++;
            }
            return histogram;
        }

        private static double[] LinearSpace(double a, double b, int count)
        {
            double[] output = new double[count];

            for (int i = 0; i < count; i++)
            {
                output[i] = a + ((i * (b - a)) / (count - 1));

            }

            return output;
        }
    }
}
