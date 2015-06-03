﻿using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;
using LibRecSysCS.Absolute;
using LibRecSysCS.Relative;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LibRecSysCS.Core
{
    /// <summary>
    /// This class implements different similarity metrics.
    /// </summary>
    public static class Metric
    {
        #region Public interfaces to compute similarities of matrix/preference relations
        public static void GetPearsonOfRows(DataMatrix R, int maxCountOfNeighbors,
            double strongSimilarityThreshold, out SimilarityData neighborsByObject)
        {
            HashSet<Tuple<int, int>> foo;
            ComputeSimilarities(R.Matrix, SimilarityMetric.PearsonRating, maxCountOfNeighbors,
                strongSimilarityThreshold, out neighborsByObject, out foo);
        }
        public static void GetCosineOfRows(DataMatrix R, int maxCountOfNeighbors, 
            double strongSimilarityThreshold, out SimilarityData neighborsByObject)
        {
            HashSet<Tuple<int, int>> foo;
            ComputeSimilarities(R.Matrix, SimilarityMetric.CosineRating, maxCountOfNeighbors,
                strongSimilarityThreshold, out neighborsByObject, out foo);
        }
        public static void GetPearsonOfColumns(DataMatrix R, int maxCountOfNeighbors,
            double strongSimilarityThreshold, out SimilarityData neighborsByObject,
            out HashSet<Tuple<int, int>> strongSimilarityIndicators)
        {
            ComputeSimilarities(R.Matrix.Transpose(), SimilarityMetric.PearsonRating, maxCountOfNeighbors,
                strongSimilarityThreshold, out neighborsByObject, out strongSimilarityIndicators);

            // Debug
            /*
            for(int i = 0; i < R.ItemCount&&i<100; i++)
            {
                for (int j = 0; j < R.ItemCount&&j<100; j++)
                {
                    if (i == j) continue;
                    double corr_ij = Correlation.Pearson((SparseVector)R.Matrix.Column(i),(SparseVector)R.Matrix.Column(j));
                    if(corr_ij>strongSimilarityThreshold)
                    {
                        Debug.Assert(strongSimilarityIndicators.Contains(new Tuple<int, int>(i, j)));
                        Debug.Assert(strongSimilarityIndicators.Contains(new Tuple<int, int>(j, i)));
                    }
                }
            }
            */
        }
        public static void GetCosineOfColumns(DataMatrix R, int maxCountOfNeighbors,
            double strongSimilarityThreshold, out SimilarityData neighborsByObject,
            out HashSet<Tuple<int, int>> strongSimilarityIndicators)
        {
            // Just rotate the matrix
            ComputeSimilarities(R.Matrix.Transpose(), SimilarityMetric.CosineRating, maxCountOfNeighbors,
                strongSimilarityThreshold, out neighborsByObject, out strongSimilarityIndicators);
        }
        public static void GetCosineOfPrefRelations(PrefRelations PR, int maxCountOfNeighbors,
                        double strongSimilarityThreshold, out SimilarityData neighborsByObject)
        {
            HashSet<Tuple<int, int>> foo;
            ComputeSimilarities(PR, SimilarityMetric.CosinePrefRelations, maxCountOfNeighbors,
    strongSimilarityThreshold, out neighborsByObject, out foo);
        }

        #endregion

        #region Private implementations
        private enum SimilarityMetric { PearsonPrefRelations, PearsonRating, CosinePrefRelations, CosineRating };

        #region Compute similarities
        private static void ComputeSimilarities(Matrix<double> R, 
            Metric.SimilarityMetric similarityMetric, int maxCountOfNeighbors,
            double minSimilarityThreshold,  out SimilarityData neighborsByObject,
            out HashSet<Tuple<int, int>> strongSimilarityIndicators)
        {
            int dimension = R.RowCount;
            List<Vector<double>> rows = new List<Vector<double>>(R.EnumerateRows());

            // I assume that the rows are enumerated from first to last
            Debug.Assert(rows[0].Sum() == R.Row(0).Sum());
            Debug.Assert(rows[rows.Count - 1].Sum() == R.Row(rows.Count - 1).Sum());

            List<Tuple<int, int>> strongSimilarityIndicators_out = new List<Tuple<int, int>>();

            SimilarityData neighborsByObject_out = new SimilarityData(maxCountOfNeighbors);

            Object lockMe = new Object();
            Parallel.For(0, dimension, indexOfRow =>
            {
                Utils.PrintEpoch("Progress current/total", indexOfRow, dimension);
                Dictionary<Tuple<int, int>,double> similarityCache = new Dictionary<Tuple<int, int>,double>();
                List<Tuple<int, int>> strongSimilarityIndocatorCache = new List<Tuple<int, int>>();

                for (int indexOfNeighbor = 0; indexOfNeighbor < dimension; indexOfNeighbor++)
                {
                    if (indexOfRow == indexOfNeighbor) { continue; } // Skip self similarity

                    else if (indexOfRow > indexOfNeighbor)
                    {
                        switch (similarityMetric)
                        {
                            case Metric.SimilarityMetric.CosineRating:
                                // TODO: make a note that it really matters to make it sparse, it computes differently!
                                double cosine = Metric.CosineR((SparseVector)rows[indexOfRow], (SparseVector)rows[indexOfNeighbor]);
                                    if(cosine >  minSimilarityThreshold)
                                    {
                                        strongSimilarityIndocatorCache.Add(new Tuple<int, int>(indexOfRow, indexOfNeighbor));
                                        strongSimilarityIndocatorCache.Add(new Tuple<int, int>(indexOfNeighbor, indexOfRow));
                                    }
                                    similarityCache[new Tuple<int, int>(indexOfRow, indexOfNeighbor)] = cosine;
                                    similarityCache[new Tuple<int, int>(indexOfNeighbor, indexOfRow)] = cosine;

                                break;
                            case Metric.SimilarityMetric.PearsonRating:
                                double pearson = Metric.PearsonR((SparseVector)rows[indexOfRow], (SparseVector)rows[indexOfNeighbor]);
                                    if (pearson> minSimilarityThreshold)
                                    {
                                        strongSimilarityIndocatorCache.Add(new Tuple<int, int>(indexOfRow, indexOfNeighbor));
                                        strongSimilarityIndocatorCache.Add(new Tuple<int, int>(indexOfNeighbor, indexOfRow));
                                    }
                                    similarityCache[new Tuple<int, int>(indexOfRow, indexOfNeighbor)] = pearson;
                                    similarityCache[new Tuple<int, int>(indexOfNeighbor, indexOfRow)] = pearson;

                                break;
                        }
                    }
                }

                lock (lockMe)
                {
                    foreach(var entry in similarityCache)
                    {
                        neighborsByObject_out.AddSimilarityData(entry.Key.Item1, entry.Key.Item2, entry.Value);
                    }
                    strongSimilarityIndicators_out.AddRange(strongSimilarityIndocatorCache);
                }
            });

            neighborsByObject = neighborsByObject_out;
            neighborsByObject.SortAndRemoveNeighbors();
            strongSimilarityIndicators = new HashSet<Tuple<int,int>>(strongSimilarityIndicators_out);
        }
        #endregion


        #region All preference relations based metrics share the same computation flow in this function
        /// <summary>
        /// Switch between different metrics.
        /// </summary>
        /// <param name="PR"></param>
        /// <param name="similarityMetric"></param>
        /// <returns></returns>
        private static void ComputeSimilarities(PrefRelations PR,
            Metric.SimilarityMetric similarityMetric, int maxCountOfNeighbors,
                        double minSimilarityThreshold, out SimilarityData neighborsByObject,
            out HashSet<Tuple<int, int>> strongSimilarityIndicators)
        {
            int dimension = PR.UserCount;
            HashSet<Tuple<int, int>> strongSimilarityIndicators_out = new HashSet<Tuple<int, int>>();
            SimilarityData neighborsByObject_out = new SimilarityData(maxCountOfNeighbors);

            // Compute similarity for the lower triangular
            Object lockMe = new Object();
            Parallel.For(0, dimension, i =>
            {
                Utils.PrintEpoch("Progress current/total", i, dimension);

                for (int j = 0; j < dimension; j++)
                {
                    if (i == j) { continue; } // Skip self similarity

                    else if (i > j)
                    {
                        switch (similarityMetric)
                        {
                            case SimilarityMetric.CosinePrefRelations:
                                double cosinePR = Metric.cosinePR(PR, i, j);
                                lock (lockMe)
                                {
                                    if (cosinePR > minSimilarityThreshold)
                                    {
                                        strongSimilarityIndicators_out.Add(new Tuple<int, int>(i, j));
                                    }
                                    neighborsByObject_out.AddSimilarityData(i, j, cosinePR);
                                    neighborsByObject_out.AddSimilarityData(j, i, cosinePR);
                                }
                                break;
                            // More metrics to be added here.
                        }
                    }
                }
            });

            neighborsByObject = neighborsByObject_out;
            strongSimilarityIndicators = strongSimilarityIndicators_out;
        }
        #endregion

        #region Rating Pearson
        private static double PearsonR(SparseVector Vector_a, SparseVector Vector_b)
        {
            double correlation = Correlation.Pearson(Vector_a,Vector_b);
            if (double.IsNaN(correlation))
            {
                // This means one of the row has 0 standard divation,
                // it does not correlate to anyone
                // so I assign the correlatino to be 0. however, strictly speaking, it should be left NaN
                correlation = 0;
            }
            return correlation;
        }
        #endregion

        #region Rating Cosine
        private static double CosineR(SparseVector Vector_a, SparseVector Vector_b)
        {
            return Vector_a.DotProduct(Vector_b) / (Vector_a.L2Norm() * Vector_b.L2Norm());
            //return Distance.Cosine(R.Row(a).ToArray(), R.Row(b).ToArray());
        }
        #endregion

        #region Preference Relation Pearson
        private static double PearsonPR()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Preference Relation Cosine
        private static double cosinePR(PrefRelations PR, int u_a, int u_b)
        {
            SparseMatrix pr_a = PR[u_a];
            SparseMatrix pr_b = PR[u_b];

            //Debug.Assert(pr_a.Trace() == SparseMatrix.Zero, "The diagonal of user preference relation matrix should be left empty.");
            //Debug.Assert(pr_b.Trace() == SparseMatrix.Zero, "The diagonal of user preference relation matrix should be left empty.");

            // The number of preference relations agreed between users a and b
            int agreedCount = pr_a.Fold2((count, prefOfA, prefOfB) =>
                    count + (prefOfA == prefOfB ? 1 : 0), 0, pr_b, Zeros.AllowSkip);

            #region Obsolate naive implementation
            /*
            // TODO: there should be a faster lambda way of doing this 
            // Loop through all non-zero elements
            foreach (Tuple<int, int, double> element in pr_a.EnumerateIndexed(Zeros.AllowSkip))
            {
                int item_i = element.Item1;
                int item_j = element.Item2;
                double preference_a = element.Item3;
                // Because pr_ij is just the reverse of pr_ji,
                // we count only i-j to avoid double counting
                // and also reduce the number of calling pr_b[]
                if (item_i > item_j)
                {
                    if (preference_a == pr_b[item_i, item_j])
                    {
                        ++agreedCount;
                    }
                }
            }
            */
            #endregion

            // Multiplicaiton result can be too large and cause overflow,
            // therefore we do Sqrt() first and then multiply
            double normalization = checked(Math.Sqrt((double)pr_a.NonZerosCount) * Math.Sqrt((double)pr_b.NonZerosCount));

            // Very small value
            return agreedCount / normalization;
        }
        #endregion

        #endregion
    }
}
