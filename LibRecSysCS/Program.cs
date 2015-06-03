using MathNet.Numerics;
using LibRecSysCS.Experiments;
using System.IO;
using System;
using LibRecSysCS.Absolute;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Providers.LinearAlgebra.Mkl;
using MathNet.Numerics.Distributions;

namespace LibRecSysCS
{
    class Program
    {
        static void Main(string[] args)
        {
            CrfExpDesign.UserKNNonMovieLens100K();
            CrfExpDesign.NMFonMovieLens100K();
            CrfExpDesign.PrefKNNonMovieLens100K();
            CrfExpDesign.PrefNMFonMovieLens100K();
            CrfExpDesign.PrefMRFonMovieLens100K();
            //CrfExpDesign.PrefCRFonMovieLens100K();


            //CrfExpDesign.PrefCRFonMovieLens1M();
            //CrfExpDesign.UserKNNonEachMovie();
            //CrfExpDesign.NMFonEachMovie();
            //CrfExpDesign.PrefKNNonEachMovie();
            //CrfExpDesign.PrefNMFonEachMovie();
            //CrfExpDesign.PrefMRFonEachMovie();

            //CrfExpDesign.UserKNNonMovieLens20M();
            //CrfExpDesign.NMFonMovieLens20M();
            //CrfExpDesign.PrefKNNonMovieLens20M();
            //CrfExpDesign.PrefNMFonMovieLens20M();
            //CrfExpDesign.PrefMRFonMovieLens20M();

            //CrfExpDesign.UserKNNonAmazon();
            //CrfExpDesign.NMFonAmazon();
            //CrfExpDesign.PrefKNNonAmazon();
            //CrfExpDesign.PrefNMFonAmazon();
            //CrfExpDesign.PrefMRFonAmazon();

            Utils.Pause();
        }
    }
}
