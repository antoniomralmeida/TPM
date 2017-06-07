using LK.GPXUtils;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Accord.MachineLearning;
using Accord.Collections;

namespace LK.TimeWindows
{  
    class DBScan
    {
        public static List<IEnumerable<DBScanPoint>> allClusters;

        public DBScan()
        {
            // To-Do
        }

        public void dbscan(double[][] rawData, List<double> dbPoints, double eps, int minPoints)
        {
            List<IEnumerable<DBScanPoint>> clusters = new List<IEnumerable<DBScanPoint>>();
            IEnumerable<DBScanPoint> neighborPoints = null;

            var points = dbPoints.Select(x => new DBScanPoint(x)).ToArray();
            int clusterCount = 0;
            int[] pointByCluster = new int[points.Count()];

            //double[][] pointsKDTree = dbPoints.Select(x => new double[] { x, 0.0 }).ToArray();
            //KDTree<int> tree = new KDTree<int>(2);
            //tree = KDTree.FromData<int>(pointsKDTree);

            for (int i = 0; i < points.Count(); i++)
            {
                var p = points[i];
                if (!p.visited)
                {
                    p.visited = true;
                    neighborPoints = findNeighbours(points, p, eps);
                    if (neighborPoints.Count() < minPoints)
                    {
                        p.id = -1; // Noise point
                    }
                    else
                    {
                        clusterCount++;
                        Console.WriteLine("clusterCount: " + clusterCount);
                        clusters.Add(neighborPoints);
                        expandCluster(points, p, neighborPoints, clusterCount, eps, minPoints);
                    }
                }
                if (p.id > 0)
                    pointByCluster[i] = p.id - 1;
                else
                    Console.WriteLine("Noise point!");
                
            }
            allClusters = clusters;

            double[][] m = mean(rawData);
            Console.WriteLine("RAW DATA: " + rawData.Count());
            Console.WriteLine("PONT BY CLUSTER: " + pointByCluster.Count());

            double sse = Utils.sumSquaresError(rawData, pointByCluster, m);

            Console.WriteLine("Testing Discretization by DBScan, numClusters:"
                           + allClusters.Count() + ", withinss: " + sse);
        }

        private static void expandCluster(IEnumerable<DBScanPoint> points, DBScanPoint p, IEnumerable<DBScanPoint> neighborPoints,
            int clusterCount, double eps, int minPoints)
        {
            p.id = clusterCount;
            IEnumerable<DBScanPoint> neighbors;

            for (int i = 0; i < neighborPoints.Count(); i++)
            {
                var pointNeighbor = neighborPoints.ElementAt(i);
                if (!pointNeighbor.visited)
                {
                    pointNeighbor.visited = true;
                    neighbors = findNeighbours(points, pointNeighbor, eps);
                    if (neighbors.Count() >= minPoints)
                   { 
                        neighborPoints = neighborPoints.Union(neighbors).ToArray();
                    }
                }
                if (pointNeighbor.id == 0) // not classified
                {
                    pointNeighbor.id = clusterCount;
                } 
            }
       }

        private static IEnumerable<DBScanPoint> findNeighbours(IEnumerable<DBScanPoint> points, DBScanPoint p, double eps)
        {
            var neighborPoints = points.Where(x => Math.Abs(x.value - p.value) <= eps);
            return neighborPoints;
        }

        public double[][] mean(double[][] rawData)
        {
            double[][] points = new double [allClusters.Count][];
            for (int i = 0; i < allClusters.Count; i++)
            {
                points[i] = new double[1];
            }

            for (int i = 0; i < allClusters.Count; i++)
            {
                double meanCluster = 0; 

                foreach (var p in allClusters[i])
                {
                    meanCluster += p.value;
                }

                meanCluster = meanCluster / allClusters.ElementAt(i).Count();

                double smallestSub = double.MaxValue;
                double point = 0;
                foreach (var p in allClusters[i])
                {
                    double subtr = Math.Abs(p.value - meanCluster);
                    if (subtr < smallestSub)
                    {
                        smallestSub = subtr;
                        point = p.value;
                    }
                        
                }
                points[i][0] = point;
            }
            return points;
        }
    }
}
