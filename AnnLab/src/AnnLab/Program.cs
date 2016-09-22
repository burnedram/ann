using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnLab
{
    public class Program
    {
        static Random rng = new Random();

        static void PrintMatrix<T>(Matrix<T> matrix, TextWriter tw = null)
        {
            if (tw == null)
                tw = Console.Out;
            int maxLength = matrix.Flatten().Select(t => t.ToString().Length).Max();
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Cols; j++)
                    tw.Write(matrix[i, j].ToString().PadLeft(maxLength, ' ') + " ");
                tw.WriteLine();
            }
        }

        static void InitWeights(Matrix<double> W, Matrix<int>[] patterns)
        {
            W.InPlace().SetAll(0);
            foreach (var mat in patterns)
            {
                Matrix<double> dmat = mat; // this skips an unnecessary implicit conversion
                W = W.InPlace() + dmat * ~dmat;
            }
            W = W.InPlace() / patterns.Length;
            W.InPlace().SetDiagonal(0);
        }

        static void GenPatterns(Matrix<int>[] patterns, int N)
        {
            for (int p = 0; p < patterns.Length; p++)
            {
                Matrix<int> mat = new Matrix<int>(N);
                for (int i = 0; i < N; i++)
                    mat[i] = -1 + 2 * rng.Next(2);
                patterns[p] = mat;
            }
        }

        public static void Main(string[] args)
        {
            int N = 10;
            int p = 10;

            Matrix<int>[] patterns = new Matrix<int>[p];
            GenPatterns(patterns, N);

            Matrix<double> W = new Matrix<double>(N, N);
            InitWeights(W, patterns);
            PrintMatrix(W);
        }
    }
}
