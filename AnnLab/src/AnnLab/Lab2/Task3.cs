using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnnLab.Lab2
{
    public class Task3
    {

        private static void RadialNumerators(Matrix<double> radial, Matrix<double> ost, Matrix<double> weights, int ostIndex)
        {
            for (int j = 0; j < radial.Cols; j++)
            {
                double dx = ost[ostIndex, 0] - weights[j, 0], dy = ost[ostIndex, 1] - weights[j, 1];
                double numerator = dx * dx + dy * dy;
                radial[0, j] = Math.Exp(-numerator / 2);
            }
        }

        private static void Radial(Matrix<double> radial)
        {
            double sum = radial.Row(0).Sum();
            for (int j = 0; j < radial.Cols; j++)
            {
                radial[0, j] /= sum;
            }
        }

        class JobDescription
        {
            public Matrix<double> ost { get; set; }
            public int[][] ostClasses { get; set; }
            public int iters_kohonen { get; set; }
            public int iters_perceptron { get; set; }
            public double eta_kohonen { get; set; }
            public double eta_perceptron { get; set; }
            public double beta { get; set; }

            public int k { get; set; }
        }

        class JobResult
        {
            public int k { get; set; }

            public Matrix<double> weights { get; set; }
            public Lab1.NeuralNetwork nn { get; set; }
            public Matrix<double> radial { get; set; }
            public double traniningErrorRate { get; set; }
            public double validationErrorRate { get; set; }
        }

        private static JobResult RunJob(JobDescription job)
        {
            Matrix<double> weights = GenerateRandomWeights(job.k, -1, 1);
            Matrix<double> radial = new Matrix<double>(1, job.k);
            Random rng = new Random();

            // kohonen learning phase
            for (int t = 0; t < job.iters_kohonen; t++)
            {
                int ostIndex = rng.Next(job.ost.Rows);
                RadialNumerators(radial, job.ost, weights, ostIndex);
                int j_0 = radial.IndexOfMax();
                weights[j_0, 0] += job.eta_kohonen * (job.ost[ostIndex, 0] - weights[j_0, 0]);
                weights[j_0, 1] += job.eta_kohonen * (job.ost[ostIndex, 1] - weights[j_0, 1]);
            }

            Lab1.NeuralNetwork nn = new Lab1.NeuralNetwork(job.beta, new int[] { job.k, 1 });
            nn.RandomizeBiases(-1, 1);
            nn.RandomizeWeights(-1, 1);
            Matrix<double>[] ostRadials = new Matrix<double>[job.ost.Rows];
            for (int i = 0; i < job.ost.Rows; i++)
            {
                RadialNumerators(ostRadials[i] = new Matrix<double>(1, job.k), job.ost, weights, i);
                Radial(ostRadials[i]);
            }
            var radialIndices = Enumerable.Range(0, job.ost.Rows).ToList();
            radialIndices.Shuffle();
            Matrix<double>[] trainingRadials = new Matrix<double>[(int)Math.Round(job.ost.Rows * 0.7)];
            int[][] trainingClasses = new int[trainingRadials.Length][];
            for (int i = 0; i < trainingRadials.Length; i++)
            {
                trainingRadials[i] = ostRadials[radialIndices[i]];
                trainingClasses[i] = job.ostClasses[radialIndices[i]];
            }
            Matrix<double>[] validationRadials = new Matrix<double>[job.ost.Rows - trainingRadials.Length];
            int[][] validationClasses = new int[validationRadials.Length][];
            for (int i = 0; i < validationRadials.Length; i++)
            {
                validationRadials[i] = ostRadials[radialIndices[trainingRadials.Length + i]];
                validationClasses[i] = job.ostClasses[radialIndices[trainingRadials.Length + i]];
            }

            // perceptron learning phase
            for (int t = 0; t < job.iters_perceptron; t++)
            {
                int ostIndex = rng.Next(trainingRadials.Length);
                nn.Train(trainingRadials[ostIndex], trainingClasses[ostIndex], job.eta_perceptron);
            }

            double trainingErrorRate = Lab1.Task4a.ErrorRate(trainingRadials, trainingClasses, nn);
            double validationErrorRate = Lab1.Task4a.ErrorRate(validationRadials, validationClasses, nn);
            var result = new JobResult
            {
                k = job.k,
                weights = weights,
                nn = nn,
                radial = radial,
                traniningErrorRate = trainingErrorRate,
                validationErrorRate = validationErrorRate
            };
            Interlocked.Increment(ref JobsCompleted);
            return result;
        }

        private static int TotalJobs, JobsCompleted = 0;

        public static void Run(IEnumerable<string> args)
        {
            if (!args.Any())
                args = new List<string> { "1", "20", "5", "20" };
            if (args.Count() < 2)
            {
                Console.WriteLine("Usage: lab2.task3 <ost_file> <kLow> <kHigh> <kDumps...>");
                return;
            }
            string ostFile;
            int koffset;
            if (int.TryParse(args.First(), out koffset))
            {
                if (args.Count() < 3)
                {
                    Console.WriteLine("Usage: lab2.task3 <ost_file> <kLow> <kHigh> <kDumps...>");
                    return;
                }
                ostFile = "C:\\ann\\task3.txt";
                koffset = 0;
            }
            else
            {
                ostFile = args.First();
                koffset = 1;
            }
            int klow = int.Parse(args.Skip(koffset).First()), khigh = int.Parse(args.Skip(koffset + 1).First());
            if (klow < 1)
            {
                Console.WriteLine("kLow must be larger than 0");
                return;
            }
            if (khigh < klow)
            {
                Console.WriteLine("kHigh must be larger than kLow");
                return;
            }
            List<int> kDumps = args.Skip(koffset + 2).Select(x => int.Parse(x)).ToList();

            int[] ks = Enumerable.Range(klow, khigh - klow + 1).ToArray();
            int iters_kohonen = (int)1E5;
            double eta_kohonen = 0.02;
            int iters_perceptron = 3000;
            double beta = 0.5, eta_perceptron = 0.1;
            int runsPerJob = 20;

            int[][] ostClasses;
            Matrix<double> ost = ReadOst(ostFile, out ostClasses);

            var jobs = ks.SelectMany(k => Enumerable.Repeat(new JobDescription
            {
                ost = ost,
                ostClasses = ostClasses,
                iters_kohonen = iters_kohonen,
                iters_perceptron = iters_perceptron,
                eta_kohonen = eta_kohonen,
                eta_perceptron = eta_perceptron,
                beta = beta,
                k = k,
            }, runsPerJob));
            TotalJobs = jobs.Count();

            Thread progress = new Thread(() => Progress.ProgressFunc(ref TotalJobs, ref JobsCompleted));
            progress.Start();

            var results = jobs.AsParallel().Select(RunJob).ToList();
            var resultsByK = results.GroupBy(res => res.k).OrderBy(byK => byK.Key);

            string dateStr = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string filename = "lab2task3_k_" + dateStr + ".txt";
            Console.WriteLine("Writing to " + filename + "...");
            using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.CreateNew)))
            {
                foreach (var res in resultsByK)
                {
                    sw.WriteLine(res.Key + "," + res.Average(r => r.validationErrorRate).ToString(CultureInfo.InvariantCulture));

                    if (kDumps.Contains(res.Key))
                    {
                        string fileStr = res.Key + "_" + dateStr;
                        var minError = res.OrderBy(r => r.validationErrorRate).First();

                        string dumpname = "lab2task3_kohonen_" + fileStr + ".txt";
                        Console.WriteLine("Writing to " + dumpname + "...");
                        using (StreamWriter sw2 = new StreamWriter(new FileStream(dumpname, FileMode.CreateNew)))
                        {
                            for (int i = 0; i < minError.weights.Rows; i++)
                                sw2.WriteLine(minError.weights[i, 0].ToString(CultureInfo.InvariantCulture) + "," + minError.weights[i, 1].ToString(CultureInfo.InvariantCulture));
                        }

                        BoundaryDump(fileStr, ost, minError.weights, minError.radial, minError.nn);
                    }
                }
            }

            string errorLog = "lab2task3_" + dateStr + ".log";
            Console.WriteLine("Executing MATLAB script...");
            if (!MATLAB.RunScript(errorLog, "Lab2Task3Grapher", "'" + ostFile + "'", "'lab2task3_%s_" + dateStr + ".txt'", "[" + string.Join(" ", kDumps) + "]"))
                Console.WriteLine("An error occured while running MATLAB, check the log\n\tLog file:" + errorLog);
            Console.WriteLine("Done!");
        }

        private static void BoundaryDump(string fileStr, Matrix<double> ost, Matrix<double> weights, Matrix<double> radial, Lab1.NeuralNetwork nn)
        {
            //double x_min = weights.Col(0).Min(), x_max = weights.Col(0).Max();
            //double y_min = weights.Col(1).Min(), y_max = weights.Col(1).Max();
            double x_min = ost.Col(0).Min(), x_max = ost.Col(0).Max();
            double y_min = ost.Col(1).Min(), y_max = ost.Col(1).Max();
            double x_stepSize = (x_max - x_min) / 999;
            double classification_precision = 1E-6;

            Matrix<double> zeroOst = new Matrix<double>(1, 2);
            List<Tuple<double, double>> boundary = new List<Tuple<double, double>>(1000);
            for (double x = x_min; x <= x_max; x += x_stepSize)
            {
                zeroOst[0, 0] = x;
                zeroOst[0, 1] = y_max;
                RadialNumerators(radial, zeroOst, weights, 0);
                Radial(radial);
                nn.FeedPattern(radial);
                double max_classification = nn.Output[0, 0];
                zeroOst[0, 1] = y_min;
                RadialNumerators(radial, zeroOst, weights, 0);
                Radial(radial);
                nn.FeedPattern(radial);
                double min_classificiaton = nn.Output[0, 0];
                if (max_classification * min_classificiaton > 0)
                    continue;

                double classification = max_classification;
                double range_max = y_max, range_min = y_min;
                while (Math.Abs(classification) > classification_precision)
                {
                    zeroOst[0, 1] = (range_max - range_min) / 2 + range_min;
                    RadialNumerators(radial, zeroOst, weights, 0);
                    Radial(radial);
                    nn.FeedPattern(radial);
                    classification = nn.Output[0, 0];
                    if (classification * max_classification > 0)
                        range_max = zeroOst[0, 1];
                    else
                        range_min = zeroOst[0, 1];
                }
                boundary.Add(Tuple.Create(zeroOst[0, 0], zeroOst[0, 1]));
            }

            string filename = "lab2task3_boundary_" + fileStr + ".txt";
            Console.WriteLine("Writing to " + filename + "...");
            using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.CreateNew)))
            {
                foreach (var xy in boundary)
                    sw.WriteLine(xy.Item1.ToString(CultureInfo.InvariantCulture) + "," + xy.Item2.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static Matrix<double> GenerateRandomWeights(int k, double wmin, double wmax)
        {
            Matrix<double> weights = new Matrix<double>(k, 2);
            Random rng = new Random();
            for (int i = 0; i < weights.Rows; i++)
                for (int j = 0; j < weights.Cols; j++)
                    weights[i, j] = rng.NextDouble() * (wmax - wmin) + wmin;
            return weights;
        }

        private static Matrix<double> ReadOst(string path, out int[][] classes)
        {
            using (StreamReader sr = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                string allTheData = sr.ReadToEnd();
                string[] split = allTheData.Split(null);
                int nSamples = split.Length / 3;
                Matrix<double> m = new Matrix<double>(nSamples, 2);
                classes = new int[nSamples][];
                for (int i = 0; i < nSamples; i++)
                {
                    classes[i] = new int[] { int.Parse(split[i * 3]) };
                    m[i, 0] = double.Parse(split[i*3 + 1], CultureInfo.InvariantCulture);
                    m[i, 1] = double.Parse(split[i*3 + 2], CultureInfo.InvariantCulture);
                }
                return m;
            }
        }

    }
    static class ArrExt
    {
        public static int IndexOfMax(this Matrix<double> arr)
        {
            int index = -1;
            double max = double.MinValue;
            for (int j = 0; j < arr.Cols; j++)
            {
                if (arr[0, j] > max)
                {
                    index = j;
                    max = arr[0, j];
                }
            }
            return index;
        }
    }

    static class ShuffleExt
    {
        // Shameless copy pasta from http://stackoverflow.com/questions/5383498/shuffle-rearrange-randomly-a-liststring
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            Random rnd = new Random();
            while (n > 1)
            {
                int k = (rnd.Next(0, n) % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}
