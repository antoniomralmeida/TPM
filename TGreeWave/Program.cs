using System;


namespace LK.TGreeNWave
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = new Simplex(
              new[] { 10.2, 422.3, 6.91, 853 },
              new[,] {
          {0.1, 0.5, 0.333333, 1},
          {30, 15, 19, 12},
          {1000, 6000, 4100, 9100},
          {50, 20, 21, 10},
          {4, 6, 19, 30}
              },
              new double[] { 2000, 1000, 1000000, 640, 432 }
            );

            var answer = s.maximize();
            Console.WriteLine(answer.Item1);
            Console.WriteLine(string.Join(", ", answer.Item2));
        }


    }
}