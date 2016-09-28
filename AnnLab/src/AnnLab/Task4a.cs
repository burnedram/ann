using AnnLab.Range;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnnLab
{
    public class Task4a
    {

        static void NormalizeMeanAndVarInPlace(params Matrix<double>[] datas)
        {
            double meanX = datas.SelectMany(data => data.Col(0)).Average(),
                meanY = datas.SelectMany(data => data.Col(1)).Average();
            foreach (var data in datas)
            {
                for (int i = 0; i < data.Rows; i++)
                {
                    data[i, 0] -= meanX;
                    data[i, 1] -= meanY;
                }
            }
            int N = datas.Sum(data => data.Rows) - 1;
            double varX = Math.Sqrt(datas.SelectMany(data => data.Col(0)).Sum(x => x * x) / N),
                varY = Math.Sqrt(datas.SelectMany(data => data.Col(1)).Sum(y => y * y) / N);
            foreach (var data in datas)
            {
                for (int i = 0; i < data.Rows; i++)
                {
                    data[i, 0] /= varX;
                    data[i, 1] /= varY;
                }
            }
        }

        static double ActivationFuncG(double b, double Beta)
        {
            return Math.Tanh(Beta * b);
        }

        static void FuncB(Matrix<double> output, Matrix<double> W, Matrix<double> bias, Matrix<double> pattern)
        {
            for (int j = 0; j < output.Cols; j++)
                output[0, j] = W.Col(j).Zip(pattern.Row(0), (wij, squigglyj) => wij * squigglyj).Sum() + bias[0, j];
        }

        static void FuncB(Matrix<double> output, Matrix<double> W, Matrix<double> bias, Matrix<double> pattern, int j)
        {
            output[0, j] = W.Col(j).Zip(pattern.Row(0), (wij, squigglyj) => wij * squigglyj).Sum() + bias[0, j];
        }

        static double ErrorRate(Matrix<double>[] data, int[,] classes, Matrix<double>[] Ws, Matrix<double>[] biases, Matrix<double>[] neurons, double Beta)
        {
            return Enumerable.Range(0, data.Length).Average(layer =>
            {
                int clazz = classes[layer, 0];
                neurons[0][Ranges.All, Ranges.All] = data[layer];
                FuncB(neurons[1], Ws[0], biases[0], neurons[0]);
                neurons[1].InPlace().UnaryOp(bi => ActivationFuncG(bi, Beta));
                return Math.Abs(clazz - (neurons[1][0, 0] >= 0 ? 1 : -1));
            }) / 2;
        }

        class JobDescription
        {
            public double Beta;
            public double LearningRate;
            public int[] Ns;
            public Matrix<double>[] TrainingData, ValidationData;
            public int[,] TrainingClasses, ValidationClasses;
        }

        class JobResult
        {
            public double Training, Validation;
        }

        static JobResult RunJob(JobDescription job)
        {
            Random rng = new Random();
            int nLayers = job.Ns.Length - 1;
            Matrix<double>[] Ws = new Matrix<double>[nLayers];
            Matrix<double>[] biases = new Matrix<double>[nLayers];
            Matrix<double>[] neurons = new Matrix<double>[nLayers + 1];
            neurons[0] = new Matrix<double>(1, job.Ns[0]);
            for (int layer = 0; layer < nLayers; layer++)
            {
                int rows = job.Ns[layer], cols = job.Ns[layer + 1];
                var W = Ws[layer] = new Matrix<double>(rows, cols);
                var bias = biases[layer] = new Matrix<double>(1, cols);
                neurons[layer + 1] = new Matrix<int>(1, cols);
                for (int j = 0; j < cols; j++)
                {
                    bias[0, j] = rng.NextDouble() * (1 - -1) + -1;
                    for (int i = 0; i < rows; i++)
                        W[i, j] = rng.NextDouble() * (0.2 - -0.2) + -0.2;
                }
            }

            int iters = 200000;
            for (int iter = 0; iter < iters; iter++)
            {
                int layer = rng.Next(0, job.TrainingData.Length);
                neurons[0][Ranges.All, Ranges.All] = job.TrainingData[layer];

                // Calculate output
                FuncB(neurons[1], Ws[0], biases[0], neurons[0]);
                neurons[1].InPlace().UnaryOp(bi => ActivationFuncG(bi, job.Beta));

                for (int i = 0; i < job.Ns[0]; i++)
                    for (int j = 0; j < job.Ns[1]; j++)
                    {
                        double error = job.TrainingClasses[layer, j] - neurons[1][0, j];
                        Ws[0][i, j] += error * job.LearningRate * neurons[0][0, i];

                        // Update output
                        FuncB(neurons[1], Ws[0], biases[0], neurons[0], j);
                        neurons[1][0, j] = ActivationFuncG(neurons[1][0, j], job.Beta);
                    }
                for (int j = 0; j < job.Ns[1]; j++)
                {
                    double error = job.TrainingClasses[layer, j] - neurons[1][0, j];
                    biases[0][0, j] += error * job.LearningRate;

                    // Update output
                    FuncB(neurons[1], Ws[0], biases[0], neurons[0], j);
                    neurons[1][0, j] = ActivationFuncG(neurons[1][0, j], job.Beta);
                }
            }

            //Console.WriteLine("a = [" + Ws[0][0, 0].ToString(CultureInfo.InvariantCulture) + "; " + Ws[0][1, 0].ToString(CultureInfo.InvariantCulture) + "];");
            //Console.WriteLine("b = " + biases[0][0, 0].ToString(CultureInfo.InvariantCulture) + ";");
            //Environment.Exit(0);

            double trainingErrorRate = ErrorRate(job.TrainingData, job.TrainingClasses, Ws, biases, neurons, job.Beta);
            double validationErrorRate = ErrorRate(job.ValidationData, job.ValidationClasses, Ws, biases, neurons, job.Beta);
            Interlocked.Increment(ref JobsCompleted);
            return new JobResult
            {
                Training = trainingErrorRate,
                Validation = validationErrorRate
            };
        }

        static int JobsCompleted = 0, TotalJobs;

        public static void Run(IEnumerable<string> args)
        {
            if (args.Count() == 0)
            {
                args = new List<string> { "C:\\ann\\train_data_2016.txt", "C:\\ann\\valid_data_2016.txt" };
            }
            else if (args.Count() != 2)
            {
                Console.WriteLine("Usage: task4a <train_data> <valid_data>");
                return;
            }

            int[,] trainingClasses, validationClasses;
            Matrix<double> trainingDataAll = ReadData(args.First(), out trainingClasses);
            Matrix<double> validationDataAll = ReadData(args.Last(), out validationClasses);
            NormalizeMeanAndVarInPlace(trainingDataAll, validationDataAll);
            Matrix<double>[] trainingData = SplitData(trainingDataAll);
            Matrix<double>[] validationData = SplitData(validationDataAll);

            int nRuns = 1000;
            int[] Ns = new int[] { 2, 1 };
            double Beta = 0.5;
            double learningRate = 0.01;

            var job = new JobDescription
            {
                Beta = Beta,
                LearningRate = learningRate,
                Ns = Ns,
                TrainingData = trainingData,
                TrainingClasses = trainingClasses,
                ValidationData = validationData,
                ValidationClasses = validationClasses
            };
            var runs = Enumerable.Repeat(job, nRuns);
            TotalJobs = runs.Count();

            Thread progress = new Thread(() => Progress.ProgressFunc(ref TotalJobs, ref JobsCompleted));
            progress.Start();

            var results = runs.AsParallel().Select(RunJob).ToList();
            double trainingErrorRate = results.Average(res => res.Training);
            double validationErrorRate = results.Average(res => res.Validation);

            string filename = "task4a_" + nRuns + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
            Console.WriteLine("Writing to " + filename + "...");
            using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.CreateNew), Encoding.ASCII))
            {
                sw.WriteLine("Average training error: " + trainingErrorRate.ToString(CultureInfo.InvariantCulture));
                sw.WriteLine("Average validation error: " + validationErrorRate.ToString(CultureInfo.InvariantCulture));
            }
            Console.WriteLine("Done!");
        }

        static Matrix<double>[] SplitData(Matrix<double> data)
        {
            Matrix<double>[] m = new Matrix<double>[data.Rows];
            for (int i = 0; i < data.Rows; i++)
            {
                m[i] = new Matrix<double>(1, 2);
                m[i][0, 0] = data[i, 0];
                m[i][0, 1] = data[i, 1];
            }
            return m;
        }

        static Matrix<double> ReadData(string filePath, out int[,] classes)
        {
            int N = CountLines(filePath);
            Matrix<double> m = new Matrix<double>(N, N);
            classes = new int[N, 1];
            using (StreamReader sr = new StreamReader(new FileStream(filePath, FileMode.Open)))
            {
                string line;
                for (int i = 0; (line = sr.ReadLine()) != null; i++)
                {
                    var vals = line.Split(null);
                    m[i, 0] = double.Parse(vals[0], CultureInfo.InvariantCulture);
                    m[i, 1] = double.Parse(vals[1], CultureInfo.InvariantCulture);
                    classes[i, 0] = int.Parse(vals[2]);
                }
            }
            return m;
        }

        static int CountLines(string filePath)
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
}
