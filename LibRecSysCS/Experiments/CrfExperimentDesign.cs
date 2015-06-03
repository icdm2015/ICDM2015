using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRecSysCS.Experiments
{
    public class CrfExpDesign
    {
        #region Shared configurations
        static int KnnNeighborCount = 50;
        static int maxSimilarityCount = 200;    // We only store the similarity values for the top 200 neighbors
        static int factorCount = 50;
        static string MovieLens100KRatings = "MovieLens100K.data";
        static string MovieLens1MRatings = "MovieLens1M.data";
        static string MovieLens20M = "ml20m_active_5y.csv";
        static string EachMovie = "EachMovie.data";
        static string Amazon = "amazon_2k_users.csv";
        static string MovieLens100KUserAttributes = "DataSets/MovieLens100K.user";
        static string MovieLens100KItemAttributes = "DataSets/MovieLens100K.item";
        static string MovieLens1MUserAttributes = "DataSets/MovieLens1M.user";
        static string MovieLens1MItemAttributes = "DataSets/MovieLens1M.item";
        //static List<int> givenSizesMovieLens100K = new List<int>() { 30, 40, 50, 60 };
        static List<int> givenSizesMovieLens100K = new List<int>() { 40};
        static List<int> givenSizesMovieLens1M = new List<int>() { 30, 40, 50, 60 };
        //static List<double> MovieLens20MBiases = new List<double>() { 0, 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.07, 0.08, 0.09, 0.1 };
        static List<double> MovieLens20MBiases = new List<double>() { 0.2, 0.4, 0.6, 0.8, 1.0, 1.2, 1.4, 1.6, 1.8, 2.0 };

        static List<int> givenSizesEachMovie = new List<int>() { 60 };
        static List<int> givenSizesAmazon = new List<int>() { 50 };
        static int userCountEachMovie = 3000;
        static bool split = true;
        static int minTestSize = 10;    // Min number of ratings reserved for testing
        static bool shuffle = true;
        static double relevantCriteria = 5;
        static double relevantCriteriaMovieLens20M = 4.5;
        static double relevantCriteriaEachMovie = 6;
        static double relevantCriteriaAmazon = 5;
        static int topN = 10;
        static int maxCountOfRatings = 500;
        static int initialSeed = 1;
        static int finalSeed = 1;
        static double minStrongCorrelationThreshold = 0.05;
        #endregion

        #region Most popular on MovieLens100K
        public static void MostPopularOnMovieLens100K()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens100K)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens100KRatings,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    string log = experiment.RunMostPopular(topN);
                }
            }
        }
        #endregion

        #region NMF on Movielens100K
        public static void NMFonMovieLens100K()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens100K)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens100KRatings,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    string log = experiment.RunNMF(100, 0.01, 0.05, 50, topN);
                    using (StreamWriter w = File.AppendText("NMFonMovieLens100K.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region UserKNN on MovieLens100K
        public static void UserKNNonMovieLens100K()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens100K)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens100KRatings,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    string log = experiment.RunUserKNN(10);
                    using (StreamWriter w = File.AppendText("UserKNNonMovieLens100K.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region PrefKnn on MovieLens100K
        public static void PrefKNNonMovieLens100K()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens100K)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens100KRatings,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);
                    string log = experiment.RunPrefKNN(10);
                    using (StreamWriter w = File.AppendText("PrefKNNonMovieLens100K.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region PrefNMF on MovieLens100K
        public static void PrefNMFonMovieLens100K()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens100K)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens100KRatings,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);
                    string log = experiment.RunPrefNMF(50, 0.01, 0.04, 0.02, factorCount, topN);
                    using (StreamWriter w = File.AppendText("PrefNMFonMovieLens100K.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region PrefMRF on MovieLens100K
        public static void PrefMRFonMovieLens100K()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens100K)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens100KRatings,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    string log = experiment.RunPrefMRF(0.01, 0.005, 100, new List<double> { 1, 2, 3 }, 10);
                    if(log=="abort")
                    {
                        PrefOMFonMovieLens100K();
                        log = experiment.RunPrefMRF(0.01, 0.005, 100, new List<double> { 1, 2, 3 }, 10);
                    }
                    
                    using (StreamWriter w = File.AppendText("PrefMRFonMovieLens100K.txt"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region PrefOMF on MovieLens100K
        public static void PrefOMFonMovieLens100K()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens100K)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens100KRatings,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    string log = experiment.RunPrefNMFbasedOMF(50, 0.01, 0.04, 0.02, factorCount, new List<double> { 1, 2, 3 }, topN);
                    using (StreamWriter w = File.AppendText("PrefOMFonMovieLens100K.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region PrefCRF on MovieLens100K
        public static void PrefCRFonMovieLens100K()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens100K)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens100KRatings,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    experiment.UserAttributesFile = MovieLens100KUserAttributes;
                    experiment.ItemAttributesFile = MovieLens100KItemAttributes;

                    string log = experiment.RunPrefCRF(0.01, 0.005, 100, new List<double> { 1, 2, 3 },10);
                    if (log == "abort")
                    {
                        PrefOMFonMovieLens100K();
                        log = experiment.RunPrefCRF(0.01, 0.005, 100, new List<double> { 1, 2, 3 }, 10);
                    }

                    using (StreamWriter w = File.AppendText("PrefCRFonMovieLens100K.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion


        #region PrefOMF on MovieLens1M
        public static void PrefOMFonMovieLens1M(int fix_seed, int fix_given)
        {
            ExperimentEngine experiment = new ExperimentEngine(
                MovieLens1MRatings,
                minTestSize,
                maxCountOfRatings,
                fix_given,
                shuffle,
                fix_seed,
                relevantCriteria,
                maxSimilarityCount,
                KnnNeighborCount,
                minStrongCorrelationThreshold);

            experiment.UserAttributesFile = MovieLens1MUserAttributes;
            experiment.ItemAttributesFile = MovieLens1MItemAttributes;

            string log = experiment.RunPrefNMFbasedOMF(50, 0.01, 0.04, 0.02, factorCount, new List<double> { 1, 2, 3 }, topN);
            using (StreamWriter w = File.AppendText("PrefOMFonMovieLens1N_with_attributes.log"))
            {
                w.WriteLine(experiment.GetDataFileName(""));
                w.WriteLine(experiment.FormattedConfigurations());
                w.WriteLine("=========================================" + fix_seed + "/" + fix_given);
                w.WriteLine(log.ToString());
            }
        }
        #endregion

        #region PrefCRF on MovieLens1M
        public static void PrefCRFonMovieLens1M()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens1M)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens1MRatings,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    experiment.UserAttributesFile = MovieLens1MUserAttributes;
                    experiment.ItemAttributesFile = MovieLens1MItemAttributes;

                    string log = experiment.RunPrefCRF(0.01, 0.005, 100, new List<double> { 1, 2, 3 }, 10);
                    if (log == "abort")
                    {
                        PrefOMFonMovieLens1M(seed, givenSize);
                        log = experiment.RunPrefCRF(0.01, 0.005, 100, new List<double> { 1, 2, 3 }, 10);
                    }

                    using (StreamWriter w = File.AppendText("PrefCRFonMovieLens1M_with_attributes.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion




        #region PrefNMF on EachMovie
        public static void PrefNMFonEachMovie()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesEachMovie)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        EachMovie,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteriaEachMovie,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    experiment.splitTrain = split;
                    experiment.someUsers = userCountEachMovie;

                    string log = experiment.RunPrefNMF(50, 0.01, 0.04, 0.02, factorCount, topN);
                    using (StreamWriter w = File.AppendText("PrefNMFonEachMovie.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region PrefOMF on EachMovie
        public static void PrefOMFonEachMovie()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesEachMovie)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        EachMovie,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteriaEachMovie,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    experiment.splitTrain = split;
                    experiment.someUsers = userCountEachMovie;

                    string log = experiment.RunPrefNMFbasedOMF(50, 0.01, 0.04, 0.02, factorCount, new List<double> { 1, 2, 3 }, topN);
                    using (StreamWriter w = File.AppendText("PrefOMFonEachMovie.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region PrefMRF on EachMovie
        public static void PrefMRFonEachMovie()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesEachMovie)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        EachMovie,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteriaEachMovie,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    experiment.splitTrain = split;
                    experiment.someUsers = userCountEachMovie;

                    string log = experiment.RunPrefMRF(0.01, 0.005, 100, new List<double> { 1, 2, 3 }, 10);
                    if (log == "abort")
                    {
                        PrefOMFonEachMovie();
                        log = experiment.RunPrefMRF(0.01, 0.005, 100, new List<double> { 1, 2, 3 }, 10);
                    }

                    using (StreamWriter w = File.AppendText("PrefCRFonEachMovie.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region NMF on EachMovie
        public static void NMFonEachMovie()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesEachMovie)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        EachMovie,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteriaEachMovie,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    experiment.splitTrain = split;
                    experiment.someUsers = userCountEachMovie;

                    string log = experiment.RunNMF(100, 0.01, 0.05, 50, topN, 6.0);
                    using (StreamWriter w = File.AppendText("NMFonEachMovie.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region UserKNN on EachMovie
        public static void UserKNNonEachMovie()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesEachMovie)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        EachMovie,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteriaEachMovie,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    experiment.splitTrain = split;
                    experiment.someUsers = userCountEachMovie;

                    string log = experiment.RunUserKNN(10);
                    using (StreamWriter w = File.AppendText("UserKNNonEachMovie.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region PrefKnn on EachMovie
        public static void PrefKNNonEachMovie()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesEachMovie)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        EachMovie,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteriaEachMovie,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    experiment.splitTrain = split;
                    experiment.someUsers = userCountEachMovie;

                    string log = experiment.RunPrefKNN(10);
                    using (StreamWriter w = File.AppendText("PrefKNNonEachMovie.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion






        #region PrefNMF on MovieLens20M
        public static void PrefNMFonMovieLens20M()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (var biase in MovieLens20MBiases)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens20M,
                        minTestSize,
                        maxCountOfRatings,
                        50,
                        shuffle,
                        seed,
                        relevantCriteriaMovieLens20M,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    //experiment.user_bias = biase;
                    experiment.item_bias = biase;
                    //experiment.global_bias = biase;
                    experiment.min_rating = int.MinValue;
                    experiment.max_rating = int.MaxValue;

                    string log = experiment.RunPrefNMF(50, 0.01, 0.04, 0.02, factorCount, topN);
                    using (StreamWriter w = File.AppendText("PrefNMFonMovieLens20M.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + biase.ToString("0.00"));
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region PrefOMF on MovieLens20M
        public static void PrefOMFonMovieLens20M()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (var biase in MovieLens20MBiases)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens20M,
                        minTestSize,
                        maxCountOfRatings,
                        50,
                        shuffle,
                        seed,
                        relevantCriteriaMovieLens20M,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    //experiment.user_bias = biase;
                    experiment.item_bias = biase;
                    //experiment.global_bias = biase;
                    experiment.min_rating = int.MinValue;
                    experiment.max_rating = int.MaxValue;

                    string log = experiment.RunPrefNMFbasedOMF(50, 0.01, 0.04, 0.02, factorCount, new List<double> { 1, 2, 3 }, topN);
                    using (StreamWriter w = File.AppendText("PrefOMFonMovieLens20M.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + biase.ToString("0.00"));
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region PrefMRF on MovieLens20M
        public static void PrefMRFonMovieLens20M()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (var biase in MovieLens20MBiases)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens20M,
                        minTestSize,
                        maxCountOfRatings,
                        50,
                        shuffle,
                        seed,
                        relevantCriteriaMovieLens20M,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    //experiment.user_bias = biase;
                    experiment.item_bias = biase;
                    //experiment.global_bias = biase;
                    experiment.min_rating = int.MinValue;
                    experiment.max_rating = int.MaxValue;

                    string log = experiment.RunPrefMRF(0.01, 0.005, 100, new List<double> { 1, 2, 3 }, 10);
                    if (log == "abort")
                    {
                        PrefOMFonMovieLens20M();
                        log = experiment.RunPrefMRF(0.01, 0.005, 100, new List<double> { 1, 2, 3 }, 10);
                    }

                    using (StreamWriter w = File.AppendText("PrefCRFonMovieLens20M.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + biase.ToString("0.00"));
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region NMF on MovieLens20M
        public static void NMFonMovieLens20M()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (var biase in MovieLens20MBiases)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens20M,
                        minTestSize,
                        maxCountOfRatings,
                        50,
                        shuffle,
                        seed,
                        relevantCriteriaMovieLens20M,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    //experiment.user_bias = biase;
                    experiment.item_bias = biase;
                    //experiment.global_bias = biase;
                    string log = experiment.RunNMF(100, 0.01, 0.05, 50, topN, 6.0);
                    using (StreamWriter w = File.AppendText("NMFonMovieLens20M.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + biase.ToString("0.00"));
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region UserKNN on MovieLens20M
        public static void UserKNNonMovieLens20M()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (var biase in MovieLens20MBiases)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens20M,
                        minTestSize,
                        maxCountOfRatings,
                        50,
                        shuffle,
                        seed,
                        relevantCriteriaMovieLens20M,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    //experiment.user_bias = biase;
                    experiment.item_bias = biase;
                    //experiment.global_bias = biase;
                    string log = experiment.RunUserKNN(10);
                    using (StreamWriter w = File.AppendText("UserKNNonMovieLens20M.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + biase.ToString("0.00"));
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region PrefKnn on MovieLens20M
        public static void PrefKNNonMovieLens20M()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (var biase in MovieLens20MBiases)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens20M,
                        minTestSize,
                        maxCountOfRatings,
                        50,
                        shuffle,
                        seed,
                        relevantCriteriaMovieLens20M,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    //experiment.user_bias = biase;
                    experiment.item_bias = biase;
                    //experiment.global_bias = biase;
                    experiment.min_rating = int.MinValue;
                    experiment.max_rating = int.MaxValue;

                    string log = experiment.RunPrefKNN(10);
                    using (StreamWriter w = File.AppendText("PrefKNNonMovieLens20M.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + biase);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion











        #region PrefNMF on Amazon
        public static void PrefNMFonAmazon()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesAmazon)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        Amazon,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteriaAmazon,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    experiment.splitTrain = split;
                    experiment.amazon = true;
                    //experiment.someUsers = userCountEachMovie;

                    string log = experiment.RunPrefNMF(50, 0.01, 0.04, 0.02, factorCount, topN);
                    using (StreamWriter w = File.AppendText("PrefNMFonAmazon.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region PrefOMF on Amazon
        public static void PrefOMFonAmazon()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesAmazon)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        Amazon,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteriaAmazon,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    experiment.splitTrain = split;
                    experiment.amazon = true;
                    //experiment.someUsers = userCountEachMovie;

                    string log = experiment.RunPrefNMFbasedOMF(50, 0.01, 0.04, 0.02, factorCount, new List<double> { 1, 2, 3 }, topN);
                    using (StreamWriter w = File.AppendText("PrefOMFonAmazon.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region PrefMRF on Amazon
        public static void PrefMRFonAmazon()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesAmazon)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        Amazon,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteriaAmazon,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    experiment.splitTrain = split;
                    experiment.amazon = true;
                    //experiment.someUsers = userCountAmazon;

                    string log = experiment.RunPrefMRF(0.01, 0.005, 100, new List<double> { 1, 2, 3 }, 10);
                    if (log == "abort")
                    {
                        PrefOMFonAmazon();
                        log = experiment.RunPrefMRF(0.01, 0.005, 100, new List<double> { 1, 2, 3 }, 10);
                    }

                    using (StreamWriter w = File.AppendText("PrefCRFonAmazon.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region NMF on Amazon
        public static void NMFonAmazon()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesAmazon)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        Amazon,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteriaAmazon,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    experiment.splitTrain = split;
                    experiment.amazon = true;
                    //experiment.someUsers = userCountAmazon;

                    string log = experiment.RunNMF(100, 0.01, 0.05, 50, topN, 6.0);
                    using (StreamWriter w = File.AppendText("NMFonAmazon.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region UserKNN on Amazon
        public static void UserKNNonAmazon()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesAmazon)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        Amazon,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteriaAmazon,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    experiment.splitTrain = split;
                    experiment.amazon = true;
                    //experiment.someUsers = userCountAmazon;

                    string log = experiment.RunUserKNN(10);
                    using (StreamWriter w = File.AppendText("UserKNNonAmazon.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        #region PrefKnn on Amazon
        public static void PrefKNNonAmazon()
        {
            for (int seed = initialSeed; seed <= finalSeed; seed++)
            {
                foreach (int givenSize in givenSizesAmazon)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        Amazon,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteriaAmazon,
                        maxSimilarityCount,
                        KnnNeighborCount,
                        minStrongCorrelationThreshold);

                    experiment.splitTrain = split;
                    experiment.amazon = true;
                    //experiment.someUsers = userCountAmazon;

                    string log = experiment.RunPrefKNN(10);
                    using (StreamWriter w = File.AppendText("PrefKNNonAmazon.log"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine(experiment.FormattedConfigurations());
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion
    }
}
