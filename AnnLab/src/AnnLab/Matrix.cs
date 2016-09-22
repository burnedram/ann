using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AnnLab
{

    public class Matrix<TVal>
    {

        /// <summary>
        /// If ModifyInPlace is set then the next applicable operation on this matrix will be done in place.
        /// After the operation is done ModifyInPlace will be set to false again.
        /// </summary>
        public bool ModifyInPlace { get; private set; } = false;

        private readonly IMathLib<TVal> _math;

        private readonly TVal[,] _matrix;
        public TVal this[int i, int j]
        {
            get
            {
                return _matrix[i, j];
            }
            set
            {
                _matrix[i, j] = value;
            }
        }
        public TVal this[int i]
        {
            get
            {
                return _matrix[i, 0];
            }
            set
            {
                _matrix[i, 0] = value;
            }
        }

        private readonly int _rows, _cols;
        public int Rows
        {
            get
            {
                return _rows;
            }
        }
        public int Cols
        {
            get
            {
                return _cols;
            }
        }

        /// <summary>
        /// Constructs an N*1 matrix, i.e. a row vector
        /// </summary>
        /// <param name="N"></param>
        public Matrix(int N) : this(N, 1)
        {
        }

        /// <summary>
        /// Constructs an N*M matrix
        /// </summary>
        /// <param name="rows">N</param>
        /// <param name="cols">M</param>
        public Matrix(int rows, int cols)
        {
            if (rows <= 0 || cols <= 0)
                throw new ArgumentOutOfRangeException("rows/cols must be larger than 0");
            _math = MathLib.Get<TVal>();
            _rows = rows;
            _cols = cols;
            _matrix = new TVal[_rows, _cols];
        }

        /// <summary>
        /// Constructs a copy of A
        /// </summary>
        /// <param name="A"></param>
        public Matrix(Matrix<TVal> A)
        {
            _math = A._math;
            _rows = A._rows;
            _cols = A._cols;
            _matrix = new TVal[_rows, _cols];
            Array.Copy(A._matrix, _matrix, _matrix.Length);
        }

        public IEnumerable<TVal> Flatten()
        {
            return _matrix.Cast<TVal>();
        }

        public Matrix<TVal> InPlace()
        {
            ModifyInPlace = true;
            return this;
        }

        public Matrix<TVal> OutOfPlace()
        {
            ModifyInPlace = false;
            return this;
        }

        public Matrix<TVal> SetAll(TVal val)
        {
            return UnaryOp(delegate { return val; });
        }

        public Matrix<TVal> SetDiagonal(TVal val)
        {
            Matrix<TVal> A = this;
            using (var inplace = new InPlaceGuard(A))
            {
                Matrix<TVal> B = inplace.Matrix;

                for (int i = 0; i < Math.Min(A.Rows, A.Cols); i++)
                    B[i, i] = val;

                return B;
            }
        }

        public Matrix<TVal> UnaryOp(Func<TVal, TVal> op)
        {
            Matrix<TVal> A = this;
            using (var inplace = new InPlaceGuard(A))
            {
                Matrix<TVal> B = inplace.Matrix;

                for (int i = 0; i < A.Rows; i++)
                    for (int j = 0; j < A.Cols; j++)
                        B[i, j] = op(A[i, j]);

                return B;
            }
        }

        public Matrix<TVal> BinaryOp(TVal val, Func<TVal, TVal, TVal> op)
        {
            Matrix<TVal> A = this;
            using (var inplace = new InPlaceGuard(A))
            {
                Matrix<TVal> B = inplace.Matrix;

                for (int i = 0; i < A.Rows; i++)
                    for (int j = 0; j < A.Cols; j++)
                        B[i, j] = op(A[i, j], val);

                return B;
            }
        }

        public Matrix<TVal> BinaryOp(Matrix<TVal> B, Func<TVal, TVal, TVal> op)
        {
            Matrix<TVal> A = this;
            if (A.Cols != B.Cols || A.Rows != B.Rows)
                throw new InvalidOperationException("dimensions does not agree");
            using (var inplace = new InPlaceGuard(A))
            {
                Matrix<TVal> C = inplace.Matrix;

                for (int i = 0; i < A.Rows; i++)
                    for (int j = 0; j < A.Cols; j++)
                        C[i, j] = op(A[i, j], B[i, j]);

                return C;
            }
        }

        public static Matrix<TVal> operator +(Matrix<TVal> A, Matrix<TVal> B)
        {
            return A.BinaryOp(B, A._math.Add);
        }

        public static Matrix<TVal> operator -(Matrix<TVal> A, Matrix<TVal> B)
        {
            return A.BinaryOp(B, A._math.Sub);
        }

        public static Matrix<TVal> operator -(Matrix<TVal> A)
        {
            return A.UnaryOp(A._math.Neg);
        }

        public static Matrix<TVal> operator *(Matrix<TVal> A, Matrix<TVal> B)
        {
            if (A.ModifyInPlace)
                throw new InvalidOperationException("did not expect ModifyInPlace to be true");
            if (A.Cols != B.Rows)
                throw new InvalidOperationException("inner dimensions does not agree");
            Matrix<TVal> C = new Matrix<TVal>(A.Rows, B.Cols);

            for (int i = 0; i < C.Rows; i++)
                for (int j = 0; j < C.Cols; j++)
                    C[i, j] = A._math.MatMul(A._matrix, B._matrix, A.Cols, i, j);

            return C;
        }

        public static Matrix<TVal> operator *(Matrix<TVal> A, TVal val)
        {
            return A.BinaryOp(val, A._math.Mul);
        }

        public static Matrix<TVal> operator /(Matrix<TVal> A, TVal val)
        {
            return A.BinaryOp(val, A._math.Div);
        }

        public static Matrix<TVal> operator ~(Matrix<TVal> A)
        {
            if (A.Rows == A.Cols && A.ModifyInPlace)
            {
                for (int i = 0; i < A.Rows; i++)
                    for (int j = i+1; j < A.Cols; j++)
                    {
                        TVal val = A[i, j];
                        A[i, j] = A[j, i];
                        A[j, i] = val;
                    }

                A.ModifyInPlace = false;
                return A;
            }
            else
            {
                Matrix<TVal> B = new Matrix<TVal>(A.Cols, A.Rows);

                for (int i = 0; i < B.Rows; i++)
                    for (int j = 0; j < B.Cols; j++)
                        B[i, j] = A[j, i];

                return B;
            }
        }

        public static implicit operator Matrix<TVal>(Matrix<int> A)
        {
            if (A.ModifyInPlace)
                throw new InvalidOperationException("did not expect ModifyInPlace to be true");
            if (!typeof(double).GetTypeInfo().IsAssignableFrom(typeof(TVal)))
                throw new InvalidOperationException("unable to implicitly convert " + typeof(TVal) + " to double");

            Matrix<TVal> B = new Matrix<TVal>(A.Rows, A.Cols);

            for (int i = 0; i < B.Rows; i++)
                for (int j = 0; j < B.Cols; j++)
                    B[i, j] = B._math.Cast(A[i, j]);

            return B;
        }

        private class InPlaceGuard : IDisposable
        {
            private readonly Matrix<TVal> _matrix;
            public Matrix<TVal> Matrix { get; }

            public InPlaceGuard(Matrix<TVal> A, bool fullCopy = false)
            {
                if (A.ModifyInPlace)
                    Matrix = A;
                else if (fullCopy)
                    Matrix = new Matrix<TVal>(A);
                else
                    Matrix = new Matrix<TVal>(A.Rows, A.Cols);
                _matrix = A;
            }

            public void Dispose()
            {
                _matrix.ModifyInPlace = false;
            }
        }
    }
}
