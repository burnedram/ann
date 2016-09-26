using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnnLab
{
    public static class Progress
    {
        public static void ProgressFunc(ref int TotalJobs, ref int JobsCompleted)
        {
            DateTimeOffset start = DateTimeOffset.UtcNow;
            Console.WriteLine("Running " + TotalJobs + " jobs");
            Thread.Sleep(1000);
            TimeSpan estimatedTotal;
            while (JobsCompleted < TotalJobs)
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
    }
}
