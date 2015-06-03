using System.Collections.Generic;

namespace LibRecSysCS
{
    public class Constants
    {
        //========Environment settings=======
        public const int Seed = 2;
        public static readonly int LeftPad = 13;
        public static readonly int RightPad = 35;
        public static readonly string Rule = "\n───────────────────────\n"; // \u2500
        public static readonly string LongRule = "\n──────────────────────────────────\n";
        public static readonly bool RunNMF = true;
        public static readonly bool RunPrefNMF = true;
        public static readonly bool RunRatingUserKNN = true;
        public static readonly bool RunPreferenceUserKNN = true;
        public static readonly bool RunGlobalMean = true;
        public static readonly bool LoadSavedData = false;
        public static readonly double ZeroInSparseMatrix = -99;//1e-14;
        public const int MinCountOfRatings = 60;
        public const int CountOfRatingsForTrain = 50;
        public static readonly string[] SplitSeperators = { "\t", "::", "," };

        public class OMF
        {
            public static readonly int MaxEpoch = 1000;
            public static readonly double LearnRate = 0.001;
            public static readonly double Regularization = 0.015;
            public static readonly int LevelCount = 5;
        }


 

        public class Ratings
        {
            public const double MaxRating = 5.0;
            public const double MinRating = 1.0;
        }

        public class Preferences
        {
            // The position should be in [-1,1] but due to the difficult of storing 0 in sparse matrix
            // we shift all position values by 2 so the range becomes [1-3]
            public static readonly double PositionShift = 2;
            public static readonly double Preferred = 3;
            public static readonly double EquallyPreferred = 2;
            public static readonly double LessPreferred = 1;
            //public static readonly List<double> quantizerFive = new List<double> { 1, 2, 3, 4, 5,6 };
            public static readonly List<double> quantizerThree = new List<double> { 1, 2, 3};
        }
    }
}
