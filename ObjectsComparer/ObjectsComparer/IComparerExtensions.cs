using System;
using System.Collections.Generic;

namespace ObjectsComparer
{
    /// <summary>
    /// Extends interface <see cref="IComparer"/> and <see cref="IComparer{T}"/> with overloaded operations CalculateDifferences and Compare that accept <see cref="ComparisonContext"/> parameter.
    /// </summary>
    public static class IComparerExtensions
    {
        /// <summary>
        /// Extends <see cref="IComparer.CalculateDifferences(Type, object, object)"/> operation with <see cref="ComparisonContext"/> parameter.
        /// </summary>
        public static IEnumerable<Difference> CalculateDifferences(this IComparer comparer, Type type, object obj1, object obj2, ComparisonContext comparisonContext)
        {
            if (comparer is IContextableComparer contextableComparer)
            {
                return contextableComparer.CalculateDifferences(type, obj1, obj2, comparisonContext);
            }

            return comparer.CalculateDifferences(type, obj1, obj2);
        }

        /// <summary>
        /// Extends <see cref="IComparer{T}.CalculateDifferences(T, T)"/> operation with <see cref="ComparisonContext"/> parameter.
        /// </summary>
        public static IEnumerable<Difference> CalculateDifferences<T>(this IComparer<T> comparer, T obj1, T obj2, ComparisonContext comparisonContext)
        {
            if (comparer is IContextableComparer<T> contextableComparer)
            {
                return contextableComparer.CalculateDifferences(obj1, obj2, comparisonContext);
            }

            return comparer.CalculateDifferences(obj1, obj2);
        }

        /// <summary>
        /// Extends <see cref="IComparer.Compare(Type, object, object)"/> operation with <see cref="ComparisonContext"/> parameter.
        /// </summary>
        public static bool Compare(this IComparer comparer, Type type, object obj1, object obj2, ComparisonContext comparisonContext)
        {
            if (comparer is IContextableComparer contextableComparer)
            {
                return contextableComparer.Compare(type, obj1, obj2, comparisonContext);
            }

            return comparer.Compare(type, obj1, obj2);
        }

        /// <summary>
        /// Extends <see cref="IComparer.Compare(Type, object, object, out IEnumerable{Difference})"/> operation with <see cref="ComparisonContext"/> parameter.
        /// </summary>
        public static bool Compare(this IComparer comparer, Type type, object obj1, object obj2, out IEnumerable<Difference> differences, ComparisonContext comparisonContext)
        {
            if (comparer is IContextableComparer contextableComparer)
            {
                return contextableComparer.Compare(type, obj1, obj2, out differences, comparisonContext);
            }

            return comparer.Compare(type, obj1, obj2, out differences);
        }

        /// <summary>
        /// Extends <see cref="IComparer{T}.Compare(T, T, out IEnumerable{Difference})"/> operation with <see cref="ComparisonContext"/> parameter.
        /// </summary>
        public static bool Compare<T>(this IComparer<T> comparer, T obj1, T obj2, out IEnumerable<Difference> differences, ComparisonContext comparisonContext)
        {
            if (comparer is IContextableComparer<T> contextableComparer)
            {
                return contextableComparer.Compare(obj1, obj2, out differences, comparisonContext);
            }

            return comparer.Compare(obj1, obj2, out differences);
        }

        /// <summary>
        /// Extends <see cref="IComparer{T}.Compare(T, T))"/> operation with <see cref="ComparisonContext"/> parameter.
        /// </summary>
        public static bool Compare<T>(this IComparer<T> comparer, T obj1, T obj2, ComparisonContext comparisonContext)
        {
            if (comparer is IContextableComparer<T> contextableComparer)
            {
                return contextableComparer.Compare(obj1, obj2, comparisonContext);
            }

            return comparer.Compare(obj1, obj2);
        }        
    }
}