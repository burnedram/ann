using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnnLab.Lab1
{
    public class Task4b
    {

        class JobDescription : Task4a.JobDescription
        {
            public int HiddenNeurons;
        }

        class JobResult : Task4a.JobResult
        {
            public int HiddenNeurons;
        }

        static bool Dump;

        static int JobsCompleted = 0, TotalJobs;

        public static void Run(IEnumerable<string> args)
        {
            int nRuns;
            if (!Task4a.ParseArgs("task4b", ref args, out nRuns, out Dump))
                return;

            Matrix<double>[][] dataAll;
            int[][][] classesAll;
            Task4a.ReadNormalizeSplit(out classesAll, out dataAll, args.First(), args.Skip(1).First());
            int[][] trainingClasses = classesAll[0], validationClasses = classesAll[1];
            Matrix<double>[] trainingData = dataAll[0], validationData = dataAll[1];

            double Beta = 0.5;
            double learningRate = 0.01;
            int[] hiddenNeurons = { 0, 2, 4, 8, 16, 32 };

            var jobs = hiddenNeurons.Select(hn => new JobDescription
            {
                HiddenNeurons = hn,
                Beta = Beta,
                LearningRate = learningRate,
                TrainingData = trainingData,
                TrainingClasses = trainingClasses,
                ValidationData = validationData,
                ValidationClasses = validationClasses
            });
            var runs = Enumerable.Repeat(jobs, nRuns).SelectMany(x => x);
            TotalJobs = runs.Count();

            Thread progress = new Thread(() => Progress.ProgressFunc(ref TotalJobs, ref JobsCompleted));
            progress.Start();

            var results = runs.AsParallel().Select(RunJob).ToList();
            if (!Dump)
            {
                var byNeurons = results.GroupBy(res => res.HiddenNeurons).OrderBy(n => n.Key);
                string filename = "task4b_" + nRuns + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
                Console.WriteLine("Writing to " + filename + "...");
                using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.CreateNew), Encoding.ASCII))
                {
                    foreach (var byNeuron in byNeurons)
                    {
                        double trainingErrorRate = byNeuron.Average(res => res.Training);
                        double validationErrorRate = byNeuron.Average(res => res.Validation);

                        sw.WriteLine(byNeuron.Key + " " +
                            trainingErrorRate.ToString(CultureInfo.InvariantCulture) + " " + 
                            validationErrorRate.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            Console.WriteLine("Done!");
        }

        static JobResult RunJob(JobDescription job)
        {
            if (job.HiddenNeurons == 0)
                job.Ns = new int[] { 2, 1 };
            else
                job.Ns = new int[] { 2, job.HiddenNeurons, 1 };

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
                nn.Train(job.TrainingData[iPattern], job.TrainingClasses[iPattern], job.LearningRate);
            }

            if (Dump)
            {
                Task4a.WriteDump("task4b_dump_" + job.HiddenNeurons + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), nn);
                Interlocked.Increment(ref JobsCompleted);
                return null;
            }

            double trainingErrorRate = Task4a.ErrorRate(job.TrainingData, job.TrainingClasses, nn);
            double validationErrorRate = Task4a.ErrorRate(job.ValidationData, job.ValidationClasses, nn);
            Interlocked.Increment(ref JobsCompleted);
            return new JobResult
            {
                HiddenNeurons = job.HiddenNeurons,
                Training = trainingErrorRate,
                Validation = validationErrorRate
            };
        }
    }
}
