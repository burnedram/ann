using AnnLab.Range;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static void Run(IEnumerable<string> args)
        {
            double q = 1;
            Matrix<double> W = Task1.InitWeights(160, DIGITS);
            Matrix<double> aoe = new Matrix<double>(10, 10);
            Matrix<int> state = DIGITS[0];
            Distort(q, state);
            Matrix<int> prevState = new Matrix<int>(state);
            var indices = Enumerable.Range(0, 160).ToList();

            Console.WriteLine(state.Reshape(16, 10));
            do
            {
                indices.Shuffle();
                foreach (var i in indices)
                {
                    state[i, 0] = Enumerable.Range(0, 160).Sum(j => W[i, j] * state[j, 0]) >= 0 ? 1 : -1;
                }
                Matrix<int> swapState = state;
                state = prevState;
                prevState = swapState;
            } while (state != prevState);
            Console.WriteLine(state.Reshape(16, 10));
        }

    }

    public static class ShuffleExt
    {
        // Shameless copy pasta from http://stackoverflow.com/questions/5383498/shuffle-rearrange-randomly-a-liststring
        public static void Shuffle<T>(this IList<T> list)
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
        }
    }
}
