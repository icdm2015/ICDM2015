﻿using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LibRecSysCS.Absolute
{
    [Serializable]
    public class DataMatrix
    {
        #region Variables
        private SparseMatrix ratingMatrix;
        private Matrix<double> userSimilarities;
        #endregion

        #region Properties
        public int UserCount { get { return ratingMatrix.RowCount; } }
        public int ItemCount { get { return ratingMatrix.ColumnCount; } }
        public int NonZerosCount { get { return ratingMatrix.NonZerosCount; } }
        public double Density { get { return (double)NonZerosCount / (UserCount * ItemCount); } }
        public SparseMatrix Matrix { get { return ratingMatrix; } }

        [Obsolete]
        public Matrix<double> UserSimilarities
        {
            get
            {
                Debug.Assert(!(userSimilarities == null));
                return userSimilarities;
            }
            set
            {
                userSimilarities = value;
            }
        }
        public double this[int indexOfUser, int indexOfItem]
        {
            get { return ratingMatrix[indexOfUser, indexOfItem]; }
            set { ratingMatrix[indexOfUser, indexOfItem] = value; }
        }

        public Vector<double> this[int userIndex]
        {
            get { return ratingMatrix.Row(userIndex); }
        }

        public IEnumerable<Tuple<int, Vector<double>>> Users
        {
            get { return ratingMatrix.EnumerateRowsIndexed(); }
        }
        public IEnumerable<Tuple<int, int, double>> Ratings
        {
            get { return ratingMatrix.EnumerateIndexed(Zeros.AllowSkip); }
        }
        #endregion

        #region Constructors
        public DataMatrix(int userCount, int itemCount)
        {
            ratingMatrix = new SparseMatrix(userCount, itemCount);
        }
        public DataMatrix(SparseMatrix ratingMatrix)
        {
            this.ratingMatrix = ratingMatrix;
        }
        public DataMatrix(Matrix<double> ratingMatrix)
        {
            this.ratingMatrix = ratingMatrix.Storage.IsDense ? SparseMatrix.OfMatrix(ratingMatrix) : (SparseMatrix)ratingMatrix;
        }
        #endregion

        #region DatasetBrief
        public string DatasetBrief(string title)
        {
            string brief = "";
            brief += Utils.CreateHeading(title);
            brief += Utils.PrintValueToString("# of users", UserCount.ToString("D")) + "\n";
            brief += Utils.PrintValueToString("# of items", ItemCount.ToString("D")) + "\n";
            brief += Utils.PrintValueToString("# of ratings", NonZerosCount.ToString("D")) + "\n";
            brief += Utils.PrintValueToString("Density level", Density.ToString("P")) + "\n";
            brief += Utils.PrintValueToString("Global mean", GetGlobalMean().ToString("0.0000000"));
            return brief;
        }
        #endregion

        /// <summary>
        /// Normalize the values into min-max interval
        /// </summary>
        public void NormalizeInplace(double oldMin, double oldMax, double newMin, double newMax)
        {
            SparseMatrix ratingMatrixNormalized = new SparseMatrix(UserCount, ItemCount);
            foreach (var element in ratingMatrix.EnumerateIndexed(Zeros.AllowSkip))
            {
                int indexOfUser = element.Item1;
                int indexOfItem = element.Item2;
                double value = element.Item3;
                ratingMatrixNormalized[indexOfUser, indexOfItem] = ((newMax - newMin) *
                    (value == Constants.ZeroInSparseMatrix ? 0 : value - oldMin) / (oldMax - oldMin) + newMin);
            }
            //TODO: Check why MapInplace will change zero elements??
            //ratingMatrix.MapInplace(x => x = ((toMax - toMin) * (x - min) / (max - min) + toMin), Zeros.AllowSkip;
            ratingMatrix = ratingMatrixNormalized;
        }


        #region Other methods
        // Returns the average of all known ratings
        public double GetGlobalMean()
        {
            return ratingMatrix.RowSums().Sum() / ratingMatrix.NonZerosCount;
        }
        // Returns the average ratings of each user
        public Vector<double> GetUserMeans()
        {
            return ratingMatrix.RowSums() / (ratingMatrix.PointwiseDivide(ratingMatrix).RowSums());
        }

        // Returns the average ratings of each item
        public Vector<double> GetItemMeans()
        {
            return ratingMatrix.ColumnSums() / (ratingMatrix.PointwiseDivide(ratingMatrix).ColumnSums());
        }

        public Vector<double> GetRow(int userIndex)
        {
            return ratingMatrix.Row(userIndex);
        }

        public int GetNonZerosCountOfRow(int indexOfRow)
        {
            return ratingMatrix.Row(indexOfRow).GetNonZerosCount();
        }

        public DataMatrix IndexesOfNonZeroElements()
        {
            return new DataMatrix(ratingMatrix.PointwiseDivide(ratingMatrix));
        }

        public DataMatrix Multiply(double scalar)
        {
            return new DataMatrix(ratingMatrix.Multiply(scalar));
        }

        public Matrix<double> PointwiseMultiply(Matrix<double> other)
        {
            return ratingMatrix.PointwiseMultiply(other);
        }

        /// <summary>
        /// Merge another matrix to this matrix. It is required that two matrixes do not overlap
        /// </summary>
        /// <param name="matrix"></param>
        public void MergeNonOverlap(DataMatrix matrix)
        {
            int count = ratingMatrix.NonZerosCount;
            ratingMatrix += matrix.Matrix;
            Debug.Assert(count + matrix.Matrix.NonZerosCount == ratingMatrix.NonZerosCount);
        }
        #endregion


        public void Quantization(double min, double range, List<double> quantizer)
        {
            //SparseMatrix ratingMatrixQuantized = new SparseMatrix(UserCount, ItemCount);
            List<Tuple<int, int, double>> ratingMatrixQuantizedCache = new List<Tuple<int, int, double>>();

            int binCount = quantizer.Count;
            double binSize = range / binCount;

            foreach (var element in ratingMatrix.EnumerateIndexed(Zeros.AllowSkip))
            {
                int indexOfUser = element.Item1;
                int indexOfItem = element.Item2;
                double value = element.Item3;
                for (int indexOfBin = 0; indexOfBin < binCount; indexOfBin++)
                {
                    if (value < (indexOfBin + 1) * binSize + min)
                    {
                        ratingMatrixQuantizedCache.Add(new Tuple<int, int, double>(indexOfUser, indexOfItem, quantizer[indexOfBin]));
                        break;
                    }
                }
            }
            ratingMatrix = SparseMatrix.OfIndexed(UserCount, ItemCount, ratingMatrixQuantizedCache);
        }


        public Dictionary<int, List<int>> GetItemsByUser()
        {
            Dictionary<int, List<int>> itemsByUser = new Dictionary<int, List<int>>();
            foreach (var user in ratingMatrix.EnumerateRowsIndexed())
            {
                int indexOfUser = user.Item1;
                SparseVector ratingsOfUser = (SparseVector)user.Item2;
                List<int> itemsOfUser = new List<int>(ratingsOfUser.NonZerosCount);
                foreach (var item in ratingsOfUser.EnumerateIndexed(Zeros.AllowSkip))
                {
                    itemsOfUser.Add(item.Item1);
                }
                Debug.Assert(itemsOfUser.Count == ratingsOfUser.NonZerosCount);
                itemsByUser[indexOfUser] = itemsOfUser;
            }

            return itemsByUser;
        }
    }
}
