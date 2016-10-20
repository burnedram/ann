using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnnLab.Lab2
{
    public class Task2
    {

        private static void Neighbourhood(double[,] hood, Tuple<int, int> bmu, double sigma)
        {
            double denominator = 2 * sigma * sigma;
            for (int i = 0; i < hood.GetLength(0); i++)
                for (int j = 0; j < hood.GetLength(1); j++)
                {
                    double dx = i - bmu.Item1, dy = j - bmu.Item2;
                    double numerator = dx * dx + dy * dy;
                    hood[i, j] = Math.Exp(-numerator/denominator);
                }
        }

        private static Tuple<int, int> BMU(Matrix<double> wine, double[,,] weights, int wineIndex)
        {
            Tuple<int, int> bmu = null;
            double dist2 = double.MaxValue;
            for (int i = 0; i < weights.GetLength(0); i++)
                for (int j = 0; j < weights.GetLength(1); j++)
                {
                    Tuple<int, int> candidate = Tuple.Create(i, j);
                    double newDist2 = wine.Distance2Between(wineIndex, weights, candidate);
                    if (newDist2 < dist2)
                    {
                        bmu = candidate;
                        dist2 = newDist2;
                    }
                }
            return bmu;
        }

        public static int TotalJobs, JobsCompleted = 0;

        public static void Run(IEnumerable<string> args)
        {
            double sigma_0 = 30, eta_0 = 0.1, sigma_conv = 0.9, eta_conv = 0.01;
            int T_order = (int)1E3, T_conv = (int)2E4, tou = 300;

            if (!args.Any())
            {
                args = new List<string> { "C:\\ann\\wine.data.txt" };
            }
            if (args.Count() > 1)
            {
                Console.WriteLine("Usage: lab2.task2 <wine_file>");
                return;
            }
            string wineFile = args.First();
            int[] wineClasses;
            Matrix<double> wine = ReadWine(wineFile, out wineClasses);
            NormalizeMeanAndVarInPlace(wine);
            double[,,] weights = GenerateRandomWeights();
            double[,] hood = new double[20, 20];
            Random rng = new Random();

            TotalJobs = T_order + T_conv;
            Thread progress = new Thread(() => Progress.ProgressFunc(ref TotalJobs, ref JobsCompleted));
            progress.Start();

            // ordering phase
            for (int t = 0; t < T_order; t++)
            {
                int wineIndex = rng.Next(wine.Rows);
                var bmu = BMU(wine, weights, wineIndex);
                Neighbourhood(hood, bmu, Task1.NeighbourhoodWidth(t, tou, sigma_0));
                double eta_t = Task1.LearningRate(t, tou, eta_0);
                for (int i = 0; i < weights.GetLength(0); i++)
                    for (int j = 0; j < weights.GetLength(1); j++)
                    {
                        if (hood[i, j] == 0)
                            continue;
                        for (int k = 0; k < weights.GetLength(2); k++)
                            weights[i, j, k] += hood[i, j] * eta_t * (wine[wineIndex, k] - weights[i, j, k]);
                    }
                JobsCompleted++;
            }

            // convergance phase
            double[,][,] hoods = new double[weights.GetLength(0), weights.GetLength(1)][,];
            for (int t = 0; t < T_conv; t++)
            {
                int wineIndex = rng.Next(wine.Rows);
                var bmu = BMU(wine, weights, wineIndex);
                double[,] myHood = hoods[bmu.Item1, bmu.Item2];
                if (myHood == null)
                    Neighbourhood(hoods[bmu.Item1, bmu.Item2] = myHood = new double[20, 20], bmu, sigma_conv);
                for (int i = 0; i < weights.GetLength(0); i++)
                    for (int j = 0; j < weights.GetLength(1); j++)
                        for (int k = 0; k < weights.GetLength(2); k++)
                            weights[i, j, k] += myHood[i, j] * eta_conv * (wine[wineIndex, k] - weights[i, j, k]);
                JobsCompleted++;
            }

            // coloring phase
            int[,] colors = new int[20, 20];
            for (int wineIndex = 0; wineIndex < wine.Rows; wineIndex++)
            {
                var bmu = BMU(wine, weights, wineIndex);
                colors[bmu.Item1, bmu.Item2] = wineClasses[wineIndex];
            }

            string dateStr = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string filename = "lab2task2_" + dateStr + ".txt";
            Console.WriteLine("Writing to " + filename + "...");
            using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.CreateNew)))
            {
                for (int i = 0; i < colors.GetLength(0); i++)
                    for (int j = 0; j < colors.GetLength(1); j++)
                        sw.WriteLine(i + "," + j + "," + colors[i, j]);
            }

            string errorLog = "lab2task2_" + dateStr + ".log";
            Console.WriteLine("Executing MATLAB script...");
            if (!MATLAB.RunScript(errorLog, "Lab2Task2Grapher", "'" + filename + "'"))
                Console.WriteLine("An error occured while running MATLAB, check the log\n\tLog file:" + errorLog);
            Console.WriteLine("Done!");
        }

        private static double[,,] GenerateRandomWeights()
        {
            double[,,] weights = new double[20, 20, 13];
            Random rng = new Random();
            for (int i = 0; i < weights.GetLength(0); i++)
                for (int j = 0; j < weights.GetLength(1); j++)
                    for (int k = 0; k < weights.GetLength(2); k++)
                        weights[i, j, k] = rng.NextDouble();
            return weights;
        }

        public static void NormalizeMeanAndVarInPlace(Matrix<double> wine)
        {
            double[] mean = Enumerable.Range(0, wine.Cols).Select(j => wine.Col(j).Average()).ToArray();
            for (int i = 0; i < wine.Rows; i++)
                for (int j = 0; j < wine.Cols; j++)
                    wine[i, j] -= mean[j];

            int N = wine.Rows - 1;
            double[] var = Enumerable.Range(0, wine.Cols).Select(j => Math.Sqrt(wine.Col(j).Sum(x => x * x) / N)).ToArray();
            for (int i = 0; i < wine.Rows; i++)
                for (int j = 0; j < wine.Cols; j++)
                    wine[i, j] /= var[j];
        }

        private static Matrix<double> ReadWine(string path, out int[] classes)
        {
            int nLines = CountLines(path);
            Matrix<double> m = new Matrix<double>(nLines, 13);
            classes = new int[nLines];
            using (StreamReader sr = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                string line;
                for (int i = 0; (line = sr.ReadLine()) != null; i++)
                {
                    var vals = line.Split(',');
                    classes[i] = int.Parse(vals[0]);
                    for (int j = 0; j < 13; j++)
                        m[i, j] = double.Parse(vals[1 + j], CultureInfo.InvariantCulture);
                }
            }
            return m;
        }

        private static int CountLines(string filePath)
        {
            int lines = 0;
            using (StreamReader sr = new StreamReader(new FileStream(filePath, FileMode.Open)))
            {
                while (sr.ReadLine() != null)
                    lines++;
            }
            return lines;
        }
    }
    static class ArrayExt
    {
        public static double Distance2Between(this Matrix<double> m, int mi, double[,,] arr, Tuple<int, int> arrij)
        {
            double dist2 = 0;
            for (int mj = 0; mj < m.Cols; mj++)
            {
                double d = m[mi, mj] - arr[arrij.Item1, arrij.Item2, mj];
                dist2 += d * d;
            }
            return dist2;
        }
    }
}
