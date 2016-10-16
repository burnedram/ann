using AnnLab.Range;
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
    public class Task3
    {

        static double FastCov(IReadOnlyCollection<double> orderParams)
        {
            if (orderParams.Count <= 1)
                return 0;
            double mean = orderParams.Average();
            return orderParams.Select(x => x - mean).Sum(x => x * x) / (orderParams.Count - 1);
        }

        static Matrix<double> Cov(Matrix<double> m)
        {
            Matrix<double> m2 = new Matrix<double>(m);
            for (int j = 0; j < m.Cols; j++)
            {
                double mean = m.Col(j).Average();
                for (int i = 0; i < m.Rows; i++)
                    m2[i, j] -= mean;
            }
            return (~m2 * m2).InPlace() / m.Rows;
        }

        static double StochasticG(double b, double Beta)
        {
            return 1 / (1 + Math.Exp(-2 * Beta * b));
        }

        static double OrderParam(Matrix<int> pattern, Matrix<int> state)
        {
            Matrix<int> sum = ~pattern * state;
            int N = pattern.Rows;
            return sum[0, 0] / (double)N;
        }

        static int OrderParamBufferSize = 100;
        static int SkipTransients = 100; // gteq OrderParamBufferSize
        static double CovCovThreshold = 1E-5;
        static int MaxIters = 1000;

        class JobDescription
        {
            public int p;
            public int N;
            public double Beta;
        }

        class JobResult
        {
            public JobDescription Job;
            public double m;
        }
        
        static JobResult RunJob(JobDescription job)
        {
            Matrix<int>[] patterns =  new Matrix<int>[job.p];
            Task1.GenPatterns(patterns, job.N);
            Matrix<double> W = Task1.InitWeights(job.N, patterns);

            Matrix<int> state = new Matrix<int>(patterns[0]);
            Matrix<double> b = W * state;
            Queue<double> orderParams = new Queue<double>(OrderParamBufferSize);
            Queue<double> covQueue = new Queue<double>(OrderParamBufferSize);
            Random rng = new Random();

            foreach (var i in Enumerable.Range(0, SkipTransients))
            {
                state[Ranges.All, 0] = b.Col(0).Select(bi => rng.NextDouble() < StochasticG(bi, job.Beta) ? 1 : -1).AsColumn();
                b = W * state;
                if (orderParams.Count == OrderParamBufferSize)
                {
                    orderParams.Dequeue();
                    covQueue.Dequeue();
                }
                orderParams.Enqueue(OrderParam(patterns[0], state));
                covQueue.Enqueue(FastCov(orderParams));
            }

            int iters = 0;
            double covcov = 0;
            while (iters < MaxIters && (covcov = FastCov(covQueue)) > CovCovThreshold)
            {
                iters++;
                state[Ranges.All, 0] = b.Col(0).Select(bi => rng.NextDouble() < StochasticG(bi, job.Beta) ? 1 : -1).AsColumn();
                b = W * state;

                if (orderParams.Count == OrderParamBufferSize)
                {
                    orderParams.Dequeue();
                    covQueue.Dequeue();
                }
                orderParams.Enqueue(OrderParam(patterns[0], state));
                covQueue.Enqueue(FastCov(orderParams));
            }
            double m = orderParams.Average();
            if (iters == MaxIters)
                Console.WriteLine("WARNING: MaxIters reached with N=" + job.N + ", p=" + job.p + ", Beta=" + job.Beta + ", covcov=" + covcov.ToString(CultureInfo.InvariantCulture));

            Interlocked.Increment(ref JobsCompleted);
            return new JobResult
            {
                Job = job,
                m = m
            };
        }

        public static int JobsCompleted = 0, TotalJobs;

        public static void Run(IEnumerable<string> args)
        {
#if DEBUG
            args = new List<string> { "2" };
#endif
            if (!args.Any())
            {
                Console.WriteLine("Usage: task3 <nRuns> <aSteps>");
                return;
            }
            if (args.Count() > 2)
            {
                Console.WriteLine("Too many arguments");
                return;
            }
            int nRuns = int.Parse(args.First());
            if (nRuns < 0)
            {
                Console.WriteLine("nRuns can't be negative");
                return;
            }
            int aSteps = 50;
            if (args.Count() == 2)
            {
                aSteps = int.Parse(args.Last());
                if (aSteps < 1)
                {
                    Console.WriteLine("aSteps can't be zero or negative");
                    return;
                }
            }

            var alphas = Enumerable.Range(1, aSteps).Select(step => step * 1d/aSteps);
            var Ns = new int[] { 50, 100, 250, 500 };
            double Beta = 2;

            foreach (var N in Ns)
            {
                if (N % aSteps != 0)
                {
                    Console.WriteLine("Invalid aSteps, " + N + " is not divisable with " + aSteps);
                    return;
                }
            }

            var jobs = alphas.SelectMany(a => Ns.Select(N => new JobDescription { p = Math.Max(1, (int)(a * N)), N = N, Beta = Beta }));
            var runs = Enumerable.Repeat(jobs, nRuns).SelectMany(x => x);
            TotalJobs = runs.Count();

            Thread progress = new Thread(() => Progress.ProgressFunc(ref TotalJobs, ref JobsCompleted));
            progress.Start();

            var results = runs.AsParallel().Select(RunJob).ToList();
            var byJob = results.GroupBy(res => Tuple.Create(res.Job.p, res.Job.N));
            var avgByN = byJob.GroupBy(job => job.Key.Item2, job => Tuple.Create(job.Key.Item1 / (double)job.Key.Item2, job.Average(res => res.m)));

            foreach (var N in avgByN)
            {
                string filename = "task3_" + nRuns + "_" + aSteps + "_" + N.Key + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
                Console.WriteLine("Writing to " + filename + "...");
                using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.CreateNew), Encoding.ASCII))
                {
                    foreach(var avg in N.OrderBy(avg => avg.Item1))
                    {
                        sw.WriteLine(avg.Item1.ToString(CultureInfo.InvariantCulture) + " " + avg.Item2.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            Console.WriteLine("Done!");
        }
    }
}
