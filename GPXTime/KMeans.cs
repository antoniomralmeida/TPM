﻿using System;
using System.Linq;
using System.Text;


// K-means clustering demo. ('Lloyd's algorithm')
// Coded using static methods. Normal error-checking removed for clarity.
// This code can be used in at least two ways. You can do a copy-paste and then insert the code into some system that uses clustering.
// Or you can wrap the code up in a Class Library. The single public method is Cluster().

namespace LK.GPXTime
{
    class KMeans
    {

        public static int[] Cluster(double[][] rawData, int numClusters,  out double[][] means,  out double withinss)
        {
            // k-means clustering
            // index of return is tuple ID, cell is cluster ID
            // ex: [2 1 0 0 2 2] means tuple 0 is cluster 2, tuple 1 is cluster 1, tuple 2 is cluster 0, tuple 3 is cluster 0, etc.
            // an alternative clustering DS to save space is to use the .NET BitArray class
            double[][] data = Normalized(rawData); // so large values don't dominate

            bool changed = true; // was there a change in at least one cluster assignment?
            bool success = true; // were all means able to be computed? (no zero-count clusters)
            int seed = 7;
            int nstart = 5;
            // init clustering[] to get things started
            // an alternative is to initialize means to randomly selected tuples
            // then the processing loop is
            // loop
            //    update clustering
            //    update means
            // end loop
            int maxCount = data.Length * 10; // sanity check
            int ct = 0;
            int[] clustering;
            clustering = new int[data.Length]; // InitClustering(data.Length, numClusters, seed++); 
            means = Allocate(numClusters, data[0].Length);

            do
            {
                initMeans(data, means, nstart, seed++); 

                do
                {
                    ++ct; // k-means typically converges very quickly
                    changed = UpdateClustering(data, clustering, means); // (re)assign tuples to clusters. no effect if fail
                    success = UpdateMeans(data, clustering, means); // compute new cluster means if possible. no effect if fail
                } while (changed && success && ct < maxCount);
                if (!changed && success)
                {
                    withinss = sumSquaresError(data, clustering, means);
                    return clustering;
                }
            }
            while (nstart-- > 0);
            throw new Exception("Bad clustering!");
            
            
        }

        private static double[][] Normalized(double[][] rawData)
        {
            // normalize raw data by computing v' = (v - min) / (max - min)

            // make a copy of input data
            double[][] result = new double[rawData.Length][];
            for (int i = 0; i < rawData.Length; ++i)
            {
                result[i] = new double[rawData[i].Length];
                Array.Copy(rawData[i], result[i], rawData[i].Length);

            }
            
            for (int j = 0; j < result[0].Length; ++j) // each col
            {

                double min = double.MaxValue;
                double max = double.MinValue;
                for (int i = 0; i < result.Length; ++i)
                {
                    if (result[i][j] < min) min = result[i][j];
                    if (result[i][j] > max) max = result[i][j];
                }
                double factor = max - min;
                for (int i = 0; i < result.Length; ++i)
                    if (factor != 0)
                        result[i][j] = (result[i][j] - min) / factor;
                    else
                        result[i][j] = 0;  //make void
            }
            return result;
        }

        private static double[][] Allocate(int numClusters, int numColumns)
        {
            // convenience matrix allocator for Cluster()
            double[][] result = new double[numClusters][];
            for (int k = 0; k < numClusters; ++k)
                result[k] = new double[numColumns];
            return result;
        }


        private static void initMeans(double[][] data, double[][] means, int nstart, int randomSeed)
        {
            Random random = new Random(randomSeed);
            switch (nstart)
            {
                case 5:
                    for (int k = 0; k < means.Length; ++k)
                        for (int j = 0; j < means[k].Length; ++j)
                            if (j == 0)
                                means[k][j] = 0.0;
                            else if (j == means[k].Length - 1)
                                means[k][j] = 1;
                            else
                                means[k][j] = j * 1 / means[k].Length;
                    break;
                case 4:
                    for (int k = 0; k < means.Length; ++k)
                        for (int j = 0; j < means[k].Length; ++j)
                            if (j == 0)
                                means[k][j] = 0.0;
                            else if (j == means[k].Length - 1)
                                means[k][j] = random.Next(0, 1);
                            else
                                means[k][j] = j * 1 / means[k].Length;
                    break;
                default:
                    for (int k = 0; k < means.Length; ++k)
                    {
                        int i = random.Next(0, data.Length);
                        for (int j = 0; j < means[k].Length; ++j)
                        {
                            means[k][j] = data[i][j];
                        }
                    }
                    break;
            }
           }

        private static bool UpdateMeans(double[][] data, int[] clustering, double[][] means)
        {
            // returns false if there is a cluster that has no tuples assigned to it
            // parameter means[][] is really a ref parameter

            // check existing cluster counts
            // can omit this check if InitClustering and UpdateClustering
            // both guarantee at least one tuple in each cluster (usually true)
            int numClusters = means.Length;
            int[] clusterCounts = new int[numClusters];
            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = clustering[i];
                ++clusterCounts[cluster];
            }

