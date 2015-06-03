﻿using LibRecSysCS.Absolute;

namespace LibRecSysCS.Evaluation
{
    /// <summary>
    /// Mean Absolute Error, applies to numerical values, the lower the better.
    /// See http://en.wikipedia.org/wiki/Mean_absolute_error
    /// </summary>
    public class MAE
    {
        public static double Evaluate(DataMatrix correctMatrix, DataMatrix predictedMatrix)
        {
            return (correctMatrix.Matrix - predictedMatrix.Matrix)
                .ColumnAbsoluteSums().Sum() / correctMatrix.NonZerosCount;
        }
    }
}
