using AnnLab.Range;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnLab
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

        public static void Run(IEnumerable<string> args)
        {
            int N = 500;
            int p = 100;
            double Beta = 2;
            int orderParamBufferSize = 100;
            int skipTransients = 100;
            if (skipTransients < orderParamBufferSize)
                skipTransients = orderParamBufferSize;
            double CovThreshold = 1E-6;

            Matrix<int>[] patterns =  new Matrix<int>[p];
            Task1.GenPatterns(patterns, N);
            Matrix<double> W = Task1.InitWeights(N, patterns);

            Matrix<int> state = new Matrix<int>(patterns[0]);
            Matrix<double> b = W * state;
            Queue<double> orderParams = new Queue<double>(orderParamBufferSize);
            Queue<double> covQueue = new Queue<double>(orderParamBufferSize);
            Random rng = new Random();

            foreach (var i in Enumerable.Range(0, skipTransients))
            {
                state[Ranges.All, 0] = b.Col(0).Select(bi => rng.NextDouble() < StochasticG(bi, Beta) ? 1 : -1).AsColumn();
                b = W * state;
                if (orderParams.Count == orderParamBufferSize)
                {
                    orderParams.Dequeue();
                    covQueue.Dequeue();
                }
                orderParams.Enqueue(OrderParam(patterns[0], state));
                covQueue.Enqueue(FastCov(orderParams));
            }

            double covcov;
            while ((covcov = FastCov(covQueue)) > CovThreshold)
            {
                state[Ranges.All, 0] = b.Col(0).Select(bi => rng.NextDouble() < StochasticG(bi, Beta) ? 1 : -1).AsColumn();
                b = W * state;

                if (orderParams.Count == orderParamBufferSize)
                {
                    orderParams.Dequeue();
                    covQueue.Dequeue();
                }
                orderParams.Enqueue(OrderParam(patterns[0], state));
                covQueue.Enqueue(FastCov(orderParams));
            }

            double m = orderParams.Average();
        }
    }
}
