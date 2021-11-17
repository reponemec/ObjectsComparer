﻿using System;
using System.Collections.Generic;

namespace ObjectsComparer
{
    /// <summary>
    /// Comparer accepting <see cref="ComparisonContext"/>.
    /// </summary>
    public interface IContextableComparer
    {
        IEnumerable<Difference> CalculateDifferences(Type type, object obj1, object obj2, ComparisonContext comparisonContext);

        bool Compare(Type type, object obj1, object obj2, out IEnumerable<Difference> differences, ComparisonContext comparisonContext);

        //bool Compare<T>(T obj1, T obj2, out IEnumerable<Difference> differences, ComparisonContext comparisonContext);
      
        bool Compare(Type type, object obj1, object obj2);

        bool Compare<T>(T obj1, T obj2);
    }
}      