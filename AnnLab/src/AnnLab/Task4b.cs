using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnLab
{
    public class Task4b
    {

        public static void Run(IEnumerable<string> args)
        {
            int nRuns;
            if (Task4a.ParseArgs(ref args, out nRuns))
                return;

            Matrix<double>[][] dataAll;
            int[][,] classesAll;
            Task4a.ReadNormalizeSplit(out classesAll, out dataAll, args.First(), args.Last());
            int[,] trainingClasses = classesAll[0], validationClasses = classesAll[1];
            Matrix<double>[] trainingData = dataAll[0], validationData = dataAll[1];

            int hiddenNeurons = 2;
            double Beta = 0.5;
            double learningRate = 0.5;
            int[] Ns = { 2, hiddenNeurons, 1 };
            int nLayers = Ns.Length;
            NeuralNetwork nn = new NeuralNetwork(Beta, Ns);
            nn.RandomizeWeights(-0.2, 0.2);
            nn.RandomizeBiases(-1, 1);

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
            int iters = 200000;
            for (int iter = 0; iter < iters; iter++)
            {
                int iPattern = rng.Next(0, job.TrainingData.Length);
                nn.FeedPattern(job.TrainingData[iPattern]);

                //bias += learingRate * deltaError
            }
        }
    }
}
