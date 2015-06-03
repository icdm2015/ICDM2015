﻿using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;
using LibRecSysCS.Core;
using LibRecSysCS.Absolute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRecSysCS.Relative
{
    class ORF
    {
        Vector<double> meanByUser;
        Vector<double> meanByItem;
        DataMatrix R_train;
        HashSet<Tuple<int, int>> strongSimilarityIndicators;
        // The weights for item-item correlation features
        // It is the \vec{w} in the paper, and featureWeightByItemItem[i,j] is w_ij
        //SparseMatrix featureWeightByItemItem;
        Dictionary<Tuple<int, int>, double> featureWeightByItemItem;

        Dictionary<Tuple<int, int>, List<double>> OMFDistributions;

        public void PredictRatings(DataMatrix R_train, DataMatrix R_unknown, 
            HashSet<Tuple<int,int>> strongSimilarityIndicators, 
            Dictionary<Tuple<int, int>, List<double>> OMFDistributions, 
            double regularization, double learnRate, int maxEpoch, int ratingLevels, 
            out DataMatrix R_predicted_expectations, out DataMatrix R_predicted_mostlikely)
        {
            /************************************************************
             *   Parameterization and Initialization
            ************************************************************/
            #region Parameterization and Initialization
            int userCount = R_train.UserCount;
            int itemCount = R_train.ItemCount;
            meanByUser = R_train.GetUserMeans(); // Mean value of each user
            meanByItem = R_train.GetItemMeans(); // Mean value of each item
            this.R_train = R_train;
            this.OMFDistributions = OMFDistributions;
            R_predicted_expectations = new DataMatrix(R_unknown.UserCount, R_unknown.ItemCount);
            R_predicted_mostlikely = new DataMatrix(R_unknown.UserCount, R_unknown.ItemCount);


            // Initialize the weights
            this.strongSimilarityIndicators = strongSimilarityIndicators;
            featureWeightByItemItem = new Dictionary<Tuple<int, int>, double>(strongSimilarityIndicators.Count);

            // Initialize all strong item-item features
            Random rnd = new Random(Constants.Seed);
            
            foreach(var strongSimilarityPair in strongSimilarityIndicators)
            {
                double randomWeight = rnd.NextDouble() * 0.01;
                featureWeightByItemItem[strongSimilarityPair] = randomWeight;
            }

            // We cache here which items have been rated by the given user
            // it will be reused in every feature update
            Dictionary<int, List<int>> itemsByUser = R_train.GetItemsByUser();

            // TODO: we actually stored more features, because some items may not be co-rated by any user
            Utils.PrintValue("# of item-item features", (featureWeightByItemItem.Count / 2).ToString());

            #endregion

            /************************************************************
             *   Learn weights from training data R_train
            ************************************************************/
            #region Learn weights from training data R_train
            double likelihood_prev = -double.MaxValue;
            for (int epoch = 0; epoch < maxEpoch; epoch++)
            {
                /************************************************************
                 *   Apply Eq. 23 and 24
                ************************************************************/
                #region Apply Eq. 23 and 24
                // Unlike NMF which uses each rating as the input for training,
                // here the set of ratings by each user is the input for each pass
                foreach (var user in R_train.Users)
                {
                    int indexOfUser = user.Item1;
                    Vector<double> ratingsOfUser = user.Item2;
                    Debug.Assert(ratingsOfUser.Storage.IsDense == false, "The user ratings should be stored in a sparse vector.");

                    List<int> itemsOfUser = itemsByUser[indexOfUser];   // Cache the items rated by this user

                    // Now we select one rating r_ui from the user's ratings R_u,
                    // and use this rating to combine with each other rating r_uj in R_u
                    // so that we can refine the weight associated to i-j item pair co-rated by this user
                    foreach (var item_i in ratingsOfUser.EnumerateIndexed(Zeros.AllowSkip))
                    {
                        int indexOfItem_i = item_i.Item1;
                        int r_ui = (int)R_train[indexOfUser, indexOfItem_i];    // The R_train should be all integers
                        double meanOfItem_i = meanByItem[indexOfItem_i];

                        // Find out the neighbors of item_i, i.e., "\vec{r}_u\r_ui" in Eq. 21
                        List<int> neighborsOfItem_i = new List<int>(itemsOfUser.Count);

                        //neighborsOfItem_i.Remove(indexOfItem_i);    // It is not a neighbor of itself

                        // Keep strong neighbors
                        foreach (int indexOfNeighbor in itemsOfUser)
                        {
                            if (strongSimilarityIndicators.Contains(new Tuple<int,int>(indexOfItem_i, indexOfNeighbor))
                                && indexOfNeighbor != indexOfItem_i)
                            {
                                neighborsOfItem_i.Add(indexOfNeighbor);
                            }
                            //else if(indexOfItem_i!=indexOfNeighbor)
                            //{
                            //    double pearson = Correlation.Pearson((SparseVector)R_train.Matrix.Column(indexOfItem_i),
                            //        (SparseVector)R_train.Matrix.Column(indexOfNeighbor));
                            //    Debug.Assert(pearson < 0.2);
                            //}
                        }

                        // Partition function Z_ui
                        double Z_ui = 0;
                        List<double> localLikelihoods = new List<double>(ratingLevels);

                        Object lockMe = new object();
                        for (int targetRating = 1; targetRating <= ratingLevels; targetRating++)
                        {
                            double Z_ui_level = OMFDistributions[new Tuple<int, int>(indexOfUser, indexOfItem_i)][targetRating-1]
                                * ComputePotential(targetRating, indexOfUser, indexOfItem_i, neighborsOfItem_i);
                            lock(lockMe)
                            {
                                Z_ui += Z_ui_level;
                            }
                        }

                        for (int targetRating = 1; targetRating <= ratingLevels; targetRating++)
                        {
                            //for (int targetRating = 1; targetRating <= ratingLevels; targetRating++)
                            //{
                            // The reason we need to compute the local likelihood for every i-j pair
                            // instead of once for i is that the weights are changing 
                            // TODO: however, it seems that the changed weights are not related to
                            // this locallikelihood, which means it can be put outside of the i-j loop?
                            // Because after we updated i, i should never be updated again by this user in this epoch
                            // TODO: so we try move it out side the j loop
                            // Experiment shows we are correct
                            double localLikelihoodOfTargetRating = ComputeLocalLikelihood(targetRating, indexOfUser,
                                indexOfItem_i, neighborsOfItem_i, Z_ui);
                            lock (lockMe)
                            {
                                localLikelihoods.Add(localLikelihoodOfTargetRating);
                            }
                        }

                        // For each neighbor item with strong correlation to item_i,
                        // update the weight w_ij
                        foreach (int indexOfItem_j in neighborsOfItem_i)
                        {
                            // As i-j and j-i correspond to the same feature, 
                            // so we train only if i < j to avoid double training
                            if (indexOfItem_i > indexOfItem_j) { continue; }

                            // If the similarity is zero then it is a weak feature and we skip it
                            // recall that we have set weak similarity to zero
                            // if (similarityByItemItem[indexOfItem_i, indexOfItem_j] == SparseMatrix.Zero) { continue; }
                            // we don't need to do this now, the filtering has been done before the loop

                            // Compute gradient Eq. 24
                            #region Compute gradients
                            double r_uj = R_train[indexOfUser, indexOfItem_j];
                            double meanOfItem_j = meanByItem[indexOfItem_j];

                            // Compute the first term in Eq.24
                            double gradientFirstTerm = ComputeCorrelationFeature(r_ui, meanOfItem_i, r_uj, meanOfItem_j);

                            // Compute the second term in Eq. 24
                            double gradientSecondTerm = 0.0;
                            for (int targetRating = 1; targetRating <= ratingLevels; targetRating++)
                            {
                                // The reason we need to compute the local likelihood for every i-j pair
                                // instead of once for i is that the weights are changing 
                                // TODO: however, it seems that the changed weights are not related to
                                // this locallikelihood, which means it can be put outside of the i-j loop?
                                // Because after we updated i, i should never be updated again by this user in this epoch
                                // TODO: so we try move it out side the j loop once the algorithm is table
                                //double localLikelihoodOfTargetRating = ComputeLocalLikelihood(targetRating, indexOfUser, indexOfItem_i, neighborsOfItem_i, Z_ui);

                                double localLikelihoodOfTargetRating = localLikelihoods[targetRating - 1];
                                double correlationFeature = ComputeCorrelationFeature(targetRating, meanOfItem_i, r_uj, meanOfItem_j);
                                gradientSecondTerm += localLikelihoodOfTargetRating * correlationFeature;
                            }

                            // Merge all terms
                            double gradient = gradientFirstTerm - gradientSecondTerm;

                            #endregion

                            #region Update weights

                            // Add regularization penalty, it should be shown in either Eq. 23 or Eq. 24
                            double weight = featureWeightByItemItem[new Tuple<int,int>( indexOfItem_i, indexOfItem_j)];
                            gradient -= regularization * weight;
                            double step = learnRate * gradient; // Add learning rate

                            // Update the weight with gradient
                            featureWeightByItemItem[new Tuple<int, int>(indexOfItem_i, indexOfItem_j)] += step;

                            // The weights are mirrored
                            featureWeightByItemItem[new Tuple<int, int>(indexOfItem_j, indexOfItem_i)] += step;

                            #endregion
                        }
                    }
                }
                #endregion

                /************************************************************
                 *   Compute the regularized sum of log local likelihoods, Eq. 20
                 *   see if it converges
                ************************************************************/
                #region Compute sum of regularized log likelihood see if it converges

                if (epoch == 0 || epoch == maxEpoch - 1 || epoch % (int)Math.Ceiling(maxEpoch * 0.1) == 4)
                //if (true)
                {
                    double likelihood_curr = 0;
                    // We compute user by user so that Z_ui can be reused
                    double sumOfLogLL = 0.0;   // sum of log local likelihoods, first term in Eq. 20
                    foreach (var user in R_train.Users)
                    {
                        int indexOfUser = user.Item1;
                        Vector<double> ratingsOfUser = user.Item2;
                        Debug.Assert(ratingsOfUser.Storage.IsDense == false, "The user ratings should be stored in a sparse vector.");

                        List<int> itemsOfUser = itemsByUser[indexOfUser];   // Cache the items rated by this user
                        double logLLOfUser = 0.0;   // The sum of all Eq. 21 of the current user

                        foreach (var item_i in ratingsOfUser.EnumerateIndexed(Zeros.AllowSkip))
                        {
                            int indexOfItem_i = item_i.Item1;
                            int r_ui = (int)R_train[indexOfUser, indexOfItem_i];    // The R_train should be all integers
                            double meanOfItem_i = meanByItem[indexOfItem_i];

                            // Find out the neighbors of item_i, i.e., "\vec{r}_u\r_ui" in Eq. 21
                            //List<int> neighborsOfItem_i = new List<int>(itemsOfUser);
                            List<int> neighborsOfItem_i = new List<int>(itemsOfUser.Count);

                            //neighborsOfItem_i.Remove(indexOfItem_i);    // It is not a neighbor of itself

                            // Remove weak neighbors
                            foreach (int indexOfNeighbor in itemsOfUser)
                            {
                                if (strongSimilarityIndicators.Contains(new Tuple<int, int>(indexOfItem_i, indexOfNeighbor))
                                    &&indexOfNeighbor!= indexOfItem_i)
                                {
                                    neighborsOfItem_i.Add(indexOfNeighbor);
                                    //neighborsOfItem_i.Remove(indexOfNeighbor);
                                }
                            }

                            // Partition function Z_ui
                            double Z_ui = 0;
                            for (int targetRating = 1; targetRating <= ratingLevels; targetRating++)
                            {
                                Z_ui += OMFDistributions[new Tuple<int, int>(indexOfUser, indexOfItem_i)][targetRating - 1]
                                    * ComputePotential(targetRating, indexOfUser, indexOfItem_i, neighborsOfItem_i);
                            }

                            // Eq. 21 for the current item i, that is for r_ui
                            double localLikelihoodOfRating_ui = ComputeLocalLikelihood(r_ui, indexOfUser, indexOfItem_i, neighborsOfItem_i, Z_ui);
                            logLLOfUser += Math.Log(localLikelihoodOfRating_ui);
                        }
                        sumOfLogLL += logLLOfUser;
                    }

                    // Eq. 20
                    double regularizedSumOfLogLL = sumOfLogLL - regularization 
                        * featureWeightByItemItem.Sum(x => x.Value * x.Value);// featureWeightByItemItem.SquaredSum();
                    likelihood_curr = regularizedSumOfLogLL;
                    Utils.PrintEpoch("Epoch", epoch, maxEpoch, "Reg sum of log LL", regularizedSumOfLogLL.ToString("0.000"));

                    double improvment = Math.Abs(likelihood_prev) - Math.Abs(likelihood_curr);
                    if (improvment < 0.001)
                    {
                        Console.WriteLine("Improvment less than 0.0001, learning stopped.");
                        break;
                    }

                    likelihood_prev = likelihood_curr;
                }


                /*
                if(epoch==0)
                {
                    likelihood_prev = likelihood_curr;
                }
                else 
                {
                    double improvment = likelihood_curr - likelihood_prev;
                    if(!(improvment < 0 && likelihood_prev < 0 && Math.Abs(improvment) > 0.001))
                    {

                    }

                    if (Math.Abslikelihood_curr - likelihood_prev < 0.0001)
                    {
                        Console.WriteLine("Improvment less than 0.0001, learning stopped.");
                        break;
                    }
                }
                */
 
                #endregion
            }
            #endregion

            /************************************************************
             *   Make predictions
            ************************************************************/
            #region Make predictions

            foreach(var user in R_unknown.Users)
            {
                int indexOfUser = user.Item1;
                Vector<double> unknownRatingsOfUser = user.Item2;
                List<int> itemsOfUser = itemsByUser[indexOfUser];

                foreach(var unknownRating in unknownRatingsOfUser.EnumerateIndexed(Zeros.AllowSkip))
                {
                    int indexOfItem = unknownRating.Item1;

                    if (double.IsNaN(meanByItem[indexOfItem]))
                    {
                        int stop = 1;
                    }

                    List<int> neighborsOfItem = new List<int>(itemsOfUser.Count);
                    //neighborsOfItem.Remove(indexOfItem);    // It is not a neighbor of itself
                    // Remove weak neighbors
                    foreach (int indexOfNeighbor in itemsOfUser)
                    {
                        if (strongSimilarityIndicators.Contains(new Tuple<int, int>(indexOfItem, indexOfNeighbor))
                            && indexOfNeighbor!= indexOfItem)
                        {
                            neighborsOfItem.Add(indexOfNeighbor);
                        }
                    }

                    // Partition function Z
                    double Z_ui = 0;
                    for (int targetRating = 1; targetRating <= ratingLevels; targetRating++)
                    {
                        double potential = ComputePotential(targetRating, indexOfUser, indexOfItem, neighborsOfItem);
                        //if (double.IsNaN(potential)) { potential = 1; }
                        Z_ui += OMFDistributions[new Tuple<int, int>(indexOfUser, indexOfItem)][targetRating - 1] * potential;
                        if(double.IsNaN(Z_ui))
                        {
                            int stop = 1;
                        }
                    }

                    double sumOfLikelihood = 0.0;
                    double currentMaxLikelihood = 0.0;
                    double mostlikelyRating = 0.0;
                    double expectationRating = 0.0;
                    for (int targetRating = 1; targetRating <= ratingLevels; targetRating++)
                    {
                        double likelihoodOfTargetRating = ComputeLocalLikelihood(targetRating, indexOfUser, indexOfItem, neighborsOfItem, Z_ui);

                        // Compute the most likely rating for MAE
                        if (likelihoodOfTargetRating > currentMaxLikelihood)
                        {
                            mostlikelyRating = targetRating;
                            currentMaxLikelihood = likelihoodOfTargetRating;
                        }

                        // Compute expectation for RMSE
                        expectationRating += targetRating * likelihoodOfTargetRating;

                        sumOfLikelihood += likelihoodOfTargetRating;
                    }

                    // The sum of likelihoods should be 1, maybe not that high precision though
                    Debug.Assert(Math.Abs(sumOfLikelihood - 1.0) < 0.0001);

                    R_predicted_expectations[indexOfUser, indexOfItem] = expectationRating;
                    R_predicted_mostlikely[indexOfUser, indexOfItem] = mostlikelyRating;
                }
            }

            #endregion

        }

        #region Compute the potentials
        private double ComputePotential(double targetRating, int indexOfUser, int indexOfItem_i, List<int> neighborsOfItem_i)
        {
            // Correlation potential
            double totalCorrelationPotential = 0;
            foreach (int indexOfNeighbor in neighborsOfItem_i)
            {
                double correlationFeature = ComputeCorrelationFeature(targetRating, meanByItem[indexOfItem_i],
                    R_train[indexOfUser, indexOfNeighbor], meanByItem[indexOfNeighbor]);

                //double strength = similarityByItemItem[indexOfItem_i, indexOfNeighbor];

                double weight = featureWeightByItemItem[new Tuple<int,int>( indexOfItem_i, indexOfNeighbor)];

                // We should not have 0 weight for two reasons:
                // zero weight means it never get initialized, which means there is 
                // no edge (two items rated by the same user) between the corresponding
                // items. However, we do have a very rare chance that the weight happended
                // to be randomly assigned/updated to 0
                Debug.Assert(weight != 0);

                //totalCorrelationPotential *= Math.Exp(correlationFeature * weight);
                // TODO: the implementation seems to be wrong in the previous paper?
                // I changed it here
                totalCorrelationPotential += correlationFeature * weight; // The summation in Eq. 21 
            }
            if (double.IsNaN(totalCorrelationPotential))
                totalCorrelationPotential = 1;
            return Math.Exp(totalCorrelationPotential);
        }

        #endregion

        #region Compute correlation features
        private double ComputeCorrelationFeature(double r_ui, double ave_i, double r_uj, double ave_j)
        {
            double feature_ij = NormalizeFeatureValue(Math.Abs((r_ui - ave_i) - (r_uj - ave_j)));
            //Debug.Assert(feature_ij >= 0 && feature_ij <= 1);
            return feature_ij;
        }
        #endregion

        #region Normalize feature values
        private double NormalizeFeatureValue(double value)
        {
            return 1 - value / (3.0);
            //return -1.0 / (1.0 + Math.Exp(-value));
        }
        #endregion

        #region Compute local likelihood
        // Eq. 21
        private double ComputeLocalLikelihood(int targetRating, int indexOfUser, int indexOfItem, List<int> neighborsOfItem, double Z_ui)
        {
            // The right hand side exp(summation) term
            double potential = ComputePotential(targetRating, indexOfUser, indexOfItem, neighborsOfItem);

            // enumerator of Eq. 21
            double numerator = OMFDistributions[new Tuple<int,int>(indexOfUser,indexOfItem)][targetRating-1] * potential;

            // TODO: Sth wrong with the numerator? NaN?
            if (numerator > Z_ui)
            {
                Debug.Assert(numerator <= Z_ui);
            }

            return numerator / Z_ui;
        }
        #endregion
    }
}
