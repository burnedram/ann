using AnnLab.Range;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

        public static void Run(IEnumerable<string> args)
        {
#if DEBUG
            args = new List<string> { "C:\\ann\\train_data_2016.txt", "C:\\ann\\valid_data_2016.txt" };
#endif
            if (args.Count() != 2)
            {
                Console.WriteLine("Usage: task4a <train_data> <valid_data>");
                return;
            }

            int[] trainingClasses, validationClasses;
            Matrix<double> trainingDataAll = ReadData(args.First(), out trainingClasses);
            Matrix<double> validationDataAll = ReadData(args.Last(), out validationClasses);
            NormalizeMeanAndVarInPlace(trainingDataAll, validationDataAll);
            Matrix<double>[] trainingData = SplitData(trainingDataAll);
            Matrix<double>[] validationData = SplitData(validationDataAll);
            Random rng = new Random();

            int[] Ns = new int[] { 2, 1 };
            int nLayers = Ns.Length - 1;
            double Beta = 0.5;
            double learningRate = 0.01;
            Matrix<double>[] Ws = new Matrix<double>[nLayers];
            Matrix<double>[] biases = new Matrix<double>[nLayers];
            Matrix<double>[] neurons = new Matrix<double>[nLayers + 1];
            neurons[0] = new Matrix<double>(1, 2);
            for (int layer = 0; layer < nLayers; layer++)
            {
                int rows = Ns[layer], cols = Ns[layer + 1];
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
                int layer = rng.Next(0, trainingData.Length);
                neurons[0][Ranges.All, Ranges.All] = trainingData[layer];
                int[] clazz = new int[] { trainingClasses[layer] };

                // Calculate output
                FuncB(neurons[1], Ws[0], biases[0], neurons[0]);
                neurons[1].InPlace().UnaryOp(bi => ActivationFuncG(bi, Beta));

                for (int i = 0; i < Ns[0]; i++)
                    for (int j = 0; j < Ns[1]; j++)
                    {
                        double error = clazz[j] - neurons[1][0, j];
                        Ws[0][i, j] += error * learningRate * neurons[0][0, i];

                        // Update output
                        FuncB(neurons[1], Ws[0], biases[0], neurons[0], j);
                        neurons[1][0, j] = ActivationFuncG(neurons[1][0, j], Beta);
                    }
                for (int j = 0; j < Ns[1]; j++)
                {
                    double error = clazz[j] - neurons[1][0, j];
                    biases[0][0, j] += error * learningRate;

                    // Update output
                    FuncB(neurons[1], Ws[0], biases[0], neurons[0], j);
                    neurons[1][0, j] = ActivationFuncG(neurons[1][0, j], Beta);
                }
            }

            Console.WriteLine();
            Console.WriteLine("a = [" + Ws[0][0, 0].ToString(CultureInfo.InvariantCulture) + "; " + Ws[0][1, 0].ToString(CultureInfo.InvariantCulture) + "];");
            Console.WriteLine("b = " + biases[0][0, 0].ToString(CultureInfo.InvariantCulture) + ";");

            double trainingErrorRate = Enumerable.Range(0, trainingData.Length).Average(layer =>
            {
                int clazz = trainingClasses[layer];
                neurons[0][Ranges.All, Ranges.All] = trainingData[layer];
                FuncB(neurons[1], Ws[0], biases[0], neurons[0]);
                neurons[1].InPlace().UnaryOp(bi => ActivationFuncG(bi, Beta));
                return Math.Abs(clazz - (neurons[1][0, 0] >= 0 ? 1 : -1));
            }) / 2;
            double validationErrorRate = Enumerable.Range(0, validationData.Length).Average(layer =>
            {
                int clazz = validationClasses[layer];
                neurons[0][Ranges.All, Ranges.All] = validationData[layer];
                FuncB(neurons[1], Ws[0], biases[0], neurons[0]);
                neurons[1].InPlace().UnaryOp(bi => ActivationFuncG(bi, Beta));
                return Math.Abs(clazz - (neurons[1][0, 0] >= 0 ? 1 : -1));
            }) / 2;
            Console.WriteLine();
            Console.WriteLine("Training error: " + trainingErrorRate.ToString("0.00%").PadLeft(7));
            Console.WriteLine("Validation error: " + validationErrorRate.ToString("0.00%").PadLeft(7));
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

        static Matrix<double> ReadData(string filePath, out int[] classes)
        {
            int N = CountLines(filePath);
            Matrix<double> m = new Matrix<double>(N, N);
            classes = new int[N];
            using (StreamReader sr = new StreamReader(new FileStream(filePath, FileMode.Open)))
            {
                string line;
                for (int i = 0; (line = sr.ReadLine()) != null; i++)
                {
                    var vals = line.Split(null);
                    m[i, 0] = double.Parse(vals[0], CultureInfo.InvariantCulture);
                    m[i, 1] = double.Parse(vals[1], CultureInfo.InvariantCulture);
                    classes[i] = int.Parse(vals[2]);
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
