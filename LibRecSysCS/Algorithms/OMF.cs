﻿using LibRecSysCS.Absolute;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRecSysCS.Relative
{
    /// <summary>
    /// Ordinal Matrix Factorization.
    /// See Koren, Y., & Sill, J. (2011). 
    /// OrdRec: an ordinal model for predicting personalized item rating distributions. 
    /// In Proceedings of the fifth ACM conference on Recommender systems (pp. 117-124). ACM.
    /// URL: http://dx.doi.org/10.1145/2043932.2043956
    /// </summary>
    public class OMF
    {

        /// <summary>
        /// Ordinal Matrix Factorization.
        /// </summary>
        /// <param name="R_train">The matrix contains training ratings</param>
        /// <param name="R_unknown">The matrix contains ones indicating unknown ratings</param>
        /// <param name="R_scorer">This matrix contains ratings predicted by the scorer on
        /// both the R_train and R_unknown sets</param>
        /// <returns>The predicted ratings on R_unknown</returns>
        #region PredictRatings
        public static string PredictRatings(SparseMatrix R_train, SparseMatrix R_unknown,
 SparseMatrix R_scorer, List<double> quantizer, out DataMatrix R_predicted,
            out Dictionary<Tuple<int, int>, List<double>> OMFDistributionByUserItem)
        {
            StringBuilder log = new StringBuilder();
            /************************************************************
             *   Parameterization and Initialization
            ************************************************************/
            #region Parameterization and Initialization
            // This matrix stores predictions
            SparseMatrix R_predicted_out = (SparseMatrix)Matrix.Build.Sparse(R_unknown.RowCount, R_unknown.ColumnCount);
            Dictionary<Tuple<int, int>, List<double>> OMFDistributionByUserItem_out =
                new Dictionary<Tuple<int, int>, List<double>>(R_unknown.NonZerosCount);

            // User specified parameters
            double maxEpoch = Constants.OMF.MaxEpoch;
            double learnRate = Constants.OMF.LearnRate;
            double regularization = Constants.OMF.Regularization;
            int intervalCount = quantizer.Count;
            int userCount = R_train.RowCount;

            // Parameters for each user
            Dictionary<int, ParametersOfUser> paramtersByUser = new Dictionary<int, ParametersOfUser>(R_train.RowCount);
            
            // Compute initial values of t1 and betas 
            // that will be used for all users, Eq. 5
            double t1_initial = (double)(quantizer[0] + quantizer[1]) / 2;
            Vector<double> betas_initial = Vector.Build.Dense(quantizer.Count - 2);
            for (int i = 1; i <= betas_initial.Count; i++)
            {
                double t_r = t1_initial;
                double t_r_plus_1 = (quantizer[i] + quantizer[i + 1]) * 0.5f;
                betas_initial[i - 1] = Math.Log(t_r_plus_1 - t_r); // natural base
                t_r = t_r_plus_1;
            }

            // Initialize parameters (t1, betas) for each user
            for (int indexOfUser = 0; indexOfUser < R_train.RowCount; indexOfUser++)
            {
                paramtersByUser[indexOfUser] = new ParametersOfUser(t1_initial, betas_initial);
            }
            #endregion

            /************************************************************
             *   Learn parameters from training data R_train and R_score
            ************************************************************/
            #region Learn parameters from training data R_train and R_score
            // Learn parameters for each user, note that each user has his own model
            Object lockMe = new Object();
            Parallel.ForEach(R_train.EnumerateRowsIndexed(), row =>
            {
                int indexOfUser = row.Item1;
                SparseVector ratingsOfUser = (SparseVector)row.Item2;

                // Store this user's ratings from R_train and correpsonding ratings from scorer
                List<double> ratingsFromScorer = new List<double>(ratingsOfUser.NonZerosCount);
                List<double> ratingsFromRTrain = new List<double>(ratingsOfUser.NonZerosCount);
                foreach (var element in ratingsOfUser.EnumerateIndexed(Zeros.AllowSkip))
                {
                    int indexOfItem = element.Item1;
                    double rating = element.Item2;
                    // Ratings need to be added in the same order
                    ratingsFromScorer.Add(R_scorer[indexOfUser, indexOfItem]);
                    ratingsFromRTrain.Add(rating);
                }

                Debug.Assert(ratingsFromScorer.Count == ratingsOfUser.NonZerosCount);
                Debug.Assert(ratingsFromRTrain.Count == ratingsOfUser.NonZerosCount);

                // Parameters for the current user are estimated by
                // maximizing the log likelihood (Eq. 21) using stochastic gradient ascent
                // Eq. 22
                double t1 = paramtersByUser[indexOfUser].t1;
                Vector<double> betas = paramtersByUser[indexOfUser].betas;
                for (int epoch = 0; epoch < maxEpoch; epoch++)
                {
                    for (int i = 0; i < ratingsFromRTrain.Count; i++)
                    {
                        double ratingFromRTrain = ratingsFromRTrain[i];
                        double ratingFromScorer = ratingsFromScorer[i];

                        int r = quantizer.IndexOf(ratingFromRTrain);    // r is the interval that the rating falls into
                        double probLE_r = ComputeProbLE(ratingFromScorer, r, t1, betas);   // Eq. 9
                        double probLE_r_minus_1 = ComputeProbLE(ratingFromScorer, r - 1, t1, betas);
                        double probE_r = probLE_r - probLE_r_minus_1;    // Eq. 10

                        // Compute derivatives/gradients
                        double derivativeOft1 = learnRate / probE_r * (probLE_r * (1 - probLE_r) * DerivativeOfBeta(r, 0, t1)
                                - probLE_r_minus_1 * (1 - probLE_r_minus_1) * DerivativeOfBeta(r - 1, 0, t1)
                                - Constants.OMF.Regularization * t1);

                        Vector<double> derivativesOfbetas = Vector.Build.Dense(betas.Count);
                        for (int k = 0; k < betas.Count; k++)
                        {
                            derivativesOfbetas[k] = learnRate / probE_r * (probLE_r * (1 - probLE_r) *
                                    DerivativeOfBeta(r, k + 1, betas[k]) - probLE_r_minus_1 * (1 - probLE_r_minus_1) *
                                    DerivativeOfBeta(r - 1, k + 1, betas[k]) - regularization * betas[k]);
                        }

                        // Update parameters
                        t1 += derivativeOft1;
                        betas += derivativesOfbetas;
                    }
                }

                // Store the leanred paramemters
                lock (lockMe)
                {
                    paramtersByUser[indexOfUser].t1 = t1;
                    paramtersByUser[indexOfUser].betas = betas;
                }

                Utils.PrintEpoch("user/total", indexOfUser, userCount, "Learned params",
                    String.Format("t1={0:0.000},betas={1}", t1, string.Concat(
                    betas.Select(i => string.Format("{0:0.00},", i)))));
            });
            #endregion

            /************************************************************
             *   Make predictions using learned parameters
            ************************************************************/
            #region Make predictions using learned parameters
            lockMe = new Object();
            Parallel.ForEach(R_unknown.EnumerateIndexed(Zeros.AllowSkip), element =>
            {
                int indexOfUser = element.Item1;
                int indexOfItem = element.Item2;
                // This is the ordinal distribution of the current user
                // given the internal score by MF
                // e.g. what is the probability of each rating 1-5
                List<double> probabilitiesByInterval = new List<double>(quantizer.Count);
                double scoreFromScorer = R_scorer[indexOfUser, indexOfItem];
                double pre = ComputeProbLE(scoreFromScorer, 0, paramtersByUser[indexOfUser].t1, paramtersByUser[indexOfUser].betas);
                probabilitiesByInterval.Add( pre);
                for (int i = 1; i < intervalCount; i++)
                {
                    double pro = ComputeProbLE(scoreFromScorer, i, paramtersByUser[indexOfUser].t1, paramtersByUser[indexOfUser].betas);
                    probabilitiesByInterval.Add( pro - pre);
                    pre = pro;
                }

                // Compute smoothed expectation for RMSE metric
                double expectationRating = 0.0;
                for (int i = 0; i < probabilitiesByInterval.Count; i++)
                {
                    expectationRating += (i + 1) * probabilitiesByInterval[i];
                }

                // TODO: Compute most likely value for MAE metric

                lock (lockMe)
                {
                    // Keep OMF Distribution
                    OMFDistributionByUserItem_out[new Tuple<int, int>(indexOfUser, indexOfItem)] = probabilitiesByInterval;
                    // Keep the numerical prediction
                    R_predicted_out[indexOfUser, indexOfItem] = expectationRating;
                }
            });
            #endregion


            /************************************************************
             *   Generate OMF distributions for R_train as well
            ************************************************************/
            #region Generate OMF distributions for R_train as well
            lockMe = new Object();
            Parallel.ForEach(R_train.EnumerateIndexed(Zeros.AllowSkip), element =>
            {
                int indexOfUser = element.Item1;
                int indexOfItem = element.Item2; ;
                // This is the ordinal distribution of the current user
                // given the internal score by MF
                // e.g. what is the probability of each rating 1-5
                List<double> probabilitiesByInterval = new List<double>(quantizer.Count);
                double scoreFromScorer = R_scorer[indexOfUser, indexOfItem];
                double pre = ComputeProbLE(scoreFromScorer, 0, paramtersByUser[indexOfUser].t1, paramtersByUser[indexOfUser].betas);
                probabilitiesByInterval.Add(pre);
                for (int i = 1; i < intervalCount; i++)
                {
                    double pro = ComputeProbLE(scoreFromScorer, i, paramtersByUser[indexOfUser].t1, paramtersByUser[indexOfUser].betas);
                    probabilitiesByInterval.Add(pro - pre);
                    pre = pro;
                }

                lock (lockMe)
                {
                    // Keep OMF Distribution
                    OMFDistributionByUserItem_out[new Tuple<int, int>(indexOfUser, indexOfItem)] = probabilitiesByInterval;
                }
            });
            #endregion

            R_predicted = new DataMatrix(R_predicted_out);
            OMFDistributionByUserItem = OMFDistributionByUserItem_out;

            return log.ToString();
        }
        #endregion

        #region Some utility functions and parameter class
        /// <summary>
        /// Compute the probability of the score falls into the interval r or before
        /// Eq. 9/13
        /// </summary>
        /// <param name="score"></param>
        /// <param name="r">The interval to be tested</param>
        /// <param name="t1">The t1 parameter of user</param>
        /// <param name="betas">The betas parameter of user</param>
        /// <returns>The probability of score falls into the interval r or before</returns>
        private static double ComputeProbLE(double score, int r, double t1, Vector<double> betas)
        {
            double t_r = t1;

            if (r < 0) { t_r = double.NegativeInfinity; }
            else if (r == 0) { t_r = t1; }
            else if (r > betas.Count) { t_r = double.PositiveInfinity; }
            else
            {
                for (int i = 0; i < r; i++) { t_r += Math.Exp(betas[i]); }  // Equation 5
            }

            return 1 / (1 + (double)Math.Exp(score - t_r));
        }

        /// <summary>
        /// Compute the derivative of beta, Eq. 12
        /// </summary>
        /// <param name="r">The interval</param>
        /// <param name="indexOfBeta">The index of beta</param>
        /// <param name="beta">The beta to be derived</param>
        /// <returns>The derivative of beta</returns>
        private static double DerivativeOfBeta(int r, int indexOfBeta, double beta)
        {
            double derivativeOfBeta = 0;
            if (r >= 0 && indexOfBeta == 0)
            {
                derivativeOfBeta = 1.0;
            }
            else if (indexOfBeta > 0 && r >= indexOfBeta)
            {
                derivativeOfBeta = Math.Exp(beta);
            }
            return derivativeOfBeta;
        }

        /// <summary>
        /// This class encapsulates two parameters for each user.
        /// t1 is the overall bias for this user,
        /// and betas are the biases for each rating interval
        /// </summary>
        private class ParametersOfUser
        {
            public double t1 { get; set; }
            public Vector<double> betas { get; set; }
            public ParametersOfUser(double t1, Vector<double> betas)
            {
                this.t1 = t1;
                this.betas = betas;
            }
        }
        #endregion
    }
}
