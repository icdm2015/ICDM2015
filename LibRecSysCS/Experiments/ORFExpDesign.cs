using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRecSysCS.Experiments
{
    public class ORFExpDesign
    {
        // Common configurations
        static int KNNNeighborCount = 50;
        static int maxSimilarityCount = 200;    // We only store the similarity values for the top 200 neighbors
        static int factorCount = 50;
        static int defaultMaxEpoch = 200;
        static string MovieLens1MFile = "MovieLens1M.data";
        static string EachMovieFile = "EachMovieCappedAt5.data";
        static List<int> givenSizesMovieLens = new List<int>() {30, 40, 50, 60 };
        static List<int> givenSizesEachMovie = new List<int>() {70, 80, 90, 100 };
        static int minTestSize = 10;
        static bool shuffle = true;
        static double relevantCriteria = 5;
        static double defaultLearnRate = 0.1;
        static double defaultRegularization = 0.15;
        static int topN = 10;
        static int maxCountOfRatings = 500;

        public static void PrefMRFonMovieLens1MRegularization(int fixed_seed, double regularization)
        {
            for (int seed = fixed_seed; seed <= fixed_seed; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens1MFile,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KNNNeighborCount,
                        0.05);

                    string log = experiment.RunPrefMRF(regularization, 0.005, 100, Constants.Preferences.quantizerThree, 10);
                    using (StreamWriter w = File.AppendText("PrefMRFonMovieLens1M" + regularization + ".txt"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }

        public static void PrefMRFonEachMovie(int fixed_seed)
        {
            for (int seed = fixed_seed; seed <= fixed_seed; seed++)
            {
                foreach (int givenSize in givenSizesEachMovie)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        EachMovieFile,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KNNNeighborCount,
                        0.05);

                    string log = experiment.RunPrefMRF(0.01, 0.005, 100, Constants.Preferences.quantizerThree, 10);
                    using (StreamWriter w = File.AppendText("PrefMRFonEachMovie" + fixed_seed + ".txt"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }

        public static void PrefNMFbasedOMFonEachMovie(int fixed_seed)
        {
            for (int seed = fixed_seed; seed <= fixed_seed; seed++)
            {
                foreach (int givenSize in givenSizesEachMovie)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        EachMovieFile,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KNNNeighborCount,
                        0.05);

                    string log = experiment.RunPrefNMFbasedOMF(50, 0.01, 0.04, 0.02, 50, Constants.Preferences.quantizerThree, 10);
                    using (StreamWriter w = File.AppendText("PrefNMFbasedOMFonEachMovie_Log" + fixed_seed + ".txt"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }

        public static void PrefNMFonEachMovie(int fixed_seed)
        {
            for (int seed = fixed_seed; seed <= fixed_seed; seed++)
            {
                foreach (int givenSize in givenSizesEachMovie)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        EachMovieFile,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KNNNeighborCount,
                        0.1);
                    //string log = experiment.RunPrefNMF(50, 0.001, 0.001, 0.0005, 50, 10);
                    string log = experiment.RunPrefNMF(50, 0.01, 0.04, 0.02, 50, 10);
                    using (StreamWriter w = File.AppendText("PrefNMFonEachMovie_Log_seed"+fixed_seed+".txt"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }

        public static void PrefKNNonEachMovie()
        {
            for (int seed = 1; seed <= 10; seed++)
            {
                foreach (int givenSize in givenSizesEachMovie)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        EachMovieFile,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KNNNeighborCount,
                        0.05);

                    string log = experiment.RunPrefKNN(10);
                    using (StreamWriter w = File.AppendText("PrefKNNonEachMovie_Log.txt"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }

        public static void UserKNNonEachMovie()
        {
            for (int seed = 1; seed <= 10; seed++)
            {
                foreach (int givenSize in givenSizesEachMovie)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        EachMovieFile,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KNNNeighborCount,
                        seed * 0.1);

                    string log = experiment.RunUserKNN(10);// experiment.RunNMF(defaultMaxEpoch, defaultLearnRate, defaultRegularization, factorCount, topN);
                    using (StreamWriter w = File.AppendText("UserKNNonEachMovie_Log.txt"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }

        public static void NMFonEachMovie()
        {
            for (int seed = 1; seed <= 10; seed++)
            {
                foreach (int givenSize in givenSizesEachMovie)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        EachMovieFile,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KNNNeighborCount,
                        seed * 0.1);

                    string log = experiment.RunNMF(100, 0.01, 0.05, 50, topN);
                    using (StreamWriter w = File.AppendText("NMFonEachMovie_Log.txt"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }

        #region Evaluation of NMF on Movielens1M
        /**********************************************************
         * Experiment 1
         * 
         * This experiment is to evaluate the top-N recommendation 
         * performance of NMF on MovieLens 1M data set with default 
         * factor count = 50
         * However, the Given data size is varied from 30, 40, 50, 60
         * Repeat ten times
         * ********************************************************/
        public static void NMFonMovieLens1M()
        {
            for(int seed = 1; seed <= 10;  seed++)
            {
                foreach(int givenSize in givenSizesMovieLens)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens1MFile,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KNNNeighborCount,
                        seed * 0.1);

                    string log = experiment.RunNMF(100, 0.01, 0.05, 50, topN);
                    using (StreamWriter w = File.AppendText("NMFonMovieLens1M_Log.txt"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
        #endregion

        public static void UserKNNonMovieLens1M()
        {
            for (int seed = 1; seed <= 10; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens1MFile,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KNNNeighborCount,
                        seed * 0.1);

                    string log = experiment.RunUserKNN(10);// experiment.RunNMF(defaultMaxEpoch, defaultLearnRate, defaultRegularization, factorCount, topN);
                    using (StreamWriter w = File.AppendText("UserKNNonMovieLens1M_Log.txt"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }

        public static void PrefKNNonMovieLens1M()
        {
            for (int seed = 1; seed <= 10; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens1MFile,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KNNNeighborCount,
                        seed * 0.1);

                    string log = experiment.RunPrefKNN(10);
                    using (StreamWriter w = File.AppendText("PrefKNNonMovieLens1M_Log.txt"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }

        public static void PrefNMFonMovieLens1M()
        {
            for (int seed = 1; seed <= 10; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens1MFile,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KNNNeighborCount,
                        seed * 0.1);
                    //string log = experiment.RunPrefNMF(50, 0.001, 0.001, 0.0005, 50, 10);
                    string log = experiment.RunPrefNMF(50, 0.01, 0.04, 0.02, 50, 10);
                    using (StreamWriter w = File.AppendText("PrefNMFonMovieLens1M_Log.txt"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }

        public static void PrefMRFonMovieLens1M(int fixed_seed)
        {
            for (int seed = fixed_seed; seed <= fixed_seed; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens1MFile,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        seed,
                        relevantCriteria,
                        maxSimilarityCount,
                        KNNNeighborCount,
                        0.05);

                    string log = experiment.RunPrefMRF(0.01, 0.005, 100, Constants.Preferences.quantizerThree, 10);
                    using (StreamWriter w = File.AppendText("PrefMRFonMovieLens1M.txt"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }

        public static void PrefNMFbasedOMFonMovieLens1M()
        {
            for (int seed = 1; seed <= 10; seed++)
            {
                foreach (int givenSize in givenSizesMovieLens)
                {
                    ExperimentEngine experiment = new ExperimentEngine(
                        MovieLens1MFile,
                        minTestSize,
                        maxCountOfRatings,
                        givenSize,
                        shuffle,
                        1,
                        relevantCriteria,
                        maxSimilarityCount,
                        KNNNeighborCount,
                        seed * 0.01);

                    string log = experiment.RunPrefNMFbasedOMF(50, 0.01, 0.04, 0.02, 50, Constants.Preferences.quantizerThree, 10);
                    using (StreamWriter w = File.AppendText("PrefNMFbasedOMFonMovieLens1M_Log.txt"))
                    {
                        w.WriteLine(experiment.GetDataFileName(""));
                        w.WriteLine("=========================================" + seed + "/" + givenSize);
                        w.WriteLine(log.ToString());
                    }
                }
            }
        }
    }
}
