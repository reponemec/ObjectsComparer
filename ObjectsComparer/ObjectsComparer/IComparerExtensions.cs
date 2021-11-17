﻿using System;
using System.Collections.Generic;

namespace ObjectsComparer
{
    /// <summary>
    /// Extends interface <see cref="IComparer"/> and <see cref="IComparer{T}"/> with overloaded operations CalculateDifferences and Compare that accept <see cref="ComparisonContext"/> parameter.
    /// </summary>
    public static class IComparerExtensions
    {
        /// <summary>
        /// Calculates list of differences between objects. Accepts comparison context.
        /// At the beginning of the comparison you can create <see cref="ComparisonContext"/> instance using the <see cref="ComparisonContext.CreateRoot"/> operation and pass it as a parameter.
        /// For more info about comparison context see <see cref="ComparisonContext"/> class.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="obj1">Object 1.</param>
        /// <param name="obj2">Object 2.</param>
        /// <param name="comparisonContext">Current comparison context. For more info see <see cref="ComparisonContext"/> class.</param>
        /// <returns>List of differences between objects.</returns>
        public static IEnumerable<Difference> CalculateDifferences(this IComparer comparer, Type type, object obj1, object obj2, ComparisonContext comparisonContext)
        {
            if (comparer is IContextableComparer contextableComparer)
            {
                return contextableComparer.CalculateDifferences(type, obj1, obj2, comparisonContext);
            }

            return comparer.CalculateDifferences(type, obj1, obj2);
        }

        public static IEnumerable<Difference> CalculateDifferences<T>(this IComparer<T> comparer, T obj1, T obj2, ComparisonContext comparisonContext)
        {
            if (comparer is IContextableComparer<T> contextableComparer)
            {
                return contextableComparer.CalculateDifferences(obj1, obj2, comparisonContext);
            }

            return comparer.CalculateDifferences(obj1, obj2);
        }

        public static bool Compare<T>(this IComparer<T> comparer, T obj1, T obj2, out IEnumerable<Difference> differences, ComparisonContext comparisonContext)
        {
            if (comparer is IContextableComparer<T> contextableComparer)
            {
                return contextableComparer.Compare(obj1, obj2, out differences, comparisonContext);
            }

            return comparer.Compare(obj1, obj2, out differences);
        }

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