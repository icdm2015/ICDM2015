using MathNet.Numerics.LinearAlgebra;
using LibRecSysCS.Absolute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;

namespace LibRecSysCS.Relative
{
    class CRF
    {
        Vector<double> meanByUser;
        Vector<double> meanByItem;
        DataMatrix R_train;
        HashSet<Tuple<int, int>> strongSimilarityIndicators;
        Dictionary<Tuple<int, int>, List<double>> OMFDistributions;
        Dictionary<Tuple<int, int>, double> correlationWeightByItemItem; // featureWeightByItemItem[i,j] is w_ij
        Dictionary<int, Vector<double>> attributeWeightsByUser;
        Dictionary<int, Vector<double>> attributeWeightsByItem;

        public void PredictRatings(
            DataMatrix R_train,
            DataMatrix R_unknown,
            Dictionary<int, List<double>> attributesByUser,
            Dictionary<int, List<double>> attributesByItem,
            HashSet<Tuple<int,int>> strongSimilarityIndicators, 
            Dictionary<Tuple<int, int>, List<double>> OMFDistributions, 
            double regularization, 
            double learnRate, 
            int maxEpoch, 
            int ratingLevels, 
            out DataMatrix R_predicted_expectations, 
            out DataMatrix R_predicted_mostlikely,
            int seed)
        {
            /************************************************************
             *   Parameterization and Initialization
            ************************************************************/
            #region Parameterization and Initialization
            int userCount = R_train.UserCount;
            int itemCount = R_train.ItemCount;
            int userAttributeCount = attributesByUser.ElementAt(0).Value.Count;
            int itemAttributeCount = attributesByItem.ElementAt(0).Value.Count;
            meanByUser = R_train.GetUserMeans();
            meanByItem = R_train.GetItemMeans();
            this.R_train = R_train;
            this.OMFDistributions = OMFDistributions;
            this.strongSimilarityIndicators = strongSimilarityIndicators;
            R_predicted_expectations = new DataMatrix(userCount, itemCount);
            R_predicted_mostlikely = new DataMatrix(userCount, itemCount);
            
            // Initialize weights
            correlationWeightByItemItem = new Dictionary<Tuple<int, int>, double>(strongSimilarityIndicators.Count);
            attributeWeightsByUser = new Dictionary<int, Vector<double>>(userCount);
            attributeWeightsByItem = new Dictionary<int, Vector<double>>(itemCount);
            Random rnd = new Random(seed);
            foreach(var strongSimilarityPair in strongSimilarityIndicators)
            {
                correlationWeightByItemItem[strongSimilarityPair] = rnd.NextDouble() * 0.01;
            }
            for(int indexOfUser=0; indexOfUser < userCount;indexOfUser++)
            {
                attributeWeightsByUser[indexOfUser] = Vector.Build.DenseOfEnumerable(
                    Enumerable.Range(0, itemAttributeCount).Select(n => rnd.NextDouble() * 0.01).ToList());
            }
            for (int indexOfItem = 0; indexOfItem < itemCount; indexOfItem++)
            {
                attributeWeightsByItem[indexOfItem] = Vector.Build.DenseOfEnumerable(
                    Enumerable.Range(0, userAttributeCount).Select(n => rnd.NextDouble() * 0.01).ToList());
            }
            
            Utils.PrintValue("# of item-item features", (correlationWeightByItemItem.Count / 2).ToString());
            #endregion

            /************************************************************
             *   Learn weights from training data R_train
            ************************************************************/
            #region Learn weights from training data R_train
            // Cache which items have been rated by the given user
            // it be reused in every feature update
            Dictionary<int, List<int>> itemsByUser = R_train.GetItemsByUser();
            double likelihood_prev = -double.MaxValue;
            for (int epoch = 0; epoch < maxEpoch; epoch++)
            {
                /************************************************************
                 *   Train with the set of ratings by each user
                ************************************************************/
                #region Apply Eq. 23 and 24
                // Each user's ratings is a batch
                foreach (var user in R_train.Users)
                {
                    int indexOfUser = user.Item1;
                    Vector<double> ratingsOfUser = user.Item2;
                    List<int> itemsOfUser = itemsByUser[indexOfUser];   // Cache the items rated by this user
                    Debug.Assert(ratingsOfUser.Storage.IsDense == false);

                    // Now we select one rating r_ui from the user's ratings R_u,
                    // and use this rating to combine with each other rating r_uj in R_u
                    // so that we can refine the weight associated to i-j item pair co-rated by this user
                    foreach (var item_i in ratingsOfUser.EnumerateIndexed(Zeros.AllowSkip))
                    {
                        int indexOfItem_i = item_i.Item1;
                        int r_ui = (int)R_train[indexOfUser, indexOfItem_i];    // The R_train should be all integers
                        double meanOfItem_i = meanByItem[indexOfItem_i];


                        #region Identify strong neighbors
                        // Find out the strong neighbors of item_i, i.e., "\vec{p}_u\p_ui" in Eq. 21
                        List<int> neighborsOfItem_i = new List<int>(itemsOfUser.Count);
                        foreach (int indexOfNeighbor in itemsOfUser)
                        {
                            if (strongSimilarityIndicators.Contains(new Tuple<int,int>(indexOfItem_i, indexOfNeighbor))
                                && indexOfNeighbor != indexOfItem_i)
                            {
                                neighborsOfItem_i.Add(indexOfNeighbor);
                            }
                        }
                        #endregion

                        #region Compute partition function Z_ui and local likelihoods
                        double Z_ui = 0;
                        List<double> localLikelihoods = new List<double>(ratingLevels);

                        for (int targetRating = 1; targetRating <= ratingLevels; targetRating++)
                        {
                            double Z_ui_level = OMFDistributions[new Tuple<int, int>(indexOfUser, indexOfItem_i)][targetRating - 1]
                                * ComputePotential(targetRating, indexOfUser, indexOfItem_i, neighborsOfItem_i,
                            attributesByUser[indexOfUser],
                            attributesByItem[indexOfItem_i]);

                            Z_ui += Z_ui_level;

                        }

                        for (int targetRating = 1; targetRating <= ratingLevels; targetRating++)
                        {
                            double localLikelihoodOfTargetRating = ComputeLocalLikelihood(targetRating, indexOfUser,
                                indexOfItem_i, neighborsOfItem_i, Z_ui,
                            attributesByUser[indexOfUser],
                            attributesByItem[indexOfItem_i]);

                            localLikelihoods.Add(localLikelihoodOfTargetRating);
                        }
                        #endregion

                        /************************************************************
                         *   Update attribute and correlation weights
                        ************************************************************/

                        /************************************************************
                         *   Update attribute feature
                        ************************************************************/
                        #region Attribute feature
                        
                        // First term
                        Vector<double>[] attributeGradientFirstTerm = ComputeAttributeFeature(r_ui,
                            attributesByUser[indexOfUser],
                            attributesByItem[indexOfItem_i],
                            meanByUser[indexOfUser],
                            meanOfItem_i);

                        // Second term
                        Vector<double> attributeGradientSecondTerm_f_u = Vector.Build.Dense(itemAttributeCount);
                        Vector<double> attributeGradientSecondTerm_f_i = Vector.Build.Dense(userAttributeCount);
                        for (int targetRating = 1; targetRating <= ratingLevels; targetRating++)
                        {
                            double localLikelihoodOfTargetRating = localLikelihoods[targetRating - 1];
                            Vector<double>[] attributeFeature = ComputeAttributeFeature(targetRating,
                            attributesByUser[indexOfUser],
                            attributesByItem[indexOfItem_i],
                            meanByUser[indexOfUser],
                            meanOfItem_i);
                            attributeGradientSecondTerm_f_u += attributeFeature[0] * localLikelihoodOfTargetRating;
                            attributeGradientSecondTerm_f_i += attributeFeature[1] * localLikelihoodOfTargetRating;
                        }

                        // Gradients for attribute weights
                        Vector<double> attributeGradient_f_u = attributeGradientFirstTerm[0] - attributeGradientSecondTerm_f_u;
                        Vector<double> attributeGradient_f_i = attributeGradientFirstTerm[1] - attributeGradientSecondTerm_f_i;

                        // Update weights 
                        attributeGradient_f_u -= attributeWeightsByUser[indexOfUser] * regularization;
                        attributeGradient_f_i -= attributeWeightsByItem[indexOfItem_i] * regularization;
                        attributeWeightsByUser[indexOfUser] += attributeGradient_f_u * learnRate;
                        attributeWeightsByItem[indexOfItem_i] += attributeGradient_f_i * learnRate;
                        #endregion

                        /************************************************************
                         *   Update correlation feature
                        ************************************************************/
                        #region Correlation feature
                        // For each neighbor item with strong correlation to item_i update the weight w_ij
                        foreach (int indexOfItem_j in neighborsOfItem_i)
                        {
                            // As i-j and j-i correspond to the same feature, 
                            // so we train only for i < j to avoid double training
                            if (indexOfItem_i > indexOfItem_j) { continue; }
                            double r_uj = R_train[indexOfUser, indexOfItem_j];
                            double meanOfItem_j = meanByItem[indexOfItem_j];

                            // First term in Eq.24
                            double gradientFirstTerm = ComputeCorrelationFeature(r_ui, meanOfItem_i, r_uj, meanOfItem_j);

                            // Second term in Eq. 24
                            double gradientSecondTerm = 0.0;
                            for (int targetRating = 1; targetRating <= ratingLevels; targetRating++)
                            {
                                double localLikelihoodOfTargetRating = localLikelihoods[targetRating - 1];
                                double correlationFeature = ComputeCorrelationFeature(targetRating, meanOfItem_i, r_uj, meanOfItem_j);
                                gradientSecondTerm += localLikelihoodOfTargetRating * correlationFeature;
                            }

                            // Gradient for correlation weight
                            double gradient = gradientFirstTerm - gradientSecondTerm;
                         
                            // Update weight
                            double weight = correlationWeightByItemItem[new Tuple<int,int>( indexOfItem_i, indexOfItem_j)];
                            gradient -= regularization * weight;
                            double step = learnRate * gradient;

                            // Update the weight with gradient
                            correlationWeightByItemItem[new Tuple<int, int>(indexOfItem_i, indexOfItem_j)] += step;
                            // The weights are mirrored
                            correlationWeightByItemItem[new Tuple<int, int>(indexOfItem_j, indexOfItem_i)] += step;
                        }
                        #endregion
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

                            // Find out strong neighbors of item_i, i.e., "\vec{r}_u\r_ui" in Eq. 21
                            List<int> neighborsOfItem_i = new List<int>(itemsOfUser.Count);
                            foreach (int indexOfNeighbor in itemsOfUser)
                            {
                                if (strongSimilarityIndicators.Contains(new Tuple<int, int>(indexOfItem_i, indexOfNeighbor))
                                    &&indexOfNeighbor!= indexOfItem_i)
                                {
                                    neighborsOfItem_i.Add(indexOfNeighbor);
                                }
                            }

                            // Partition function Z_ui
                            double Z_ui = 0;
                            for (int targetRating = 1; targetRating <= ratingLevels; targetRating++)
                            {
                                Z_ui += OMFDistributions[new Tuple<int, int>(indexOfUser, indexOfItem_i)][targetRating - 1]
                                    * ComputePotential(targetRating, indexOfUser, indexOfItem_i, neighborsOfItem_i,
                            attributesByUser[indexOfUser],
                            attributesByItem[indexOfItem_i]);
                            }

                            // Eq. 21 for the current item i, that is for r_ui
                            double localLikelihoodOfRating_ui = ComputeLocalLikelihood(r_ui, indexOfUser, indexOfItem_i, neighborsOfItem_i, Z_ui,
                            attributesByUser[indexOfUser],
                            attributesByItem[indexOfItem_i]);
                            logLLOfUser += Math.Log(localLikelihoodOfRating_ui);
                        }
                        sumOfLogLL += logLLOfUser;
                    }

                    // Eq. 20
                    double regularizedSumOfLogLL = sumOfLogLL - regularization 
                        * correlationWeightByItemItem.Sum(x => x.Value * x.Value);
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

                    // Skip items without any ratings in the training set
                    //if (double.IsNaN(meanByItem[ indexOfItem]))
                    //{
                    //    int stop = 1;
                    //}

                    // Find strong neighbors
                    List <int> neighborsOfItem = new List<int>(itemsOfUser.Count);
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
                        Z_ui += OMFDistributions[new Tuple<int, int>(indexOfUser, indexOfItem)][targetRating - 1] * 
                            ComputePotential(targetRating, indexOfUser, indexOfItem, neighborsOfItem,
                            attributesByUser[indexOfUser],
                            attributesByItem[indexOfItem]);
                    }

                    double sumOfLikelihood = 0.0;
                    double currentMaxLikelihood = 0.0;
                    double mostlikelyRating = 0.0;
                    double expectationRating = 0.0;
                    for (int targetRating = 1; targetRating <= ratingLevels; targetRating++)
                    {
                        double likelihoodOfTargetRating = ComputeLocalLikelihood(targetRating, indexOfUser, indexOfItem, neighborsOfItem, Z_ui,
                            attributesByUser[indexOfUser],
                            attributesByItem[indexOfItem]);

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
        private double ComputePotential(double targetRating, 
            int indexOfUser, 
            int indexOfItem_i, 
            List<int> neighborsOfItem_i,
            List<double> attributesOfUser,
            List<double> attributesOfItem)
        {
            // Specially for cold-item that never seen in the train set
            if(neighborsOfItem_i.Count==0)
            {
                return Math.Exp(0);
            }

            // attribute potential
            double attributePotential = 0.0;
            Vector<double>[] attributeFeatures = ComputeAttributeFeature(targetRating, attributesOfUser,
                attributesOfItem, meanByUser[indexOfUser], meanByItem[indexOfItem_i]);
            attributePotential += attributeFeatures[0].DotProduct(attributeWeightsByUser[indexOfUser]);
            attributePotential += attributeFeatures[1].DotProduct(attributeWeightsByItem[indexOfItem_i]);
            
            // Correlation potential
            double totalCorrelationPotential = 0;
            foreach (int indexOfNeighbor in neighborsOfItem_i)
            {
                double correlationFeature = ComputeCorrelationFeature(targetRating, meanByItem[indexOfItem_i],
                    R_train[indexOfUser, indexOfNeighbor], meanByItem[indexOfNeighbor]);

                //double strength = similarityByItemItem[indexOfItem_i, indexOfNeighbor];

                double weight = correlationWeightByItemItem[new Tuple<int,int>( indexOfItem_i, indexOfNeighbor)];

                // We should not have 0 weight for two reasons:
                // zero weight means it never get initialized, which means there is 
                // no edge (two items rated by the same user) between the corresponding
                // items. However, we do have a very rare chance that the weight happended
                // to be randomly assigned/updated to 0
                Debug.Assert(weight != 0);

                // totalCorrelationPotential *= Math.Exp(correlationFeature * weight);
                // TODO: the above old implementation seems to be wrong 
                totalCorrelationPotential += correlationFeature * weight; // The summation in Eq. 21 
            }
            return Math.Exp(attributePotential + totalCorrelationPotential);
        }

        #endregion

        #region Compute correlation feature
        private double ComputeCorrelationFeature(double r_ui, double ave_i, double r_uj, double ave_j)
        {
            double feature_ij = NormalizeFeatureValue(Math.Abs((r_ui - ave_i) - (r_uj - ave_j)));
            //Debug.Assert(feature_ij >= 0 && feature_ij <= 1);
            return feature_ij;
        }
        #endregion

        #region Compute attribute features
        private Vector<double>[] ComputeAttributeFeature(double r_ui, List<double> attributesOfUser,
            List<double> attributesOfItem, double meanOfUser, double meanOfItem)
        {
            Vector<double> f_u = Vector.Build.DenseOfEnumerable(attributesOfItem);
            Vector<double> f_i = Vector.Build.DenseOfEnumerable(attributesOfUser);

            f_u = f_u * NormalizeFeatureValue(Math.Abs((r_ui - meanOfUser)));
            f_i = f_i * NormalizeFeatureValue(Math.Abs((r_ui - meanOfItem)));
            return new Vector<double>[] { f_u, f_i };
        }
        #endregion

        #region Normalize feature values
        private double NormalizeFeatureValue(double value)
        {
            return 1.0 - value / 3.0;
            //return 1.0 / (1.0 + Math.Exp(-value));
        }
        #endregion

        #region Compute local likelihood
        // Eq. 21
        private double ComputeLocalLikelihood(int targetRating, int indexOfUser, int indexOfItem, List<int> neighborsOfItem, double Z_ui,
                            List<double> attributesOfUser,
                            List<double> attributesOfItem)
        {
            // The right hand side exp(summation) term
            double potential = ComputePotential(targetRating, indexOfUser, indexOfItem, neighborsOfItem,
                            attributesOfUser,
                            attributesOfItem);

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
