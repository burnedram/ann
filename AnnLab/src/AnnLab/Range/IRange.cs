using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnLab.Range
{
    public interface IRange
    {
        /// <summary>
        /// Return an ordered (ascending) array of indices
        /// </summary>
        /// <param name="length"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        IEnumerable<int> Range(int length, int min = 0);
        bool Test(int i, int length, int min = 0);
    }
    
    /// <summary>
    /// A continous and deterministic range
    /// </summary>
    public interface IContinousRange
    {
        int Min(int length, int min = 0);
        int Max(int length, int min = 0);
    }

    public static class Ranges
    {
        public static RangeAllImpl All { get; } = new RangeAllImpl();
        public static RangePredicateImpl Where(Predicate<int> pred)
        {
            return new RangePredicateImpl(pred);
        }
        public static RangeArrayImpl In(params int[] arr)
        {
            return new RangeArrayImpl(arr);
        }
        public static RangeArrayImpl In(IEnumerable<int> arr)
        {
            return new RangeArrayImpl(arr);
        }
        public static RangeFromImpl From(int from)
        {
            return new RangeFromImpl(from);
        }
        public static RangeToImpl To(int to)
        {
            return new RangeToImpl(to);
        }
        public static RangeSimpleImpl At(int at)
        {
            return new RangeSimpleImpl(at);
        }
        /// <summary>
        /// Inclusive
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static RangeBetweenImpl Between(int from, int to)
        {
            return new RangeBetweenImpl(from, to);
        }
    }

    public class RangeSimpleImpl : IRange, IContinousRange
    {

        public int Index { get; }

        public RangeSimpleImpl(int i)
        {
            Index = i;
        }

        public IEnumerable<int> Range(int length, int min = 0)
        {
            yield return Index;
        }

        public bool Test(int i, int length, int min = 0)
        {
            return i >= min && i < min + length && i == Index;
        }

        public int Min(int length, int min = 0)
        {
            if (Index < min || Index >= min + length)
                throw new InvalidOperationException();
            return Index;
        }

        public int Max(int length, int min = 0)
        {
            if (Index < min || Index >= min + length)
                throw new InvalidOperationException();
            return Index;
        }

        public static implicit operator RangeSimpleImpl(int i)
        {
            return new RangeSimpleImpl(i);
        }

        public static explicit operator int(RangeSimpleImpl i)
        {
            return i.Index;
        }

        public static implicit operator RangeWrapper(RangeSimpleImpl range)
        {
            return new RangeWrapper(range);
        }

    }

    public class RangeWrapper : IRange
    {

        public IRange WrappedRange { get; }

        public RangeWrapper(IRange range)
        {
            while (range is RangeWrapper)
                range = ((RangeWrapper)range).WrappedRange;
            WrappedRange = range;
        }

        public IEnumerable<int> Range(int length, int min = 0)
        {
            return WrappedRange.Range(length, min);
        }

        public bool Test(int i, int length, int min = 0)
        {
            return WrappedRange.Test(i, length, min);
        }

        public static implicit operator RangeWrapper(int i)
        {
            return new RangeSimpleImpl(i);
        }

        public static implicit operator RangeWrapper(Predicate<int> pred)
        {
            return new RangePredicateImpl(pred);
        }

        public static implicit operator RangeWrapper(int[] arr)
        {
            return new RangeArrayImpl(arr);
        }

    }

    public class RangeAllImpl : IRange, IContinousRange
    {

        public IEnumerable<int> Range(int length, int min = 0)
        {
            for (int i = min; i < min + length; i++)
                yield return min + i;
        }

        public bool Test(int i, int length, int min = 0)
        {
            return i >= min && i < min + length;
        }

        public int Min(int length, int min = 0)
        {
            return min;
        }

        public int Max(int length, int min = 0)
        {
            return min + length - 1;
        }

        public static implicit operator RangeWrapper(RangeAllImpl range)
        {
            return new RangeWrapper(range);
        }

    }

    public class RangePredicateImpl : IRange
    {

        public Predicate<int> PredicateFunc { get; }

        public RangePredicateImpl(Predicate<int> pred)
        {
            PredicateFunc = pred;
        }

        public IEnumerable<int> Range(int length, int min = 0)
        {
            for (int i = min; i < min + length; i++)
                if (PredicateFunc(i))
                    yield return i;
        }

        public bool Test(int i, int length, int min = 0)
        {
            return i >= min && i < min + length && PredicateFunc(i);
        }

        public static implicit operator RangePredicateImpl(Predicate<int> pred)
        {
            return new RangePredicateImpl(pred);
        }

        public static implicit operator RangeWrapper(RangePredicateImpl range)
        {
            return new RangeWrapper(range);
        }

    }

    public class RangeArrayImpl : IRange
    {
        public IOrderedEnumerable<int> Array { get; }

        public RangeArrayImpl(IEnumerable<int> arr)
        {
            Array = arr.OrderBy(x => x);
        }

        public IEnumerable<int> Range(int length, int min = 0)
        {
            IEnumerator<int> enumerator = Array.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;
            for (int i = min; i < min + length; i++)
            {
                while (enumerator.Current < i)
                    if (!enumerator.MoveNext())
                        yield break;
                if (enumerator.Current == i)
                    yield return i;
            }
        }

        public bool Test(int i, int length, int min = 0)
        {
            return i >= min && i < min + length && Array.Any(x => x == i);
        }

        public static implicit operator RangeArrayImpl(int[] arr)
        {
            return new RangeArrayImpl(arr);
        }

        public static implicit operator RangeWrapper(RangeArrayImpl range)
        {
            return new RangeWrapper(range);
        }

    }

    public class RangeFromImpl : IRange, IContinousRange
    {
        public int From { get; }

        public RangeFromImpl(int from)
        {
            From = from;
        }

        public IEnumerable<int> Range(int length, int min = 0)
        {
            for (int i = Math.Max(From, min); i < min + length; i++)
                yield return i;
        }

        public bool Test(int i, int length, int min = 0)
        {
            return i >= min && i < min + length && i >= From;
        }

        public int Min(int length, int min = 0)
        {
            if (From >= min + length)
                throw new InvalidOperationException();
            return Math.Max(min, From);
        }

        public int Max(int length, int min = 0)
        {
            if (From >= min + length)
                throw new InvalidOperationException();
            return min + length - 1;
        }

        public static implicit operator RangeWrapper(RangeFromImpl range)
        {
            return new RangeWrapper(range);
        }

    }

    public class RangeToImpl : IRange, IContinousRange
    {
        public int To { get; }

        public RangeToImpl(int to)
        {
            To = to;
        }

        public IEnumerable<int> Range(int length, int min = 0)
        {
            for (int i = min; i < min + length && i <= To; i++)
                yield return i;
        }

        public bool Test(int i, int length, int min = 0)
        {
            return i >= min && i < min + length && i <= To;
        }

        public int Min(int length, int min = 0)
        {
            if (To < min)
                throw new InvalidOperationException();
            return min;
        }

        public int Max(int length, int min = 0)
        {
            if (To < min)
                throw new InvalidOperationException();
            return Math.Min(To, min + length - 1);
        }

        public static implicit operator RangeWrapper(RangeToImpl range)
        {
            return new RangeWrapper(range);
        }

    }

    public class RangeBetweenImpl : IRange, IContinousRange
    {
        public int From { get; }
        public int To { get; }

        public RangeBetweenImpl(int from, int to)
        {
            if (to < from)
                throw new ArgumentException("to < from");
            From = from;
            To = to;
        }

        public IEnumerable<int> Range(int length, int min = 0)
        {
            for (int i = Math.Max(From, min); i < min + length && i <= To; i++)
                yield return i;
        }

        public bool Test(int i, int length, int min = 0)
        {
            return i >= min && i < min + length && i >= From && i <= To;
        }

        public int Min(int length, int min = 0)
        {
            if (From >= min + length)
                throw new InvalidOperationException();
            return Math.Max(min, From);
        }

        public int Max(int length, int min = 0)
        {
            if (From >= min + length || To < min)
                throw new InvalidOperationException();
            return Math.Min(To, min + length - 1);
        }

        public static implicit operator RangeWrapper(RangeBetweenImpl range)
        {
            return new RangeWrapper(range);
        }

    }

}
