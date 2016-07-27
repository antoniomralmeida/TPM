using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LK.GPXTime
{

    public class HistSample
    {

        public static List<Double> CalculateOptimalBinWidth(List<double> x, out double xMin, out double optimalBinWidth, out List<int> freq)
        {
            var xMax = x.Max();
            xMin = x.Min();
            optimalBinWidth = 0;
            const int MIN_BINS = 1;
            const int MAX_BINS = 5;
            List<Double> r = new List<double>();
            int[] minKi = null;
            var minOffset = double.MaxValue;

            foreach (var n in Enumerable.Range(MIN_BINS, MAX_BINS - MIN_BINS).Select(v => v * 5))
            {
                var d = (xMax - xMin) / n;
                var ki = Histogram(x, n, xMin, d);
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
            r.Add(xMin);
            freq = new List<int>();
            for (int i = 0; i < minKi.Length; i++)
            {
                if (minKi[i] > 0)
                {
                    r.Add(xMin + (i + 1) * optimalBinWidth);
                    freq.Add(minKi[i]);
                }
            }
            return r;
        }

        private static int[] Histogram(List<double> data, int count, double xMin, double d)
        {
            var histogram = new int[count];
            foreach (var t in data)
            {
                var bucket = (int)Math.Truncate((t - xMin) / d);
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