using AnnLab.Range;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AnnLab
{
    public class NeuralNetwork
    {

        public static double FuncG(double b, double Beta)
        {
            return Math.Tanh(Beta * b);
        }

        public static double FuncGPrim(double b, double Beta)
        {
            double g = Math.Tanh(Beta * b);
            return Beta * (1 - g * g);
        }

        public double Beta { get; }
        public int[] Ns { get; }
        public int nTotalLayers { get; }
        public ReadOnlyCollection<Matrix<double>> Ws { get; }
        public ReadOnlyCollection<Matrix<double>> biases { get; }
        public ReadOnlyCollection<Matrix<double>> localFields { get; }
        public ReadOnlyCollection<Matrix<double>> neurons { get; }

        public int InputIndex { get { return 0; } }
        public int OutputIndex { get; }
        public Matrix<double> Input { get; }
        public Matrix<double> Output { get; }
        public Matrix<double> OutputLocalField { get; }

        #region INIT

        public NeuralNetwork(double _Beta, int[] _Ns)
        {
            if (_Ns.Length < 2)
                throw new ArgumentException("Must have atleast an input and output layer");
            Beta = _Beta;
            Ns = _Ns;
            nTotalLayers = Ns.Length;
            OutputIndex = nTotalLayers - 1;

            var _Ws = new List<Matrix<double>>(nTotalLayers - 1);
            var _biases = new List<Matrix<double>>(nTotalLayers - 1);
            var _localFields = new List<Matrix<double>>(nTotalLayers - 1);
            var _neurons = new List<Matrix<double>>(nTotalLayers);

            _neurons.Add(new Matrix<double>(1, Ns[0]));
            Input = _neurons[0];
            for (int layer = 0; layer < OutputIndex; layer++)
            {
                int rows = Ns[layer], cols = Ns[layer + 1];
                _Ws.Add(new Matrix<double>(rows, cols));
                _biases.Add(new Matrix<double>(1, cols));
                _localFields.Add(new Matrix<int>(1, cols));
                _neurons.Add(new Matrix<int>(1, cols));
            }
            Output = _neurons[OutputIndex];
            OutputLocalField = _localFields[OutputIndex - 1];

            Ws = _Ws.AsReadOnly();
            biases = _biases.AsReadOnly();
            localFields = _localFields.AsReadOnly();
            neurons = _neurons.AsReadOnly();
        }

        public void RandomizeWeights(double wlow, double whigh)
        {
            Random rng = new Random();
            for (int layer = 0; layer < nTotalLayers - 1; layer++)
            {
                int rows = Ns[layer], cols = Ns[layer + 1];
                for (int j = 0; j < cols; j++)
                {
                    for (int i = 0; i < rows; i++)
                        Ws[layer][i, j] = rng.NextDouble() * (whigh - wlow) + wlow;
                }
            }
        }

        public void RandomizeBiases(double blow, double bhigh)
        {
            Random rng = new Random();
            for (int layer = 0; layer < nTotalLayers - 1; layer++)
            {
                int cols = Ns[layer + 1];
                for (int j = 0; j < cols; j++)
                {
                    biases[layer][0, j] = rng.NextDouble() * (bhigh - blow) + blow;
                }
            }
        }

        #endregion

        #region FEED

        public void FeedPattern(Matrix<double> pattern)
        {
            if (Input.Rows != pattern.Rows || Input.Cols != pattern.Cols)
                throw new ArgumentException("dimensions does not agree");
            Input[Ranges.All, Ranges.All] = pattern;
            PropagateInput();
        }

        public void PropagateInput()
        {
            for (int layer = 1; layer <= OutputIndex; layer++)
                UpdateLayer(layer);
        }

        public void UpdateLayer(int layer)
        {
            CalculateB(layer);
            CalculateGB(layer);
        }

        void CalculateB(int layer)
        {
            Matrix<double> output = localFields[layer - 1];
            Matrix<double> pattern = neurons[layer - 1];
            Matrix<double> W = Ws[layer - 1];
            Matrix<double> bias = biases[layer - 1];
            for (int j = 0; j < output.Cols; j++)
                output[0, j] = W.Col(j).Zip(pattern.Row(0), (wij, squigglyj) => wij * squigglyj).Sum() + bias[0, j];
        }

        void CalculateGB(int layer)
        {
            neurons[layer][0, Ranges.All] = localFields[layer - 1][0, Ranges.All].Select(bi => FuncG(bi, Beta)).AsRow();
        }

        #endregion

    }
}
