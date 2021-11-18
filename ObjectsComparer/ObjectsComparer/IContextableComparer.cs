using System;
using System.Collections.Generic;

namespace ObjectsComparer
{
    /// <summary>
    /// A comparer that accepts <see cref="ComparisonContext"/>.
    /// </summary>
    public interface IContextableComparer
    {
        IEnumerable<Difference> CalculateDifferences(Type type, object obj1, object obj2, ComparisonContext comparisonContext);

        bool Compare(Type type, object obj1, object obj2, out IEnumerable<Difference> differences, ComparisonContext comparisonContext);
      
        bool Compare(Type type, object obj1, object obj2, ComparisonContext comparisonContext);
    }
}      