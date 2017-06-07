using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LK.TimeWindows
{
    class Utils
    {


        public static double sumSquaresError(double[][] data, int[] clustering, double[][] means)
        {
            double result = 0;
            for (int i = 0; i < data.Length; ++i)
            {
                int c = clustering[i];

                for (int j = 0; j < data[i].Length; ++j)
                {
                    result += Math.Pow(data[i][j] - means[c][j], 2);
                }
            }
            return result;
        }

        public static void Sturges(int length, out int min, out int max)
        {
            int sturges =  (int)(Math.Log(length) / Math.Log(2) + 1);
            max = Math.Max(2, sturges * 12 / 10);
            min = Math.Max(2, max / 4); 
        }

        public static double Time2Double(DateTime dt)
        {
            return (dt.Hour + dt.Minute / 60.0 + dt.Second / (60.0 * 60.0)) / 24.0;
        }
    }
}
