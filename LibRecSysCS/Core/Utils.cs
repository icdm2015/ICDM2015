using LibRecSysCS.Absolute;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LibRecSysCS
{
    /// <summary>
    /// This class implements core functions shared by differnet algorithms.
    /// Including read/write files, printing messages, timer, etc.
    /// </summary>
    public class Utils
    {
        #region Data IO

        #region Load and Save objects
        public class IO<T>
        {
            public static T LoadObject(string fileName)
            {
                Stream inStream = new FileStream(
                                        fileName,
                                        FileMode.Open,
                                        FileAccess.Read,
                                        FileShare.Read);
                BinaryFormatter bFormatter = new BinaryFormatter();
                T myObject = (T)bFormatter.Deserialize(inStream);
                inStream.Close();
                return myObject;
            }

            public static void SaveObject(T objectToSave, string fileName)
            {
                Stream outStream = new FileStream(
                                        fileName,
                                        FileMode.Create,
                                        FileAccess.Write,
                                        FileShare.None);
                BinaryFormatter bFormatter = new BinaryFormatter();
                bFormatter.Serialize(outStream, objectToSave);
                outStream.Close();
            }
        }
        #endregion

        #region Amazon reviews to CSV
        public static void ReviewsToCsv(string filename)
        {
            StringBuilder outputBuffer = new StringBuilder();

            int bufferSize = 0;
            string itemId = "";
            string userId = "";
            double rating = 0;
            int time = 0;
            string review = "";
            foreach (string line in File.ReadLines(filename))
            {
                if (line.StartsWith("product/productId: "))
                {
                    itemId = line.Substring("product/productId: ".Length);
                }
                else if (line.StartsWith("review/userId: "))
                {
                    userId = line.Substring("review/userId: ".Length);
                }
                else if (line.StartsWith("review/score: "))
                {
                    rating = double.Parse(line.Substring("review/score: ".Length));
                }
                else if (line.StartsWith("review/time: "))
                {
                    time = int.Parse(line.Substring("review/time: ".Length));
                }
                else if (line.StartsWith("review/text: "))
                {
                    review = line.Substring("review/text: ".Length);
                    //outputBuffer.AppendFormat("{0}::{1}::{2}::{3}::{4}\n", userId, itemId, rating, time, review);
                    outputBuffer.AppendFormat("{0}::{1}::{2}::{3}\n", userId, itemId, rating, time);
                    bufferSize++;
                    if (bufferSize % 500000 == 0)
                    {
                        using (StreamWriter w = File.AppendText("reviews_csv.txt"))
                        {
                            w.WriteLine(outputBuffer);
                            outputBuffer.Clear();
                        }
                        Console.WriteLine(bufferSize);
                    }
                }
            }
            using (StreamWriter w = File.AppendText("reviews_csv.txt"))
            {
                w.WriteLine(outputBuffer);
                outputBuffer.Clear();
            }
        }
        #endregion

        #region Load user/item attributes
        public static Dictionary<int, List<double>> LoadML1MItemAttributes(string filename)
        {
            Dictionary<int, List<double>> attributesByItem = new Dictionary<int, List<double>>();

            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(new string[] { "::" }, StringSplitOptions.None);
                int itemId = int.Parse(tokens[0]);
                List<double> attributeVector = new List<double>();
                string[] genres = tokens[2].Split(new string[] { "|" }, StringSplitOptions.None);
                attributeVector.Add(genres.Contains("Action") ? 1 : 0);
                attributeVector.Add(genres.Contains("Adventure") ? 1 : 0);
                attributeVector.Add(genres.Contains("Animation") ? 1 : 0);
                attributeVector.Add(genres.Contains("Children's") ? 1 : 0);
                attributeVector.Add(genres.Contains("Comedy") ? 1 : 0);
                attributeVector.Add(genres.Contains("Crime") ? 1 : 0);
                attributeVector.Add(genres.Contains("Documentary") ? 1 : 0);
                attributeVector.Add(genres.Contains("Drama") ? 1 : 0);
                attributeVector.Add(genres.Contains("Fantasy") ? 1 : 0);
                attributeVector.Add(genres.Contains("Film-Noir") ? 1 : 0);
                attributeVector.Add(genres.Contains("Horror") ? 1 : 0);
                attributeVector.Add(genres.Contains("Musical") ? 1 : 0);
                attributeVector.Add(genres.Contains("Mystery") ? 1 : 0);
                attributeVector.Add(genres.Contains("Romance") ? 1 : 0);
                attributeVector.Add(genres.Contains("Sci-Fi") ? 1 : 0);
                attributeVector.Add(genres.Contains("Thriller") ? 1 : 0);
                attributeVector.Add(genres.Contains("War") ? 1 : 0);
                attributeVector.Add(genres.Contains("Western") ? 1 : 0);
                attributesByItem[itemId] = attributeVector;
            }
            return attributesByItem;
        }

        public static Dictionary<int, List<double>> LoadML1MUserAttributes(string filename)
        {
            Dictionary<int, List<double>> attributesByUser = new Dictionary<int, List<double>>();

            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(new string[] { "::" }, StringSplitOptions.None);
                int userId = int.Parse(tokens[0]);
                string gender = tokens[1];
                string age = tokens[2];
                string occupation = tokens[3];

                List<double> attributesVector = new List<double>();

                attributesVector.Add(age == "1" ? 1 : 0);
                attributesVector.Add(age == "18" ? 1 : 0);
                attributesVector.Add(age == "25" ? 1 : 0);
                attributesVector.Add(age == "35" ? 1 : 0);
                attributesVector.Add(age == "45" ? 1 : 0);
                attributesVector.Add(age == "50" ? 1 : 0);
                attributesVector.Add(age == "56" ? 1 : 0);

                attributesVector.Add(gender == "F" ? 1 : 0);
                attributesVector.Add(gender == "M" ? 1 : 0);

                attributesVector.Add(occupation == "0" ? 1 : 0);
                attributesVector.Add(occupation == "1" ? 1 : 0);
                attributesVector.Add(occupation == "2" ? 1 : 0);
                attributesVector.Add(occupation == "3" ? 1 : 0);
                attributesVector.Add(occupation == "4" ? 1 : 0);
                attributesVector.Add(occupation == "5" ? 1 : 0);
                attributesVector.Add(occupation == "6" ? 1 : 0);
                attributesVector.Add(occupation == "7" ? 1 : 0);
                attributesVector.Add(occupation == "8" ? 1 : 0);
                attributesVector.Add(occupation == "9" ? 1 : 0);
                attributesVector.Add(occupation == "10" ? 1 : 0);
                attributesVector.Add(occupation == "11" ? 1 : 0);
                attributesVector.Add(occupation == "12" ? 1 : 0);
                attributesVector.Add(occupation == "13" ? 1 : 0);
                attributesVector.Add(occupation == "14" ? 1 : 0);
                attributesVector.Add(occupation == "15" ? 1 : 0);
                attributesVector.Add(occupation == "16" ? 1 : 0);
                attributesVector.Add(occupation == "17" ? 1 : 0);
                attributesVector.Add(occupation == "18" ? 1 : 0);
                attributesVector.Add(occupation == "19" ? 1 : 0);
                attributesVector.Add(occupation == "20" ? 1 : 0);

                attributesByUser[userId] = attributesVector;
            }

            return attributesByUser;
        }


        public static Dictionary<int, List<double>> LoadML100KItemAttributes(string filename)
        {
            Dictionary<int, List<double>> attributesByItem = new Dictionary<int, List<double>>();

            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(new string[] { "|" }, StringSplitOptions.None);
                int itemId = int.Parse(tokens[0]);
                List<double> attributeVector = tokens.ToList().GetRange(5, 19).Select(x => double.Parse(x)).ToList();
                attributesByItem[itemId] = attributeVector;
            }
            return attributesByItem;
        }

        public static Dictionary<int, List<double>> LoadML100KUserAttributes(string filename)
        {
            Dictionary<int, List<double>> attributesByUser = new Dictionary<int, List<double>>();

            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(new string[] { "|" }, StringSplitOptions.None);
                int userId = int.Parse(tokens[0]);
                int age = int.Parse(tokens[1]);
                string gender = tokens[2];
                string occupation = tokens[3];

                List<double> attributesVector = new List<double>();

                attributesVector.Add(age < 18 ? 1 : 0);
                attributesVector.Add(age >= 18 && age <=24 ? 1 : 0);
                attributesVector.Add(age >= 25 && age <= 34 ? 1 : 0);
                attributesVector.Add(age >= 35 && age <= 44 ? 1 : 0);
                attributesVector.Add(age >= 45 && age <= 49 ? 1 : 0);
                attributesVector.Add(age >= 50 && age <= 55 ? 1 : 0);
                attributesVector.Add(age >= 56 ? 1 : 0);

                attributesVector.Add(gender=="F" ? 1 : 0);
                attributesVector.Add(gender == "M" ? 1 : 0);

                attributesVector.Add(occupation == "administrator" ? 1 : 0);
                attributesVector.Add(occupation == "artist" ? 1 : 0);
                attributesVector.Add(occupation == "doctor" ? 1 : 0);
                attributesVector.Add(occupation == "educator" ? 1 : 0);
                attributesVector.Add(occupation == "engineer" ? 1 : 0);
                attributesVector.Add(occupation == "entertainment" ? 1 : 0);
                attributesVector.Add(occupation == "executive" ? 1 : 0);
                attributesVector.Add(occupation == "healthcare" ? 1 : 0);
                attributesVector.Add(occupation == "homemaker" ? 1 : 0);
                attributesVector.Add(occupation == "lawyer" ? 1 : 0);
                attributesVector.Add(occupation == "librarian" ? 1 : 0);
                attributesVector.Add(occupation == "marketing" ? 1 : 0);
                attributesVector.Add(occupation == "none" ? 1 : 0);
                attributesVector.Add(occupation == "other" ? 1 : 0);
                attributesVector.Add(occupation == "programmer" ? 1 : 0);
                attributesVector.Add(occupation == "retired" ? 1 : 0);
                attributesVector.Add(occupation == "salesman" ? 1 : 0);
                attributesVector.Add(occupation == "scientist" ? 1 : 0);
                attributesVector.Add(occupation == "student" ? 1 : 0);
                attributesVector.Add(occupation == "technician" ? 1 : 0);
                attributesVector.Add(occupation == "writer" ? 1 : 0);

                attributesByUser[userId] = attributesVector;
            }

            return attributesByUser;
        }
        #endregion

        #region Load CSV with Reviews
        public static void LoadReviews(string filename, int given_size, int test_size, 
            int item_size, int time_afterwards)
        {
            Dictionary<string, int> userById = new Dictionary<string, int>();   // Mapping from user id to index in matrix
            Dictionary<string, int> itemById = new Dictionary<string, int>();   // Mapping from item id to index in matrix
            Dictionary<string, int> numOfRatingsByUserId = new Dictionary<string, int>(); // Num of ratings by user id
            Dictionary<string, int> numOfRatingsByItemId = new Dictionary<string, int>(); // Num of ratings by item id
            Dictionary<string, int> randomSelectedItems = new Dictionary<string, int>();
            int positiveCount = 0;
            int negativeCount = 0;
            int neutralCount = 0;

            // Scan data file to randomly select some items
            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(new string[] { "::" }, StringSplitOptions.None);

                //if (int.Parse(tokens[3]) < time_afterwards) { continue; }

                string itemId = tokens[1];

                if (randomSelectedItems.Count < 10000)
                {
                    randomSelectedItems[itemId] = 1;
                }
            }

            // Scan data file to identify cold items
            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(new string[] { "::" }, StringSplitOptions.None);
                string itemId = tokens[1];

                //if (int.Parse(tokens[3]) < time_afterwards) { continue; }

                if (!randomSelectedItems.ContainsKey(itemId)) { continue; }

                if (!numOfRatingsByItemId.ContainsKey(itemId)) { numOfRatingsByItemId[itemId] = 0; }
                numOfRatingsByItemId[itemId]++;
            }

            // Scan data file to identify cold users
            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(new string[] { "::" }, StringSplitOptions.None);
                string userId = tokens[0];
                string itemId = tokens[1];

                //if (int.Parse(tokens[3]) < time_afterwards) { continue; }

                if (!randomSelectedItems.ContainsKey(itemId)) { continue; }

                if (numOfRatingsByItemId[itemId] < item_size) { continue; } // Skip cold items

                if (!numOfRatingsByUserId.ContainsKey(userId)) { numOfRatingsByUserId[userId] = 0; }
                numOfRatingsByUserId[userId]++;
            }

            // Scan data file again to fetch data entries of non-cold users
            int bufferSize = 0;
            StringBuilder posOutputBuffer = new StringBuilder();
            StringBuilder negOutputBuffer = new StringBuilder();
            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(new string[] { "::" }, StringSplitOptions.None);
                string userId = tokens[0];
                string itemId = tokens[1];
                double rating = double.Parse(tokens[2]);

                //if (int.Parse(tokens[3]) < time_afterwards) { continue; }

                if (!randomSelectedItems.ContainsKey(itemId)) { continue; }
                // Skip cold items
                if (numOfRatingsByItemId[itemId] < item_size) { continue; }
                // Skip cold user
                if (numOfRatingsByUserId[userId] < given_size + test_size
                    || numOfRatingsByUserId[userId] > 500)
                { continue; }

                // Assign a matrix index for this user/item (if new)
                if (!userById.ContainsKey(userId))
                {
                    userById[userId] = userById.Count;
                }
                if (!itemById.ContainsKey(itemId))
                {
                    itemById[itemId] = itemById.Count;
                }

                if (rating > 3) positiveCount++;
                if (rating < 3) negativeCount++;
                if (rating == 3) neutralCount++;

                bufferSize++;
                if(rating > 3)
                {
                    posOutputBuffer.AppendLine(tokens[4]);
                    using (StreamWriter w = File.AppendText("pos/" + bufferSize + ".txt"))
                    {
                        w.WriteLine(posOutputBuffer);
                        posOutputBuffer.Clear();
                    }
                }
                else
                {
                    negOutputBuffer.AppendLine(tokens[4]);
                    using (StreamWriter w = File.AppendText("neg/" + bufferSize + ".txt"))
                    {
                        w.WriteLine(negOutputBuffer);
                        negOutputBuffer.Clear();
                    }
                }
                //if (bufferSize % 100000 == 0)
                //{


                 //   Console.WriteLine(bufferSize);
                //}
            }
            //using (StreamWriter w = File.AppendText("noncold-pos-reviews.txt"))
            //{
            //    w.WriteLine(posOutputBuffer);
            //    posOutputBuffer.Clear();
            //}
           // using (StreamWriter w = File.AppendText("noncold-neg-reviews.txt"))
           // {
            //    w.WriteLine(negOutputBuffer);
            //    negOutputBuffer.Clear();
           // }

            Console.WriteLine("# of warm users: " + userById.Count);
            Console.WriteLine("# of warm items: " + itemById.Count);
            Console.WriteLine("# positiveCount: " + positiveCount);
            Console.WriteLine("# negativeCount: " + negativeCount);
            Console.WriteLine("# neutralCount: " + neutralCount);
        }
        #endregion

        #region Load CSV data with attributes
        /// <summary>
        /// Load movielens data set, the data set will be split into train and test sets.
        /// Pre-shuffle the file and swith off shuffle option is recommended for large data set.
        /// </summary>
        /// <param name="fileOfDataSet">Path to the movielens data set.</param>
        /// <param name="trainset">The training set will be sent out from this parameter.</param>
        /// <param name="testset">The testing set will be sent out from this parameter.</param>
        /// <param name="numOfRatingsForTest">Specifies the min number of ratings for each user to 
        /// keep in the testing set.</param>
        /// <param name="numOfRatingsForTrain">Specifies how many ratings for each user to 
        /// keep in the training set, and the reset in the testing set.</param>
        /// <param name="shuffle">Specifies whether the lines in the file should be read 
        /// in random order or not.</param>
        /// <param name="seed">The random seed for shuffle.</param>
        public static void LoadCsvWithAttributes(string ratingFilename, string userAttributesFilename,
            string itemAttributesFilename, out DataMatrix trainset,
            out DataMatrix testset, out Dictionary<int, List<double>> attributesByUser,
            out Dictionary<int, List<double>> attributesByItem,
            int numOfRatingsForTrain, int numOfRatingsForTest,
            int maxNumOfRatings, bool shuffle, int seed)
        {
            Dictionary<int, int> userById = new Dictionary<int, int>();   // Mapping from user id to index in matrix
            Dictionary<int, int> itemById = new Dictionary<int, int>();   // Mapping from item id to index in matrix
            Dictionary<int, int> numOfRatingsByUserId = new Dictionary<int, int>(); // Num of ratings by user id
            Dictionary<int, int> numOfRatingsByItemId = new Dictionary<int, int>(); // Num of ratings by item id
            Dictionary<int, List<Tuple<int, double>>> ratingsByNoncoldUser = new Dictionary<int, List<Tuple<int, double>>>();
            attributesByUser = new Dictionary<int, List<double>>();
            attributesByItem = new Dictionary<int, List<double>>();
            Dictionary<int, List<double>> attributesByUserId = Utils.LoadML100KUserAttributes(userAttributesFilename);
            Dictionary<int, List<double>> attributesByItemId = Utils.LoadML100KItemAttributes(itemAttributesFilename);

            // Scan data file to identify cold items
            foreach (string line in File.ReadLines(ratingFilename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int itemId = int.Parse(tokens[1]);
                if (!numOfRatingsByItemId.ContainsKey(itemId)) { numOfRatingsByItemId[itemId] = 0; }
                numOfRatingsByItemId[itemId]++;
            }

            // Scan data file to identify cold users
            foreach (string line in File.ReadLines(ratingFilename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int userId = int.Parse(tokens[0]);
                int itemId = int.Parse(tokens[1]);

                //if (numOfRatingsByItemId[itemId] < 10) { continue; } // Skip cold items

                if (!numOfRatingsByUserId.ContainsKey(userId)) { numOfRatingsByUserId[userId] = 0; }
                numOfRatingsByUserId[userId]++;
            }

            // Scan data file again to fetch data entries of non-cold users
            foreach (string line in File.ReadLines(ratingFilename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int userId = int.Parse(tokens[0]);
                int itemId = int.Parse(tokens[1]);
                double rating = double.Parse(tokens[2]);


                // Skip cold items
                //if (numOfRatingsByItemId[itemId] < 10) { continue; }
                // Skip cold user
                if (numOfRatingsByUserId[userId] < (numOfRatingsForTrain + numOfRatingsForTest)
                    || numOfRatingsByUserId[userId] > maxNumOfRatings)
                { continue; }

                // Assign a matrix index for this user/item (if new)
                if (!userById.ContainsKey(userId))
                {
                    userById[userId] = userById.Count;
                }
                if (!itemById.ContainsKey(itemId))
                {
                    itemById[itemId] = itemById.Count;
                }

                int indexOfUser = userById[userId];
                int indexOfItem = itemById[itemId];
                // Store the rating for this non-cold user
                if (!ratingsByNoncoldUser.ContainsKey(indexOfUser)) { ratingsByNoncoldUser[indexOfUser] = new List<Tuple<int, double>>(); }
                ratingsByNoncoldUser[indexOfUser].Add(new Tuple<int, double>(indexOfItem, rating));
            }

            // Distribute all stored ratings into train/test sets
            int numOfUsers = ratingsByNoncoldUser.Count;
            int numOfItems = itemById.Count;
            int trainsetSize = numOfUsers * numOfRatingsForTrain;

            // Process each line and put ratings into training/testing sets
            List<Tuple<int, int, double>> trainsetCache = new List<Tuple<int, int, double>>();
            List<Tuple<int, int, double>> testsetCache = new List<Tuple<int, int, double>>();

            Random random = new Random(seed);
            foreach (var ratings in ratingsByNoncoldUser)
            {
                int indexOfUser = ratings.Key;
                var ratingByItem = ratings.Value;

                if (shuffle)
                {
                    ratingByItem.Shuffle(random);
                }

                // Add the first part of the rating list to trainset
                foreach (var rating in ratingByItem.GetRange(0, numOfRatingsForTrain))
                {
                    trainsetCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, rating.Item2));
                }

                // Add the rest part of the rating list to testset
                foreach (var rating in ratingByItem.GetRange(numOfRatingsForTrain - 1, ratingByItem.Count - numOfRatingsForTrain))
                {
                    testsetCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, rating.Item2));
                }
            }

            Console.WriteLine(numOfRatingsByUserId.Count - userById.Count +
                " users have less than " + (numOfRatingsForTrain + numOfRatingsForTest)
                + " and were removed.");

            trainset = new DataMatrix(SparseMatrix.OfIndexed(userById.Count, itemById.Count, trainsetCache));
            testset = new DataMatrix(SparseMatrix.OfIndexed(userById.Count, itemById.Count, testsetCache));

            foreach(var user in userById)
            {
                int indexOfUser = user.Value;
                int idOfUser = user.Key;
                attributesByUser[indexOfUser] = attributesByUserId[idOfUser];
            }

            foreach (var item in itemById)
            {
                int indexOfItem = item.Value;
                int idOfItem = item.Key;
                attributesByItem[indexOfItem] = attributesByItemId[idOfItem];
            }

            Console.WriteLine(trainset.Matrix.ColumnSums().Sum());
        }
        #endregion

        #region Load CSV data of some users
        public static void LoadCsvSplit(string filename, out DataMatrix trainsetRatings,
            out DataMatrix trainsetBinary,
            out DataMatrix testset, int numOfRatingsForTrain, int numOfRatingsForTest,
            int maxNumOfRatings, bool shuffle, int seed, int selectUserCount, bool split = false)
        {
            Dictionary<int, int> userById = new Dictionary<int, int>();   // Mapping from user id to index in matrix
            Dictionary<int, int> itemById = new Dictionary<int, int>();   // Mapping from item id to index in matrix
            Dictionary<int, int> numOfRatingsByUserId = new Dictionary<int, int>(); // Num of ratings by user id
            Dictionary<int, int> numOfRatingsByItemId = new Dictionary<int, int>(); // Num of ratings by item id
            Dictionary<int, List<Tuple<int, double>>> ratingsByNoncoldUser = new Dictionary<int, List<Tuple<int, double>>>();
            Dictionary<int, int> selectedUsers = new Dictionary<int, int>();

            int binary_given = numOfRatingsForTrain / 2;
            if (split)
            {
                numOfRatingsForTrain = numOfRatingsForTrain - binary_given;
            }

            int colduser_threshold = numOfRatingsForTrain + numOfRatingsForTest;

            // Scan data file to identify cold users
            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int userId = int.Parse(tokens[0]);
                int itemId = int.Parse(tokens[1]);
                
                if (!numOfRatingsByUserId.ContainsKey(userId)) { numOfRatingsByUserId[userId] = 0; }
                numOfRatingsByUserId[userId]++;
            }

            // Scan data file again to fetch data entries of non-cold users
            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int userId = int.Parse(tokens[0]);
                int itemId = int.Parse(tokens[1]);
                double rating = double.Parse(tokens[2]);


                // Skip cold items
                //if (numOfRatingsByItemId[itemId] < 10) { continue; }
                // Skip cold user
                if (numOfRatingsByUserId[userId] < colduser_threshold
                    || numOfRatingsByUserId[userId] > maxNumOfRatings)
                { continue; }

                if(selectedUsers.Count < selectUserCount || selectedUsers.ContainsKey(userId))
                {
                    selectedUsers[userId] = 1;
                }
                else
                {
                    continue;
                }

                // Assign a matrix index for this user/item (if new)
                if (!userById.ContainsKey(userId))
                {
                    userById[userId] = userById.Count;
                }
                if (!itemById.ContainsKey(itemId))
                {
                    itemById[itemId] = itemById.Count;
                }

                int indexOfUser = userById[userId];
                int indexOfItem = itemById[itemId];
                // Store the rating for this non-cold user
                if (!ratingsByNoncoldUser.ContainsKey(indexOfUser)) { ratingsByNoncoldUser[indexOfUser] = new List<Tuple<int, double>>(); }
                ratingsByNoncoldUser[indexOfUser].Add(new Tuple<int, double>(indexOfItem, rating));
            }

            // Distribute all stored ratings into train/test sets
            int numOfUsers = ratingsByNoncoldUser.Count;
            int numOfItems = itemById.Count;
            int trainsetSize = numOfUsers * numOfRatingsForTrain;

            // Process each line and put ratings into training/testing sets
            List<Tuple<int, int, double>> trainsetRatingsCache = new List<Tuple<int, int, double>>();
            List<Tuple<int, int, double>> trainsetBinaryCache = new List<Tuple<int, int, double>>();
            List<Tuple<int, int, double>> testsetCache = new List<Tuple<int, int, double>>();

            Random random = new Random(seed);
            foreach (var ratings in ratingsByNoncoldUser)
            {
                int indexOfUser = ratings.Key;
                var ratingByItem = ratings.Value;

                if (shuffle)
                {
                    ratingByItem.Shuffle(random);
                }

                // Add the first part of the rating list to trainset

                foreach (var rating in ratingByItem.GetRange(0, numOfRatingsForTrain))
                {
                    trainsetRatingsCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, rating.Item2));
                }

                // we try to get some ratings from large test set
                if (split && (ratingByItem.Count > (numOfRatingsForTrain + binary_given + numOfRatingsForTest)))
                {
                    foreach (var rating in ratingByItem.GetRange(numOfRatingsForTrain, binary_given))
                    {
                        trainsetBinaryCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, rating.Item2));
                        //trainsetBinaryCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, rating.Item2 < 3 ? 1 : 2));
                    }

                    // Add the rest part of the rating list to testset
                    foreach (var rating in ratingByItem.GetRange(numOfRatingsForTrain + binary_given,
                        ratingByItem.Count - numOfRatingsForTrain - binary_given))
                    {
                        testsetCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, rating.Item2));
                    }
                }
                else
                {
                    // Add the rest part of the rating list to testset
                    foreach (var rating in ratingByItem.GetRange(numOfRatingsForTrain, ratingByItem.Count - numOfRatingsForTrain))
                    {
                        testsetCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, rating.Item2));
                    }
                }
            }

            Console.WriteLine(numOfRatingsByUserId.Count - userById.Count +
                " users have less than " + (numOfRatingsForTrain + binary_given + numOfRatingsForTest)
                + " and were removed.");

            trainsetRatings = new DataMatrix(SparseMatrix.OfIndexed(userById.Count, itemById.Count, trainsetRatingsCache));
            trainsetBinary = new DataMatrix(SparseMatrix.OfIndexed(userById.Count, itemById.Count, trainsetBinaryCache));
            testset = new DataMatrix(SparseMatrix.OfIndexed(userById.Count, itemById.Count, testsetCache));

        }
        #endregion

        #region Load CSV data and split
        public static void LoadCsvSplitAmazon(string filename, out DataMatrix trainsetRatings,
            out DataMatrix trainsetBinary,
            out DataMatrix testset, int numOfRatingsForTrain, int numOfRatingsForTest,
            int maxNumOfRatings, bool shuffle, int seed, bool split)
        {
            Dictionary<int, int> userById = new Dictionary<int, int>();   // Mapping from user id to index in matrix
            Dictionary<int, int> itemById = new Dictionary<int, int>();   // Mapping from item id to index in matrix
            Dictionary<int, int> numOfRatingsByUserId = new Dictionary<int, int>(); // Num of ratings by user id
            Dictionary<int, int> numOfRatingsByItemId = new Dictionary<int, int>(); // Num of ratings by item id
            Dictionary<int, List<Tuple<int, double>>> ratingsByNoncoldUser = new Dictionary<int, List<Tuple<int, double>>>();
    

            int binary_given = numOfRatingsForTrain / 2;
            if (split)
            {
                numOfRatingsForTrain = numOfRatingsForTrain - binary_given;
            }

            int colduser_threshold = numOfRatingsForTrain + numOfRatingsForTest;

            // Scan data file to identify cold users
            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int userId = int.Parse(tokens[0]);
                int itemId = int.Parse(tokens[1]);

                if (!numOfRatingsByUserId.ContainsKey(userId)) { numOfRatingsByUserId[userId] = 0; }
                numOfRatingsByUserId[userId]++;
            }

            // Scan data file again to fetch data entries of non-cold users
            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int userId = int.Parse(tokens[0]);
                int itemId = int.Parse(tokens[1]);
                double rating = double.Parse(tokens[2]);


                // Skip cold items
                //if (numOfRatingsByItemId[itemId] < 10) { continue; }
                // Skip cold user
                if (numOfRatingsByUserId[userId] < colduser_threshold
                    || numOfRatingsByUserId[userId] > maxNumOfRatings)
                { continue; }


                // Assign a matrix index for this user/item (if new)
                if (!userById.ContainsKey(userId))
                {
                    userById[userId] = userById.Count;
                }
                if (!itemById.ContainsKey(itemId))
                {
                    itemById[itemId] = itemById.Count;
                }

                int indexOfUser = userById[userId];
                int indexOfItem = itemById[itemId];
                // Store the rating for this non-cold user
                if (!ratingsByNoncoldUser.ContainsKey(indexOfUser)) { ratingsByNoncoldUser[indexOfUser] = new List<Tuple<int, double>>(); }
                ratingsByNoncoldUser[indexOfUser].Add(new Tuple<int, double>(indexOfItem, rating));
            }

            // Distribute all stored ratings into train/test sets
            int numOfUsers = ratingsByNoncoldUser.Count;
            int numOfItems = itemById.Count;
            int trainsetSize = numOfUsers * numOfRatingsForTrain;

            // Process each line and put ratings into training/testing sets
            List<Tuple<int, int, double>> trainsetRatingsCache = new List<Tuple<int, int, double>>();
            List<Tuple<int, int, double>> trainsetBinaryCache = new List<Tuple<int, int, double>>();
            List<Tuple<int, int, double>> testsetCache = new List<Tuple<int, int, double>>();

            Random random = new Random(seed);
            foreach (var ratings in ratingsByNoncoldUser)
            {
                int indexOfUser = ratings.Key;
                var ratingByItem = ratings.Value;

                if (shuffle)
                {
                    ratingByItem.Shuffle(random);
                }

                // Add the first part of the rating list to trainset

                foreach (var rating in ratingByItem.GetRange(0, numOfRatingsForTrain))
                {
                    trainsetRatingsCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, rating.Item2));
                }

               
                // we try to get some ratings from large test set
                if (split && (ratingByItem.Count > (numOfRatingsForTrain + binary_given + numOfRatingsForTest)))
                {
                    foreach (var rating in ratingByItem.GetRange(numOfRatingsForTrain, binary_given))
                    {
                        //trainsetBinaryCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, rating.Item2));
                        //trainsetBinaryCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, rating.Item2 < 3 ? 1 : 2));
                        double binaryRating = rating.Item2 < 3 ? 1 : 2;
                        //if (random.Next(0, 10) < 0.1 * 10) { binaryRating = binaryRating == 1 ? 2 : 1; }
                        trainsetBinaryCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, binaryRating));
                    }

                    // Add the rest part of the rating list to testset
                    foreach (var rating in ratingByItem.GetRange(numOfRatingsForTrain + binary_given,
                        ratingByItem.Count - numOfRatingsForTrain - binary_given))
                    {
                        testsetCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, rating.Item2));
                    }
                }
                else
                {
                    // Add the rest part of the rating list to testset
                    foreach (var rating in ratingByItem.GetRange(numOfRatingsForTrain, ratingByItem.Count - numOfRatingsForTrain))
                    {
                        testsetCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, rating.Item2));
                    }
                }
            }

            Console.WriteLine(numOfRatingsByUserId.Count - userById.Count +
                " users have less than " + (numOfRatingsForTrain + binary_given + numOfRatingsForTest)
                + " and were removed.");

            trainsetRatings = new DataMatrix(SparseMatrix.OfIndexed(userById.Count, itemById.Count, trainsetRatingsCache));
            trainsetBinary = new DataMatrix(SparseMatrix.OfIndexed(userById.Count, itemById.Count, trainsetBinaryCache));
            testset = new DataMatrix(SparseMatrix.OfIndexed(userById.Count, itemById.Count, testsetCache));

        }
        #endregion

        #region Load CSV data
        /// <summary>
        /// Load movielens data set, the data set will be split into train and test sets.
        /// Pre-shuffle the file and swith off shuffle option is recommended for large data set.
        /// </summary>
        /// <param name="fileOfDataSet">Path to the movielens data set.</param>
        /// <param name="trainset">The training set will be sent out from this parameter.</param>
        /// <param name="testset">The testing set will be sent out from this parameter.</param>
        /// <param name="numOfRatingsForTest">Specifies the min number of ratings for each user to 
        /// keep in the testing set.</param>
        /// <param name="numOfRatingsForTrain">Specifies how many ratings for each user to 
        /// keep in the training set, and the reset in the testing set.</param>
        /// <param name="shuffle">Specifies whether the lines in the file should be read 
        /// in random order or not.</param>
        /// <param name="seed">The random seed for shuffle.</param>
        public static void LoadCsv(string filename, out DataMatrix trainset,
            out DataMatrix testset, int numOfRatingsForTrain, int numOfRatingsForTest,
            int maxNumOfRatings, bool shuffle, int seed)
        {
            Dictionary<int, int> userById = new Dictionary<int, int>();   // Mapping from user id to index in matrix
            Dictionary<int, int> itemById = new Dictionary<int, int>();   // Mapping from item id to index in matrix
            Dictionary<int, int> numOfRatingsByUserId = new Dictionary<int, int>(); // Num of ratings by user id
            Dictionary<int, int> numOfRatingsByItemId = new Dictionary<int, int>(); // Num of ratings by item id
            Dictionary<int, List<Tuple<int, double>>> ratingsByNoncoldUser = new Dictionary<int, List<Tuple<int, double>>>();

            // Scan data file to identify cold items
            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int itemId = int.Parse(tokens[1]);
                if (!numOfRatingsByItemId.ContainsKey(itemId)) { numOfRatingsByItemId[itemId] = 0; }
                numOfRatingsByItemId[itemId]++;
            }

            // Scan data file to identify cold users
            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int userId = int.Parse(tokens[0]);
                int itemId = int.Parse(tokens[1]);

                //if(numOfRatingsByItemId[itemId] < 10) { continue; } // Skip cold items

                if (!numOfRatingsByUserId.ContainsKey(userId)) { numOfRatingsByUserId[userId] = 0; }
                numOfRatingsByUserId[userId]++;
            }

            // Scan data file again to fetch data entries of non-cold users
            foreach (string line in File.ReadLines(filename))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int userId = int.Parse(tokens[0]);
                int itemId = int.Parse(tokens[1]);
                double rating = double.Parse(tokens[2]);


                // Skip cold items
                //if (numOfRatingsByItemId[itemId] < 10) { continue; }
                // Skip cold user
                if (numOfRatingsByUserId[userId] < (numOfRatingsForTrain + numOfRatingsForTest))
                { continue; }
                if (numOfRatingsByUserId[userId] > maxNumOfRatings)
                { continue; }

                // Assign a matrix index for this user/item (if new)
                if (!userById.ContainsKey(userId))
                {
                    userById[userId] = userById.Count;
                }
                if (!itemById.ContainsKey(itemId))
                {
                    itemById[itemId] = itemById.Count;
                }

                int indexOfUser = userById[userId];
                int indexOfItem = itemById[itemId];
                // Store the rating for this non-cold user
                if (!ratingsByNoncoldUser.ContainsKey(indexOfUser)) { ratingsByNoncoldUser[indexOfUser] = new List<Tuple<int, double>>(); }
                ratingsByNoncoldUser[indexOfUser].Add(new Tuple<int, double>(indexOfItem, rating));
            }

            // Distribute all stored ratings into train/test sets
            int numOfUsers = ratingsByNoncoldUser.Count;
            int numOfItems = itemById.Count;
            int trainsetSize = numOfUsers * numOfRatingsForTrain;

            // Process each line and put ratings into training/testing sets
            List<Tuple<int, int, double>> trainsetCache = new List<Tuple<int, int, double>>();
            List<Tuple<int, int, double>> testsetCache = new List<Tuple<int, int, double>>();

            Random random = new Random(seed);
            foreach (var ratings in ratingsByNoncoldUser)
            {
                int indexOfUser = ratings.Key;
                var ratingByItem = ratings.Value;

                if (shuffle)
                {
                    ratingByItem.Shuffle(random);
                }

                // Add the first part of the rating list to trainset
                foreach (var rating in ratingByItem.GetRange(0, numOfRatingsForTrain))
                {
                    trainsetCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, rating.Item2));
                }

                // Add the rest part of the rating list to testset
                foreach (var rating in ratingByItem.GetRange(numOfRatingsForTrain, ratingByItem.Count - numOfRatingsForTrain))
                {
                    testsetCache.Add(new Tuple<int, int, double>(indexOfUser, rating.Item1, rating.Item2));
                }
            }

            Console.WriteLine(numOfRatingsByUserId.Count - userById.Count +
                " users have less than " + (numOfRatingsForTrain + numOfRatingsForTest)
                + " and were removed.");

            trainset = new DataMatrix(SparseMatrix.OfIndexed(userById.Count, itemById.Count, trainsetCache));
            testset = new DataMatrix(SparseMatrix.OfIndexed(userById.Count, itemById.Count, testsetCache));

            Console.WriteLine(trainset.Matrix.ColumnSums().Sum());
        }
        #endregion
        
        [Obsolete]
        #region Load movielens dataset into SparseMatrix
            /// <summary>
            /// Load movielens data set, the data set will be split into train and test sets.
            /// Pre-shuffle the file and swith off shuffle option is recommended for large data set.
            /// </summary>
            /// <param name="fileOfDataSet">Path to the movielens data set.</param>
            /// <param name="R_train">The training set will be sent out from this parameter.</param>
            /// <param name="R_test">The testing set will be sent out from this parameter.</param>
            /// <param name="minCountOfRatings">Users with ratings less than the specified count 
            /// will be excluded from the data set.</param>
            /// <param name="countOfRatingsForTrain">Specifies how many ratings for each user to 
            /// keep in the training set, and the reset in the testing set.</param>
            /// <param name="shuffle">Specifies whether the lines in the file should be read 
            /// in random order or not.</param>
            /// <param name="seed">The random seed for shuffle.</param>
        public static void LoadMovieLensSplitByCount(string fileOfDataSet, out DataMatrix R_train,
            out DataMatrix R_test, int minCountOfRatings = Constants.MinCountOfRatings, int maxCountOfRatings = 500,
            int countOfRatingsForTrain = Constants.CountOfRatingsForTrain, bool shuffle = false, int seed = 1)
        {
            Dictionary<int, int> userByIndex = new Dictionary<int, int>();   // Mapping from index in movielens file to user index in matrix
            Dictionary<int, int> ratingCountByUser = new Dictionary<int, int>(); // count how many ratings of each user
            Dictionary<int, int> itemByIndex = new Dictionary<int, int>();   // Mapping from index in movielens file to item index in matrix

            // Read the file to discover the whole matrix structure and mapping
            foreach (string line in File.ReadLines(fileOfDataSet))
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int indexOfUser = int.Parse(tokens[0]);
                int indexOfItem = int.Parse(tokens[1]);
                if (!userByIndex.ContainsKey(indexOfUser))          // We update index only for new user
                {
                    userByIndex[indexOfUser] = userByIndex.Count;   // The current size is just the current matrix index
                    ratingCountByUser[indexOfUser] = 1;             // Initialize the rating count for this new user
                }
                else { ratingCountByUser[indexOfUser]++; }

                if (!itemByIndex.ContainsKey(indexOfItem))          // We update index only for new item
                {
                    itemByIndex[indexOfItem] = itemByIndex.Count;   // The current size is just the current matrix index
                }
            }

            // Remove users with too few or more many ratings
            int countOfRemovedUsers = 0;
            List<int> indexes = userByIndex.Keys.ToList();
            foreach (int fileIndexOfUser in indexes)
            {
                if (ratingCountByUser[fileIndexOfUser] < minCountOfRatings || ratingCountByUser[fileIndexOfUser] > maxCountOfRatings)
                {
                    int indexOfRemovedUser = userByIndex[fileIndexOfUser];
                    userByIndex.Remove(fileIndexOfUser);
                    List<int> keys = userByIndex.Keys.ToList();
                    // We need to shift the matrix index by 1 after removed one user
                    foreach (int key in keys)
                    {
                        if (userByIndex[key] > indexOfRemovedUser)
                        {
                            userByIndex[key] -= 1;
                        }
                    }
                    countOfRemovedUsers++;
                }
            }

            Console.WriteLine(countOfRemovedUsers + " users have less than " + minCountOfRatings + " and were removed.");

            R_train = new DataMatrix(userByIndex.Count, itemByIndex.Count);
            R_test = new DataMatrix(userByIndex.Count, itemByIndex.Count);

            // Read file data into rating matrix
            Dictionary<int, int> trainCountByUser = new Dictionary<int, int>(); // count how many ratings in the train set of each user

            // Create a enumerator to enumerate each line in the file
            IEnumerable<string> linesInFile;
            if (shuffle)
            {
                Random rng = new Random(seed);
                var allLines = new List<string>(File.ReadAllLines(fileOfDataSet));
                allLines.Shuffle(rng);
                linesInFile = allLines.AsEnumerable<string>();
            }
            else
            {
                linesInFile = File.ReadLines(fileOfDataSet);
            }

            // Process each line and put ratings into training/testing sets
            List<Tuple<int, int, double>> R_train_cache = new List<Tuple<int, int, double>>();
            List<Tuple<int, int, double>> R_test_cache = new List<Tuple<int, int, double>>();

            //List<SparseVector> R_test_list = new List<SparseVector>(userByIndex.Count);
            //List<SparseVector> R_train_list = new List<SparseVector>(userByIndex.Count);
            //for (int i = 0; i < userByIndex.Count;i++ )
            //{
            //    R_test_list.Add(new SparseVector(itemByIndex.Count));
            //    R_train_list.Add(new SparseVector(itemByIndex.Count));
            //}

            foreach (string line in linesInFile)
            {
                if (line == "") { continue; }
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int fileIndexOfUser = int.Parse(tokens[0]);
                int fileIndexOfItem = int.Parse(tokens[1]);
                double rating = double.Parse(tokens[2]);
                if (userByIndex.ContainsKey(fileIndexOfUser))   // If this user was not removed
                {
                    int indexOfUser = userByIndex[fileIndexOfUser];
                    int indexOfItem = itemByIndex[fileIndexOfItem];
                    if (!trainCountByUser.ContainsKey(indexOfUser))
                    {
                        // Fill up the train set
                        //R_train[indexOfUser, indexOfItem] = rating;
                        R_train_cache.Add(new Tuple<int, int, double>(indexOfUser, indexOfItem, rating));// = rating;
                        trainCountByUser[indexOfUser] = 1;
                    }
                    else if (trainCountByUser[indexOfUser] < countOfRatingsForTrain)
                    {
                        // Fill up the train set
                        //R_train.Matrix.Storage.At(indexOfUser, indexOfItem, rating);
                        R_train_cache.Add(new Tuple<int, int, double>(indexOfUser, indexOfItem, rating));
                        trainCountByUser[indexOfUser]++;
                    }
                    else
                    {
                        // Fill up the test set
                        R_test_cache.Add(new Tuple<int, int, double>(indexOfUser, indexOfItem, rating));
                        //R_test.Matrix.Storage.At(indexOfUser, indexOfItem, rating);
                    }
                }
            }
            R_test = new DataMatrix(SparseMatrix.OfIndexed(R_test.UserCount, R_test.ItemCount, R_test_cache));
            R_train = new DataMatrix(SparseMatrix.OfIndexed(R_train.UserCount, R_train.ItemCount, R_train_cache));

            Debug.Assert(userByIndex.Count * countOfRatingsForTrain == R_train.NonZerosCount);
            Console.WriteLine(R_train.Matrix.ColumnSums().Sum());
        }
        #endregion
        #endregion

        #region Remove old records
        public static void RemoveOldUsers(int minTimestamp, string fileOfDataSet)
        {
            StringBuilder output = new StringBuilder();

            // Read the file to discover the whole matrix structure and mapping
            foreach (string line in File.ReadLines(fileOfDataSet))
            {
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int timestamp = int.Parse(tokens[3]);
                if (timestamp > minTimestamp)
                {
                    output.AppendLine(line);
                }
                if (output.Length > 1000000)
                {
                    using (StreamWriter w = File.AppendText(minTimestamp + ".data"))
                    {
                        w.WriteLine(output);
                        output.Clear();
                    }
                }

            }
            using (StreamWriter w = File.AppendText(minTimestamp + ".data"))
            {
                w.WriteLine(output);
                output.Clear();
            }
        }
        #endregion

        #region Remove cold users
        public static void RemoveColdUsers(int minRatingCount, string fileOfDataSet)
        {
            StringBuilder output = new StringBuilder();
            Dictionary<int, int> ratingCountByUser = new Dictionary<int, int>(); // count how many ratings of each user

            // Read the file to discover the whole matrix structure and mapping
            foreach (string line in File.ReadLines(fileOfDataSet))
            {
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int indexOfUser = int.Parse(tokens[0]);
                if (!ratingCountByUser.ContainsKey(indexOfUser))          // We update index only for new user
                {
                    ratingCountByUser[indexOfUser] = 1;             // Initialize the rating count for this new user
                }
                else { ratingCountByUser[indexOfUser]++; }
            }

            // Remove users with too few ratings
            foreach (string line in File.ReadLines(fileOfDataSet))
            {
                string[] tokens = line.Split(Constants.SplitSeperators, StringSplitOptions.RemoveEmptyEntries);
                int indexOfUser = int.Parse(tokens[0]);
                if (ratingCountByUser[indexOfUser] >= minRatingCount)
                {
                    output.AppendLine(tokens[0] + "," + tokens[1] + "," + tokens[2]);
                    if (output.Length > 1000000)
                    {
                        using (StreamWriter w = File.AppendText(minRatingCount + "PlusRatings_" + fileOfDataSet))
                        {
                            w.WriteLine(output);
                            output.Clear();
                        }
                    }
                }
            }
            using (StreamWriter w = File.AppendText(minRatingCount + "PlusRatings_" + fileOfDataSet))
            {
                w.WriteLine(output);
                output.Clear();
            }
        }
        #endregion

        #region Create matrix with random values
        /// <summary>
        /// Create a Matrix filled with random numbers from [0,1], uniformly distributed.
        /// </summary>
        public static Matrix<double> CreateRandomMatrixFromUniform(int rowCount, int columnCount, 
            double min, double max, int seed)
        {
            ContinuousUniform uniformDistribution = new ContinuousUniform(min, max, new Random(seed));
            Matrix<double> randomMatrix = Matrix.Build.Random(rowCount, columnCount, uniformDistribution);

            Debug.Assert(randomMatrix.Find(x => x > 1 && x < 0) == null);  // Check the numbers are in [0,1]

            return randomMatrix;
        }

        /// <summary>
        /// Create a Matrix filled with random numbers from Normal distribution
        /// </summary>
        public static Matrix<double> CreateRandomMatrixFromNormal(int rowCount, int columnCount,
            double mean, double stddev, int seed)
        {
            Normal normalDistribution = new Normal(mean, stddev, new Random(seed));
            Matrix<double> randomMatrix = Matrix.Build.Random(rowCount, columnCount, normalDistribution);

            return randomMatrix;
        }
        #endregion

        #region String formatting and printing
        public static string CreateHeading(string title)
        {
            string formatedTitle = "";
            formatedTitle += new String('*', Constants.RightPad + Constants.LeftPad + 2) + "\n";
            formatedTitle += title.PadLeft((Constants.RightPad + Constants.LeftPad + title.Length) / 2, ' ') + "\n";
            formatedTitle += new String('*', Constants.RightPad + Constants.LeftPad + 2) + "\n";
            return formatedTitle;
        }

        public static string PrintValue(string label, string value)
        {
            string formatedString = "";
            string labelToPrint = label;
            while (labelToPrint.Length > Constants.RightPad)
            {
                formatedString += labelToPrint.Substring(0, Constants.RightPad) + "│\n";
                labelToPrint = labelToPrint.Remove(0, Constants.RightPad);
            }
            formatedString += String.Format("{0}│{1}",
                labelToPrint.PadRight(Constants.RightPad, ' '),
                value.PadLeft(Constants.LeftPad, ' '));
            Console.WriteLine(formatedString);
            return formatedString;
        }

        public static string PrintValueToString(string label, string value)
        {
            return string.Format("{0}│{1}", label.PadRight(Constants.RightPad, ' '),
                value.PadLeft(Constants.LeftPad, ' '));
        }

        public static string PrintHeading(string title)
        {
            string heading = CreateHeading(title);
            Console.Write(heading);
            return heading;
        }

        public static void PrintEpoch(string label, int epoch, int maxEpoch)
        {
            if (epoch == 0 || epoch == maxEpoch - 1 || epoch % (int)Math.Ceiling(maxEpoch * 0.1) == 4)
            {
                PrintValue(label, (epoch + 1) + "/" + maxEpoch);
            }
        }
        public static void PrintEpoch(string label1, int epoch, int maxEpoch, string label2, double error, bool alwaysPrint = false)
        {
            if (alwaysPrint || epoch == 0 || epoch == maxEpoch - 1 || epoch % (int)Math.Ceiling(maxEpoch * 0.1) == 4)
            {
                PrintValue(label2 + "@" + label1 + " (" + (epoch + 1) + "/" + maxEpoch + ")", error.ToString("0.0000"));
            }
        }
        public static string PrintEpoch(string label1, int epoch, int maxEpoch, string label2, string message, bool alwaysPrint = false)
        {
            StringBuilder log = new StringBuilder();
            if (alwaysPrint || epoch == 0 || epoch == maxEpoch - 1 || epoch % (int)Math.Ceiling(maxEpoch * 0.1) == 4)
            {
                log.AppendLine(PrintValue(label2 + "@" + label1 + " (" + (epoch + 1) + "/" + maxEpoch + ")", message));
            }

            return log.ToString();
        }
        #endregion

        #region Timer & Excution control
        private static Stopwatch stopwatch;
        public static void StartTimer()
        {
            stopwatch = Stopwatch.StartNew();
        }

        public static string StopTimer()
        {
            stopwatch.Stop();
            double seconds = stopwatch.Elapsed.TotalMilliseconds / 1000;
            string log = string.Format("{0}│{1}s", "Computation time".PadRight(Constants.RightPad, ' '),
                seconds.ToString("0.000").PadLeft(Constants.LeftPad - 1, ' '));
            Console.WriteLine(log);
            return log;
        }

        public static void Pause()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.SetCursorPosition(0, Console.CursorTop - 2);
            Console.Write(new String(' ', Console.BufferWidth));
        }

        public static bool Ask()
        {
            Console.WriteLine("\nPress 'S' to skip or any key to run...");
            ConsoleKeyInfo key = Console.ReadKey();
            if (key.Key == ConsoleKey.S)
            {
                Console.WriteLine("Skipped.");
                return false;
            }
            else
            {
                Console.SetCursorPosition(0, Console.CursorTop - 2);
                Console.Write(new String(' ', Console.BufferWidth));
                return true;
            }
        }
        #endregion

        #region Load OMF
        /*
        public static Dictionary<Tuple<int, int>, double[]> LoadOMFDistributions(string fileName)
        {
            Dictionary<Tuple<int, int>, double[]> OMFDistributions = new Dictionary<Tuple<int, int>, double[]>();

            // Read the file to discover the whole matrix structure and mapping
            foreach (string line in File.ReadLines(fileName))
            {
                List<string> tokens = line.Split(Config.SplitSeperators, StringSplitOptions.RemoveEmptyEntries).ToList();
                int indexOfUser = int.Parse(tokens[0]);
                int indexOfItem = int.Parse(tokens[1]);

                OMFDistributions[new Tuple<int, int>(indexOfUser, indexOfItem)] = 
                    tokens.GetRange(2,tokens.Count-2).Select(x => double.Parse(x)).ToArray();
            }

            return OMFDistributions;
        }
        */
        #endregion
    }

    #region My extensions to .Net and Math.Net libraries
    public static class ExtensionsToDotNet
    {
        /// <summary>
        /// Add a function to IList interfance to shuffle the list with Fisher–Yates shuffle.
        /// See http://stackoverflow.com/questions/273313/randomize-a-listt-in-c-sharp
        /// and http://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public static class ExtensionsToMathNet
    {
        /// <summary>
        /// An extension function to compute the sum of squares of non-zero elements
        /// </summary>
        public static double SquaredSum(this Matrix<double> matrix)
        {
            return matrix.PointwisePower(2).RowSums().Sum();
            //return Math.Pow(matrix.FrobeniusNorm(), 2);
        }

        public static double SquaredSum(this Vector<double> vector)
        {
            return vector.PointwisePower(2).Sum();
            //return Math.Pow(matrix.FrobeniusNorm(), 2);
        }

        public static int GetNonZerosCount(this Vector<double> vector)
        {
            //Debug.Assert(!vector.Storage.IsDense);
            return ((SparseVector)vector).NonZerosCount;
        }

        public static int GetNonZerosCount(this Matrix<double> matrix)
        {
            //Debug.Assert(!vector.Storage.IsDense);
            return ((SparseMatrix)matrix).NonZerosCount;
        }
    }
    #endregion
}
