using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;

namespace LibRecSysCS.Absolute
{
    class RatingVector
    {
        SparseVector ratingVector;
        public RatingVector(Vector<double> ratingVector)
        {
            this.ratingVector = ratingVector.Storage.IsDense ? SparseVector.OfVector(ratingVector) : (SparseVector)ratingVector;
        }

        public IEnumerable<Tuple<int, double>> Ratings
        {
            get { return ratingVector.EnumerateIndexed(Zeros.AllowSkip); }
        }
    }
}
