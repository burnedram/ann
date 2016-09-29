using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnLab
{
    public class Task4b
    {
        static double FuncGPrim(double b, double Beta)
        {
            double g = Math.Tanh(Beta * b);
            return Beta * (1 - g * g);
        }

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
            Matrix<double>[] Ws, biases, neurons;
            Task4a.InitWeightsBiasesNeurons(Ns,
                out Ws, -0.2, 0.2,
                out biases, -1, 1,
                out neurons);

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
                int layer = rng.Next(0, job.TrainingData.Length);
                Task4a.FeedPattern(Ws, biases, neurons, job.TrainingData[layer], job.Beta);

                //bias += learingRate * deltaError
            }
        }
    }
}
