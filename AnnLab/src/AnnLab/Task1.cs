using AnnLab.Range;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnnLab
{
    public class Task1
    {

        public static Matrix<double> InitWeights(int N, Matrix<int>[] patterns)
        {
            /* Naive version, does not take in consideration that W is symmetric
             * This is at least 2 times slower than the code below it
            Matrix<double> W = new Matrix<double>(N, N);
            foreach (var mat in patterns)
            {
                Matrix<double> dmat = mat; // This skips an implicit conversion
                W = W.InPlace() + dmat * ~dmat;
            }
            W.InPlace().SetDiagonal(0);
            return W;
            */
            Matrix<int> W = new Matrix<int>(N, N);
            foreach (var mat in patterns)
            {
                for (int i = 0; i < N; i++)
                    for (int j = i + 1; j < N; j++)
                        W[i, j] += mat[i, 0] * mat[j, 0];
            }
            for (int i = 0; i < N; i++)
                for (int j = i + 1; j < N; j++)
                    W[j, i] = W[i, j];
            Matrix<double> Wd = W;
            return Wd.InPlace() / N;
        }

        static void GenPatterns(Matrix<int>[] patterns, int N)
        {
            Random rng = new Random();
            for (int p = 0; p < patterns.Length; p++)
            {
                Matrix<int> mat = new Matrix<int>(N);
                for (int i = 0; i < N; i++)
                    mat[i, 0] = -1 + 2 * rng.Next(2);
                patterns[p] = mat;
            }
        }

        public class JobDescription
        {
            public int N { get; set; }
            public int p { get; set; }
        }

        public class JobResult
        {
            public JobDescription Job { get; set; }
            public double AverageErrorRate { get; set; }
        }

        public static JobResult RunJob(JobDescription job)
        {
            Matrix<int>[] patterns = new Matrix<int>[job.p];
            GenPatterns(patterns, job.N);

            Matrix<double> W = InitWeights(job.N, patterns);

            var res = new JobResult
            {
                Job = job,
                AverageErrorRate = Enumerable.Range(0, job.p).Select(i => patterns[i]).Average(state =>
                {
                    // Count number of errors after one update
                    int errors = (W * state).Col(0).Zip(state.Col(0), (si, sj) => (si >= 0) == (sj == 1)).Count(b => !b);
                    return errors / (double)job.N;
                })
            };
            Interlocked.Increment(ref JobsCompleted);
            return res;
        }

        public static void Progress()
        {
            DateTimeOffset start = DateTimeOffset.UtcNow;
            Console.WriteLine("Running " + TotalJobs + " jobs");
            Thread.Sleep(1000);
            TimeSpan estimatedTotal;
            while(JobsCompleted < TotalJobs)
            {
                double done = JobsCompleted / (double)TotalJobs;
                var elapsed = DateTimeOffset.UtcNow - start;
                estimatedTotal = new TimeSpan((long)((DateTimeOffset.UtcNow - start).Ticks / done));
                Console.WriteLine(
                    $"[{DateTime.Now.ToString("HH:mm:ss")}] " +
                    $"({done.ToString("0.%").PadLeft(4)}) " + 
                    $"{JobsCompleted.ToString().PadLeft(TotalJobs.ToString().Length)} out of {TotalJobs} jobs completed, " +
                    $"time left: {(estimatedTotal - elapsed).ToString("hh\\:mm\\:ss")}");
                Thread.Sleep(Math.Min((int)(estimatedTotal.TotalMilliseconds / 50), 5 * 60 * 1000));
            }
        }

        public static int JobsCompleted = 0, TotalJobs;

        public static void Run(IEnumerable<string> args)
        {
            if (args.Count() != 1)
            {
                Console.WriteLine("Usage: AnnLab <nRuns>");
                return;
            }
            int nRuns = int.Parse(args.First());
            if (nRuns < 0)
            {
                Console.WriteLine("Usage: AnnLab <nRuns>");
                return;
            }
            int[] possibleN = { 100, 200 };
            int[] possibleP = { 10, 20, 30, 40, 50, 75, 100, 150, 200 };

            var jobs = possibleN.SelectMany(N => possibleP.Select(p => new JobDescription { N = N, p = p }));
            var runs = Enumerable.Repeat(jobs, nRuns).SelectMany(x => x);
            TotalJobs = runs.Count();

            Thread progress = new Thread(Progress);
            progress.Start();

            var results = runs.AsParallel().Select(RunJob).ToList();
            var resByJob = results
                .GroupBy(res => Tuple.Create(res.Job.p, res.Job.N));
            var avgByJob = resByJob.Select(byJob => Tuple.Create(byJob.Key.Item1 / (double)byJob.Key.Item2,
                                                                 byJob.Key,
                                                                 byJob.Average(res => res.AverageErrorRate)))
                .OrderBy(byJob => byJob.Item1);


            string filename = "result_" + nRuns + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
            Console.WriteLine("Writing to " + filename + "...");
            using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.CreateNew), Encoding.ASCII))
            {
                foreach (var avg in avgByJob)
                {
                    sw.WriteLine(avg.Item1.ToString(CultureInfo.InvariantCulture) + " " + avg.Item3.ToString(CultureInfo.InvariantCulture) + " " + avg.Item2.Item1 + " " + avg.Item2.Item2);
                }
            }
            Console.WriteLine("Done!");
        }

    }
}
