using System;
using System.Collections.Generic;

namespace ObjectsComparer
{
    /// <summary>
    /// A generic comparer that accepts <see cref="ComparisonContext"/>.
    /// </summary>
    public interface IContextableComparer<T>
    {
        IEnumerable<Difference> CalculateDifferences(T obj1, T obj2, ComparisonContext comparisonContext);

        bool Compare(T obj1, T obj2, out IEnumerable<Difference> differences, ComparisonContext comparisonContext);

        bool Compare(T obj1, T obj2, ComparisonContext comparisonContext);
    }
}