            for (int k = 0; k < numClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false;
 
            // update, zero-out means so it can be used as scratch matrix 
            for (int k = 0; k < means.Length; ++k)
                for (int j = 0; j < means[k].Length; ++j)
                    means[k][j] = 0.0;

            for (int i = 0; i < data.Length; ++i)
            {
                int cluster = clustering[i];
                for (int j = 0; j < data[i].Length; ++j)
                    means[cluster][j] += data[i][j]; // accumulate sum
            }

            for (int k = 0; k < means.Length; ++k)
                for (int j = 0; j < means[k].Length; ++j)
                    means[k][j] /= clusterCounts[k]; // danger of div by 0
            return true;
        }

        private static bool UpdateClustering(double[][] data, int[] clustering, double[][] means)
        {
            // (re)assign each tuple to a cluster (closest mean)
            // returns false if no tuple assignments change OR
            // if the reassignment would result in a clustering where
            // one or more clusters have no tuples.

            int numClusters = means.Length;
            bool changed = false;

            int[] newClustering = new int[clustering.Length]; // proposed result
            Array.Copy(clustering, newClustering, clustering.Length);

            double[] distances = new double[numClusters]; // distances from curr tuple to each mean

            for (int i = 0; i < data.Length; ++i) // walk thru each tuple
            {
                for (int k = 0; k < numClusters; ++k)
                    distances[k] = Distance(data[i], means[k]); // compute distances from curr tuple to all k means

                int newClusterID = MinIndex(distances); // find closest mean ID
                if (newClusterID != newClustering[i])
                {
                    changed = true;
                    newClustering[i] = newClusterID; // update
                }
            }

            if (changed == false)
                return false; // no change so bail and don't update clustering[][]
            
            Array.Copy(newClustering, clustering, newClustering.Length); // update
            return true; // good clustering and at least one change
        }

        private static double sumSquaresError(double[][] data, int[] clustering, double[][] means)
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


        private static double Distance(double[] tuple, double[] mean)
        {
            // Euclidean distance between two vectors for UpdateClustering()
            // consider alternatives such as Manhattan distance
            double sumSquaredDiffs = 0.0;
            for (int j = 0; j < tuple.Length; ++j)
                sumSquaredDiffs += Math.Pow((tuple[j] - mean[j]), 2);              
            return Math.Sqrt(sumSquaredDiffs);
        }

        private static int MinIndex(double[] distances)
        {
            // index of smallest value in array
            // helper for UpdateClustering()
            int indexOfMin = 0;
            double smallDist = distances[0];
            
            for (int k = 0; k < distances.Length; ++k)
            {
                if (distances[k] < smallDist)
                {

                    smallDist = distances[k];
                    indexOfMin = k;
                }
            }
            return indexOfMin;
        }

        // ============================================================================

        // misc display helpers for demo

        public static void ShowData(double[][] data, int decimals, bool indices, bool newLine)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                if (indices) Console.Write(i.ToString().PadLeft(3) + " ");
                for (int j = 0; j < data[i].Length; ++j)
                {
                    if (data[i][j] >= 0.0) Console.Write(" ");
                    Console.Write(data[i][j].ToString("F" + decimals) + " ");
                }
                Console.WriteLine("");
            }
            if (newLine) Console.WriteLine("");
        } // ShowData

        public static void ShowVector(int[] vector, bool newLine)
        {
            for (int i = 0; i < vector.Length; ++i)
                Console.Write(vector[i] + " ");
            if (newLine) Console.WriteLine("\n");
        }

        public static void ShowClustered(double[][] data, int[] clustering, double[][] means, int numClusters, int decimals, System.IO.StreamWriter toout)
        {
            for (int k = 0; k < numClusters; ++k)
            {

                for (int i = 0; i < data.Length; ++i)
                {
                    int clusterID = clustering[i];
                    if (clusterID != k) continue;
                    for (int j = 0; j < data[i].Length; ++j)
                    {
                        toout.Write(data[i][j].ToString("F" + decimals) + ";");
                    }
                    toout.Write(k+";");
                    for (int j = 0; j < means[k].Length; ++j)
                    {
                        toout.Write(means[k][j].ToString("F" + decimals) + ";");
                    }
                    toout.WriteLine();

                }

            } // k
        }

        public static void debug(double[][] data, int[] clustering, double[][] means, int numClusters, int decimals)
        {
            Console.WriteLine("-------------------------");
            for (int i = 0; i < data.Length; ++i)
            {
                for (int j = 0; j < data[i].Length; ++j)
                {
                    Console.Write(data[i][j].ToString("F" + decimals) + ";");
                }
                int k = clustering[i];
                Console.Write(k + ";");
                for (int j = 0; j < means[k].Length; ++j)
                {
                    Console.Write(means[k][j].ToString("F" + decimals) + ";");
                }
                Console.WriteLine();

            }
        }


    } // Program
} // ns
