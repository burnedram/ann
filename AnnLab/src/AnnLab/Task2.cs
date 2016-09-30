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
    public class Task2
    {
        public static readonly Matrix<int>[] DIGITS = new Matrix<int>[5];

        #region DIGITS

        static Task2() {
            DIGITS[0] = new Matrix<int>(16, 10);
            DIGITS[0][Ranges.All, Ranges.All] = new int[][] {
                new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1,  1,  1,  1,  1,  1,  1, -1, -1},
                new int[] {-1,  1,  1,  1, -1, -1,  1,  1,  1, -1},
                new int[] {-1,  1,  1,  1, -1, -1,  1,  1,  1, -1},
                new int[] {-1,  1,  1,  1, -1, -1,  1,  1,  1, -1},
                new int[] {-1,  1,  1,  1, -1, -1,  1,  1,  1, -1},
                new int[] {-1,  1,  1,  1, -1, -1,  1,  1,  1, -1},
                new int[] {-1,  1,  1,  1, -1, -1,  1,  1,  1, -1},
                new int[] {-1,  1,  1,  1, -1, -1,  1,  1,  1, -1},
                new int[] {-1,  1,  1,  1, -1, -1,  1,  1,  1, -1},
                new int[] {-1,  1,  1,  1, -1, -1,  1,  1,  1, -1},
                new int[] {-1,  1,  1,  1, -1, -1,  1,  1,  1, -1},
                new int[] {-1, -1,  1,  1,  1,  1,  1,  1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            };

            DIGITS[1] = new Matrix<int>(16, 10);
            DIGITS[1][Ranges.All, Ranges.All] = new int[][]
            {
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
                new int[] {-1, -1, -1,  1,  1,  1,  1, -1, -1, -1},
            };

            DIGITS[2] = new Matrix<int>(16, 10);
            DIGITS[2][Ranges.All, Ranges.All] = new int[][]
            {
                new int[] { 1,  1,  1,  1,  1,  1,  1,  1, -1, -1},
                new int[] { 1,  1,  1,  1,  1,  1,  1,  1, -1, -1},
                new int[] {-1, -1, -1, -1, -1,  1,  1,  1, -1, -1},
                new int[] {-1, -1, -1, -1, -1,  1,  1,  1, -1, -1},
                new int[] {-1, -1, -1, -1, -1,  1,  1,  1, -1, -1},
                new int[] {-1, -1, -1, -1, -1,  1,  1,  1, -1, -1},
                new int[] {-1, -1, -1, -1, -1,  1,  1,  1, -1, -1},
                new int[] { 1,  1,  1,  1,  1,  1,  1,  1, -1, -1},
                new int[] { 1,  1,  1,  1,  1,  1,  1,  1, -1, -1},
                new int[] { 1,  1, -1, -1, -1, -1, -1, -1, -1, -1},
                new int[] { 1,  1, -1, -1, -1, -1, -1, -1, -1, -1},
                new int[] { 1,  1, -1, -1, -1, -1, -1, -1, -1, -1},
                new int[] { 1,  1, -1, -1, -1, -1, -1, -1, -1, -1},
                new int[] { 1,  1, -1, -1, -1, -1, -1, -1, -1, -1},
                new int[] { 1,  1,  1,  1,  1,  1,  1,  1, -1, -1},
                new int[] { 1,  1,  1,  1,  1,  1,  1,  1, -1, -1},
            };

            DIGITS[3] = new Matrix<int>(16, 10);
            DIGITS[3][Ranges.All, Ranges.All] = new int[][]
            {
                new int[] {-1, -1,  1,  1,  1,  1,  1,  1, -1, -1},
                new int[] {-1, -1,  1,  1,  1,  1,  1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1,  1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1,  1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1,  1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1,  1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1,  1,  1,  1, -1},
                new int[] {-1, -1, -1, -1,  1,  1,  1,  1, -1, -1},
                new int[] {-1, -1, -1, -1,  1,  1,  1,  1, -1, -1},
                new int[] {-1, -1, -1, -1, -1, -1,  1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1,  1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1,  1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1,  1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1,  1,  1,  1, -1},
                new int[] {-1, -1,  1,  1,  1,  1,  1,  1,  1, -1},
                new int[] {-1, -1,  1,  1,  1,  1,  1,  1, -1, -1},
            };

            DIGITS[4] = new Matrix<int>(16, 10);
            DIGITS[4][Ranges.All, Ranges.All] = new int[][]
            {
                new int[] {-1,  1,  1, -1, -1, -1, -1,  1,  1, -1},
                new int[] {-1,  1,  1, -1, -1, -1, -1,  1,  1, -1},
                new int[] {-1,  1,  1, -1, -1, -1, -1,  1,  1, -1},
                new int[] {-1,  1,  1, -1, -1, -1, -1,  1,  1, -1},
                new int[] {-1,  1,  1, -1, -1, -1, -1,  1,  1, -1},
                new int[] {-1,  1,  1, -1, -1, -1, -1,  1,  1, -1},
                new int[] {-1,  1,  1, -1, -1, -1, -1,  1,  1, -1},
                new int[] {-1,  1,  1,  1,  1,  1,  1,  1,  1, -1},
                new int[] {-1,  1,  1,  1,  1,  1,  1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1, -1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1, -1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1, -1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1, -1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1, -1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1, -1,  1,  1, -1},
                new int[] {-1, -1, -1, -1, -1, -1, -1,  1,  1, -1},
            };

            DIGITS = DIGITS.Select(dig => dig.Reshape(160, 1)).ToArray();
        }

        #endregion

        public static void Distort(double q, Matrix<int> state)
        {
            var indices = Enumerable.Range(0, state.Rows).ToList();
            indices.Shuffle();
            foreach (int i in indices.Take((int)Math.Round(160 * q)))
                state[i, 0] *= -1;
        }

        class JobDescription
        {
            public double q;
            public Matrix<double> W;
        }

        class JobResult
        {
            public double q;
            public IEnumerable<bool> Corrects;
            public double AverageCorrectRate;
        }

        static JobResult RunJob(JobDescription job)
        {
            var indices = Enumerable.Range(0, 160).ToList();
            Matrix<int> state = new Matrix<int>(160), prevState = new Matrix<int>(160);
            var corrects = Enumerable.Range(0, DIGITS.Length).Select(dig =>
            {
                state[Ranges.All, Ranges.All] = DIGITS[dig];
                Distort(job.q, state);
                do
                {
                    prevState[Ranges.All, Ranges.All] = state;
                    indices.Shuffle();
                    foreach (var i in indices)
                    {
                        state[i, 0] = Enumerable.Range(0, 160).Sum(j => job.W[i, j] * state[j, 0]) >= 0 ? 1 : -1;
                    }
                } while (state != prevState);
                return state == DIGITS[dig];
            }).ToList();
            var res = new JobResult
            {
                q = job.q,
                Corrects = corrects,
                AverageCorrectRate = corrects.Count(b => b) / (double)DIGITS.Length
            };
            Interlocked.Increment(ref JobsCompleted);
            return res;
        }

        public static int JobsCompleted = 0, TotalJobs;

        static Dictionary<double, double[]> AggregateThrowsStackOverflowExceptionWorkaround(IEnumerable<IGrouping<double, JobResult>> resByJob, int nRuns)
        {
            var avgByJobByDigit = new Dictionary<double, double[]>();
            foreach (var byJob in resByJob)
            {
                avgByJobByDigit[byJob.Key] = new double[DIGITS.Length];
                foreach (var corrects in byJob.Select(res => res.Corrects))
                {
                    int digit = 0;
                    foreach (var correct in corrects)
                    {
                        if (correct)
                            avgByJobByDigit[byJob.Key][digit]++;
                        digit++;
                    }
                }
                for (int digit = 0; digit < DIGITS.Length; digit++)
                    avgByJobByDigit[byJob.Key][digit] /= nRuns;
            }
            return avgByJobByDigit;
        }

        public static void Run(IEnumerable<string> args)
        {
#if DEBUG
            args = new List<string> { "60000" };
#endif
            if (args.Count() < 1)
            {
                Console.WriteLine("Must specify nRuns");
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
            int qSteps = 100;
            if (args.Count() == 2)
            {
                qSteps = int.Parse(args.Last());
                if (qSteps < 1)
                {
                    Console.WriteLine("qSteps can't be zero or negative");
                    return;
                }
            }
            var qs = Enumerable.Range(0, qSteps).Select(step => step * 1d/(qSteps - 1));
            Matrix<double> W = Task1.InitWeights(160, DIGITS);

            var jobs = qs.Select(q => new JobDescription { q = q, W = W });
            var runs = Enumerable.Repeat(jobs, nRuns).SelectMany(x => x);
            TotalJobs = runs.Count();

            Thread progress = new Thread(() => Progress.ProgressFunc(ref TotalJobs, ref JobsCompleted));
            progress.Start();

            var results = runs.AsParallel().Select(RunJob).ToList();
            var resByJob = results.GroupBy(res => res.q);
            var avgByJobByDigit = AggregateThrowsStackOverflowExceptionWorkaround(resByJob, nRuns);
            var avgByJob = resByJob.Select(byJob => Tuple.Create(byJob.Key,
                     avgByJobByDigit[byJob.Key],
                     /* Throws StackOverflowException when nRuns is somewhat large, 
                      * avgByJobByDigit is a "manual" implementation that works
                     byJob.Select(res => res.Corrects)
                          .Aggregate(Enumerable.Repeat(0, DIGITS.Length), (acc, next) => acc.Zip(next, (a, b) => b ? a + 1 : a))
                          .Select(avg => avg / (double)nRuns),*/
                     byJob.Average(res => res.AverageCorrectRate)))
                .OrderBy(avg => avg.Item1);

            string filename = "task2_" + nRuns + "_" + qSteps + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
            Console.WriteLine("Writing to " + filename + "...");
            using (StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.CreateNew), Encoding.ASCII))
            {
                foreach (var avg in avgByJob)
                {
                    sw.Write(avg.Item1.ToString(CultureInfo.InvariantCulture));
                    foreach(var dig in avg.Item2)
                        sw.Write(" " + dig.ToString(CultureInfo.InvariantCulture));
                    sw.WriteLine(" " + avg.Item3.ToString(CultureInfo.InvariantCulture));
                }
            }
            Console.WriteLine("Done!");
        }

    }

    public static class ShuffleExt
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
