using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnnLab.Lab2
{
    public class Task1
    {

        private static void Neighbourhood(double[] hood, int bmu, double sigma)
        {
            double denominator = 2 * sigma * sigma;
            for (int i = 0; i < hood.Length; i++)
            {
                double numerator = i - bmu;
                numerator *= numerator;
                hood[i] = Math.Exp(-numerator/denominator);
            }
        }

        public static double NeighbourhoodWidth(int t, double tou, double sigma0)
        {
            return sigma0 * Math.Exp(-t / tou);
        }

        public static double LearningRate(int t, double tou, double n0)
        {
            return n0 * Math.Exp(-t / tou);
        }

        private static int BMU(Matrix<double> input, Matrix<double> weights, int target)
        {
            int bmu = 0;
            double dx = input[target, 0] - weights[bmu, 0], dy = input[target, 1] - weights[bmu, 1];
            double dist2 = dx * dx + dy * dy;
            for (int i = 1; i < weights.Rows; i++)
            {
                dx = input[target, 0] - weights[i, 0];
                dy = input[target, 1] - weights[i, 1];
                double newDist2 = dx * dx + dy * dy;
                if (newDist2 < dist2)
                {
                    bmu = i;
                    dist2 = newDist2;
                }
            }
            return bmu;
        }

        private static int TotalJobs, JobsCompleted = 0;

        public static void Run(IEnumerable<string> args)
        {
#if DEBUG
            args = new List<string> { "100" };
#endif
            if (args.Count() != 1)
            {
                Console.WriteLine("Usage: lab2.task1 <sigma_0>");
                return;
            }
            double sigma0 = int.Parse(args.First());
            if (sigma0 < 1)
            {
                Console.WriteLine("sigma_0 must be larger than 0");
                return;
            }
            double n0 = 0.1;
            int T = 200;
            double sigmaconv = 0.9;
            double nconv = 0.01;
            int iters_order = (int)1E3, iters_conv = (int)5E4;
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string fileStr = sigma0 + "_" + dateStr;

            TotalJobs = iters_order + iters_conv;
            Thread progress = new Thread(() => Progress.ProgressFunc(ref TotalJobs, ref JobsCompleted));
            progress.Start();

            Matrix<double> input = GenerateTriangleInput(1000, 1, 0, 0.5, 1);
            Matrix<double> weights = new Matrix<double>(100, 2);
            Random rng = new Random();
            for (int i = 0; i < weights.Rows; i++)
                for (int j = 0; j < weights.Cols; j++)
                    weights[i, j] = rng.NextDouble();
            double[] hood = new double[weights.Rows];

            // ordering phase
            for (int t = 0; t < iters_order; t++)
            {
                int target = rng.Next(input.Rows);
                int bmu = BMU(input, weights, target);
                double sigma = NeighbourhoodWidth(t, T, sigma0);
                double learningRate = LearningRate(t, T, n0);
                Neighbourhood(hood, bmu, sigma);
                for (int i = 0; i < weights.Rows; i++)
                {
                    weights[i, 0] += hood[i] * learningRate * (input[target, 0] - weights[i, 0]);
                    weights[i, 1] += hood[i] * learningRate * (input[target, 1] - weights[i, 1]);
                }
                JobsCompleted++;
            }

            using (StreamWriter sw = new StreamWriter(new FileStream("lab2task1_ordering_" + fileStr + ".txt", FileMode.CreateNew)))
            {
                for (int i = 0; i < weights.Rows; i++)
                    sw.WriteLine(weights[i, 0].ToString(CultureInfo.InvariantCulture) + ", " + weights[i, 1].ToString(CultureInfo.InvariantCulture));
            }

            // convergance phase
            double[][] hoods = new double[input.Rows][];
            for (int t = 0; t < iters_conv; t++)
            {
                int target = rng.Next(input.Rows);
                int bmu = BMU(input, weights, target);
                if (hoods[bmu] == null)
                    Neighbourhood(hoods[bmu] = new double[weights.Rows], bmu, sigmaconv);
                for (int i = 0; i < weights.Rows; i++)
                {
                    weights[i, 0] += hoods[bmu][i] * nconv * (input[target, 0] - weights[i, 0]);
                    weights[i, 1] += hoods[bmu][i] * nconv * (input[target, 1] - weights[i, 1]);
                }
                JobsCompleted++;
            }

            string filename = "lab2task1_convergance_" + fileStr + ".txt";
            Console.WriteLine("Writing to " + filename + "...");
            using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.Create)))
            {
                for (int i = 0; i < weights.Rows; i++)
                    sw.WriteLine(weights[i, 0].ToString(CultureInfo.InvariantCulture) + ", " + weights[i, 1].ToString(CultureInfo.InvariantCulture));
            }
            
            filename = "lab2task1_input_" + fileStr + ".txt";
            Console.WriteLine("Writing to " + filename + "...");
            using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.CreateNew)))
            {
                for (int i = 0; i < input.Rows; i++)
                    sw.WriteLine(input[i, 0].ToString(CultureInfo.InvariantCulture) + ", " + input[i, 1].ToString(CultureInfo.InvariantCulture));
            }
            Console.WriteLine("Done!");
        }

        private static Matrix<double> GenerateTriangleInput(int nPoints, double x1, double y1, double x2, double y2)
        {
            Random rng = new Random();
            Matrix<double> input = new Matrix<double>(nPoints, 2);
            // http://mathworld.wolfram.com/TrianglePointPicking.html
            for (int i = 0; i < input.Rows; i++)
            {
                double a1 = rng.NextDouble(), a2 = rng.NextDouble();
                if (a1 + a2 < 1) {
                    input[i, 0] = a1 * x1 + a2 * x2;
                    input[i, 1] = a1 * y1 + a2 * y2;
                }
                else
                {
                    input[i, 0] = (a1 * x1 + a2 * x2) - x2;
                    input[i, 1] = y2 - (a1 * y1 + a2 * y2);
                }
            }
            return input;
        }
    }
}
