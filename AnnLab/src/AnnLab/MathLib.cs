using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AnnLab
{
    public interface IMathLib<TVal>
    {
        TVal Add(TVal left, TVal right);
        TVal Sub(TVal left, TVal right);
        TVal Neg(TVal val);
        TVal Mul(TVal left, TVal right);
        TVal Div(TVal left, TVal right);
        TVal MatMul(TVal[,] A, TVal[,] B, int len, int i, int j);

        TVal Cast(int val);
        bool Eq(TVal left, TVal right);
    }

    public static class MathLib
    {
        public static IMathLib<TVal> Get<TVal>()
        {
            if (typeof(int).GetTypeInfo().IsAssignableFrom(typeof(TVal)))
                return (IMathLib<TVal>) new IntMathLib();
            if (typeof(double).GetTypeInfo().IsAssignableFrom(typeof(TVal)))
                return (IMathLib<TVal>) new DoubleMathLib();
            throw new NotImplementedException("no math lib for type " + typeof(TVal));
        }
    }

    public class IntMathLib : IMathLib<int>
    {
        public int Add(int left, int right)
        {
            return left + right;
        }

        public int Sub(int left, int right)
        {
            return left - right;
        }

        public int Neg(int val)
        {
            return -val;
        }

        public int Mul(int left, int right)
        {
            return left * right;
        }
        
        public int Div(int left, int right)
        {
            return left / right;
        }

        public int MatMul(int[,] A, int[,] B, int len, int i, int j)
        {
            int val = 0;
            for (int k = 0; k < len; k++)
                val += A[i, k] * B[k, j];
            return val;
        }

        public int Cast(int val)
        {
            return val;
        }
        
        public bool Eq(int left, int right)
        {
            return left == right;
        }
    }

    public class DoubleMathLib : IMathLib<double>
    {
        public double Add(double left, double right)
        {
            return left + right;
        }

        public double Sub(double left, double right)
        {
            return left - right;
        }

        public double Neg(double val)
        {
            return -val;
        }

        public double Mul(double left, double right)
        {
            return left * right;
        }

        public double Div(double left, double right)
        {
            return left / right;
        }

        public double MatMul(double[,] A, double[,] B, int len, int i, int j)
        {
            double val = 0;
            for (int k = 0; k < len; k++)
                val += A[i, k] * B[k, j];
            return val;
        }

        public double Cast(int val)
        {
            return val;
        }

        public bool Eq(double left, double right)
        {
            return left == right;
        }
    }
}
