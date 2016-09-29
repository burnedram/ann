using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AnnLab
{
    public class Task4b
    {

        public static void Run(IEnumerable<string> args)
        {
            int nRuns;
            if (!Task4a.ParseArgs(ref args, out nRuns))
                return;

            Matrix<double>[][] dataAll;
            int[][,] classesAll;
            Task4a.ReadNormalizeSplit(out classesAll, out dataAll, args.First(), args.Last());
            int[,] trainingClasses = classesAll[0], validationClasses = classesAll[1];
            Matrix<double>[] trainingData = dataAll[0], validationData = dataAll[1];

            int hiddenNeurons = 10;
            double Beta = 0.5;
            double learningRate = 0.5;
            int[] Ns;
            if (hiddenNeurons == 0)
                Ns = new int[] { 2, 1 };
            else
                Ns = new int[] { 2, hiddenNeurons, 1 };
            int nLayers = Ns.Length;

            var job = new Task4a.JobDescription
            {
                Beta = Beta,
                LearningRate = learningRate,
                Ns = Ns,
                TrainingData = trainingData,
                TrainingClasses = trainingClasses,
                ValidationData = validationData,
                ValidationClasses = validationClasses
            };

            Random rng = new Random();
            NeuralNetwork nn = new NeuralNetwork(job.Beta, job.Ns);
            nn.RandomizeWeights(-0.2, 0.2);
            nn.RandomizeBiases(-1, 1);
            Matrix<double>[] deltas = new Matrix<double>[nn.nTotalLayers - 1];
            for (int layer = 1; layer <= nn.OutputIndex; layer++)
                deltas[layer - 1] = new Matrix<double>(nn.neurons[layer].Rows, nn.neurons[layer].Cols);

            int iters = 200000;
            for (int iter = 0; iter < iters; iter++)
            {
                int iPattern = rng.Next(0, job.TrainingData.Length);
                nn.FeedPattern(job.TrainingData[iPattern]);

                // Calculate output layer error
                for (int j = 0; j < nn.Output.Cols; j++)
                {
                    double error = job.TrainingClasses[iPattern, j] - nn.Output[0, j];
                    error *= NeuralNetwork.FuncGPrim(nn.OutputPreG[0, j], nn.Beta);
                    deltas[nn.OutputIndex - 1][0, j] = error;
                }

                // Calculate preceding layer errors
                for (int layer = nn.OutputIndex - 1; layer >= 1; layer--)
                    for (int j = 0; j < nn.Ws[layer - 1].Cols; j++)
                    {
                        double error = nn.Ws[layer].Row(j).Zip(deltas[layer].Row(0), (wij, deltai) => wij * deltai).Sum();
                        error *= NeuralNetwork.FuncGPrim(nn.neuronsPreG[layer][0, j], nn.Beta);
                        deltas[layer - 1][0, j] = error;
                    }

                // Update
                for (int layer = 0; layer < nn.nTotalLayers - 1; layer++)
                    for (int j = 0; j < nn.Ws[layer].Cols; j++)
                    {
                        for (int i = 0; i < nn.Ws[layer].Rows; i++)
                            nn.Ws[layer][i, j] += job.LearningRate * deltas[layer][0, j] * nn.neurons[layer][0, i];
                        nn.biases[layer][0, j] += job.LearningRate * deltas[layer][0, j];
                    }

                if (iter % 1000 == 0)
                    Console.WriteLine(Task4a.ErrorRate(trainingData, trainingClasses, nn).ToString("0.00%").PadLeft(7) + " " + iter.ToString().PadLeft(iters.ToString().Length));
            }

            Console.WriteLine(Task4a.ErrorRate(trainingData, trainingClasses, nn).ToString("0.00%").PadLeft(7) + " " + iters);

            for (int j = 0; j < nn.Ws[0].Cols; j++)
            {
                Console.WriteLine();
                Console.WriteLine("a = [" + nn.Ws[0][0, j].ToString(CultureInfo.InvariantCulture) + "; " + nn.Ws[0][1, j].ToString(CultureInfo.InvariantCulture) + "];");
                Console.WriteLine("b = " + nn.biases[0][0, j].ToString(CultureInfo.InvariantCulture) + ";");
            }
        }
    }
}
