using AnnLab.Range;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections;

namespace AnnLab
{

    public class Matrix<TVal> : IEnumerable<IEnumerable<TVal>>, IEnumerable
    {

        /// <summary>
        /// If ModifyInPlace is set then the next applicable operation on this matrix will be done in place.
        /// After the operation is done ModifyInPlace will be set to false again.
        /// </summary>
        public bool ModifyInPlace { get; private set; } = false;

        private readonly IMathLib<TVal> _math;

        /* IEnumerable<IEnumerable<TVal>>
         *    ^^^^^       ^^^^^     ^^^^
         *    outer       inner     vals

         * Example: Outer has length 2, inner has length 3,
         *          or there are 2 rows and 3 columns
         * 0 1 2
         * 3 4 5
         */

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
        public IEnumerable<TVal> Row(int i)
        {
            for (int j = 0; j < Cols; j++)
                yield return _matrix[i, j];
        }
        public IEnumerable<TVal> Col(int j)
        {
            for (int i = 0; i < Rows; i++)
                yield return _matrix[i, j];
        }
        public IEnumerable<TVal> Row(int i, RangeWrapper jRange)
        {
            foreach (int j in jRange.Range(Cols))
                yield return _matrix[i, j];
        }
        public IEnumerable<TVal> Col(int j, RangeWrapper iRange)
        {
            foreach (int i in iRange.Range(Rows))
                yield return _matrix[i, j];
        }
        public IEnumerable<IEnumerable<TVal>> this[RangeWrapper iRangeW, RangeWrapper jRangeW]
        {
            get
            {
                foreach (int i in iRangeW.Range(Rows))
                    yield return Row(i, jRangeW);
            }
            set
            {
                if (value is Matrix<TVal> && iRangeW.WrappedRange is IContinousRange && jRangeW.WrappedRange is IContinousRange)
                {
                    Matrix<TVal> B = (Matrix<TVal>)value;
                    IContinousRange iRangeC = (IContinousRange)iRangeW.WrappedRange;
                    IContinousRange jRangeC = (IContinousRange)jRangeW.WrappedRange;
                    int iMin = iRangeC.Min(Rows), iMax = iRangeC.Max(Rows);
                    int jMin = jRangeC.Min(Cols), jMax = jRangeC.Max(Cols);
                    //iMax = Math.Min(iMax, iMin + B.Rows - 1);
                    if (B.Rows > iMax - iMin + 1 || B.Cols > jMax - jMin + 1)
                        throw new InvalidOperationException("Source is larger than destination, [" + (iMax - iMin + 1) + ", " + (jMax - jMin + 1) + "] = [" + B.Rows + ", " + B.Cols + "]");

                    int nRows = Math.Min(iMax - iMin + 1, B.Rows);
                    if (Cols == B.Cols && jMin == 0 && jMax == Cols - 1)
                    {
                        // We can copy the entire array in one go, since both src and dst spans entire rows (and has the same length)
                        Array.Copy(B._matrix, 0, _matrix, Cols * iMin, Cols * nRows);
                        return;
                    }

                    int rowSize = Math.Min(jMax - jMin + 1, B.Cols); // Don't copy more than what exists in a src row
                    // Copy row by row
                    for (int i = 0; i < nRows; i++)
                        Array.Copy(B._matrix, B.Cols * i, _matrix, Cols * (iMin + i) + jMin, rowSize);
                    return;
                }

                IEnumerator<IEnumerable<TVal>> iEnumerator = value.GetEnumerator();
                var iRange = iRangeW.Range(Rows);
                var jRange = jRangeW.Range(Cols);
                foreach (int i in iRange)
                {
                    if (!iEnumerator.MoveNext())
                        return; // Not enough values in src
                    IEnumerator<TVal> jEnumerator = iEnumerator.Current.GetEnumerator();
                    foreach (int j in jRange)
                    {
                        if (!jEnumerator.MoveNext())
                            break; // Not enough values in src row
                        _matrix[i, j] = jEnumerator.Current;
                    }
                    if (jEnumerator.MoveNext())
                        throw new InvalidOperationException("Source is larger than destination, [" + iRange.Count() + ", " + jRange.Count() + "] = [" + value.Count() + ", " + iEnumerator.Current.Count() + "]");
                }
                if (iEnumerator.MoveNext())
                    throw new InvalidOperationException("Source is larger than destination, [" + iRange.Count() + ", " + jRange.Count() + "] = [" + value.Count() + ", ?]");
            }
        }
        public IEnumerable<TVal> this[int i, RangeWrapper jRange]
        {
            get
            {
                return Row(i, jRange);
            }
            set
            {
                this[(RangeWrapper)i, jRange] = Enumerable.Repeat(value, 1);
            }
        }
        public IEnumerable<TVal> this[RangeWrapper iRange, int j]
        {
            get
            {
                return Col(j, iRange);
            }
            set
            {
                this[iRange, (RangeWrapper)j] = Enumerable.Repeat(value, 1);
            }
        }
        public IEnumerable<IEnumerable<TVal>> this[RangeWrapper iRange]
        {
            get
            {
                return this[iRange, Ranges.All];
            }
            set
            {
                this[iRange, Ranges.All] = value;
            }
        }
        public IEnumerable<TVal> this[int i]
        {
            get
            {
                return Row(i);
            }
            set
            {
                this[i, Ranges.All] = value;
            }
        }

        public int Rows { get; }
        public int Cols { get; }

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
            Rows = rows;
            Cols = cols;
            _matrix = new TVal[rows, cols];
        }

        /// <summary>
        /// Constructs a copy of A
        /// </summary>
        /// <param name="A"></param>
        public Matrix(Matrix<TVal> A)
        {
            _math = A._math;
            Rows = A.Rows;
            Cols = A.Cols;
            _matrix = new TVal[Rows, Cols];
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
            return SetEach((i, j) => op(A[i, j]));
        }

        public Matrix<TVal> SetEach(Func<int, int, TVal> op)
        {
            Matrix<TVal> A = this;
            using (var inplace = new InPlaceGuard(A))
            {
                Matrix<TVal> B = inplace.Matrix;

                for (int i = 0; i < A.Rows; i++)
                    for (int j = 0; j < A.Cols; j++)
                        B[i, j] = op(i, j);

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

        public IEnumerator<IEnumerable<TVal>> GetEnumerator()
        {
            for (int i = 0; i < Rows; i++)
                yield return Row(i);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
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
