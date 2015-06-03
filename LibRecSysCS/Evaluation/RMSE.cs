﻿using MathNet.Numerics.LinearAlgebra.Double;
using LibRecSysCS.Absolute;
using System;
using System.Diagnostics;

namespace LibRecSysCS.Evaluation
{
    /// <summary>
    /// Root-Mean-Square Error.
    /// See https://www.kaggle.com/wiki/RootMeanSquaredError
    /// </summary>
    public class RMSE
    {
        public static double Evaluate(DataMatrix correctMatrix, DataMatrix predictedMatrix)
        {
            //Debug.Assert(correctMatrix.NonZerosCount == predictedMatrix.NonZerosCount);
            double enumerator = (predictedMatrix.Matrix - correctMatrix.Matrix).FrobeniusNorm();
            return enumerator / Math.Sqrt(correctMatrix.NonZerosCount);
        }
    }
}
