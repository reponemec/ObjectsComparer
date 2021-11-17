﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectsComparer.Utils;

namespace ObjectsComparer
{
    /// <summary>
    /// Compares objects.
    /// </summary>
    public class Comparer : AbstractComparer, IContextableComparer
    {
        private static string CalculateDifferencesMethodName
        {
            // ReSharper disable once IteratorMethodResultIsIgnored
            get { return MemberInfoExtensions.GetMethodName<Comparer<object>>(x => x.CalculateDifferences(null, null)); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Comparer" /> class. 
        /// </summary>
        /// <param name="settings">Comparison Settings.</param>
        /// <param name="parentComparer">Parent Comparer. Is used to copy DefaultValueComparer and Overrides. Null by default.</param>
        /// <param name="factory">Factory to create comparers in case of some members of the objects will need it.</param>
        public Comparer(ComparisonSettings settings = null, BaseComparer parentComparer = null, IComparersFactory factory = null) : base(settings, parentComparer, factory)
        {
        }

        public override IEnumerable<Difference> CalculateDifferences(Type type, object obj1, object obj2)
        {
            return CalculateDifferences(type, obj1, obj2, ComparisonContext.CreateRoot());
        }

        public IEnumerable<Difference> CalculateDifferences(Type type, object obj1, object obj2, ComparisonContext comparisonContext)
        {
            if (comparisonContext is null)
            {
                throw new ArgumentNullException(nameof(comparisonContext));
            }

            var getObjectsComparerMethod = typeof(IComparersFactory).GetTypeInfo().GetMethods().First(m => m.IsGenericMethod);
            var getObjectsComparerGenericMethod = getObjectsComparerMethod.MakeGenericMethod(type);
            var comparer = getObjectsComparerGenericMethod.Invoke(Factory, new object[] { Settings, this });

            bool comparerIsContextable = comparer.GetType().GetTypeInfo().GetInterfaces()
                .Any(intft => intft.GetTypeInfo().IsGenericType && intft.GetGenericTypeDefinition() == typeof(IContextableComparer<>));

            var genericType = comparerIsContextable ? typeof(IContextableComparer<>).MakeGenericType(type) : typeof(IComparer<>).MakeGenericType(type);
            var genericMethodParameterTypes = comparerIsContextable ? new[] { type, type, typeof(ComparisonContext) } : new[] { type, type };
            var genericMethod = genericType.GetTypeInfo().GetMethod(CalculateDifferencesMethodName, genericMethodParameterTypes);
            var genericMethodParameters = comparerIsContextable ? new[] { obj1, obj2, comparisonContext } : new[] { obj1, obj2 };

            // ReSharper disable once PossibleNullReferenceException
            return (IEnumerable<Difference>)genericMethod.Invoke(comparer, genericMethodParameters);
        }

        public bool Compare(Type type, object obj1, object obj2, out IEnumerable<Difference> differences, ComparisonContext comparisonContext)
        {
            differences = CalculateDifferences(type, obj1, obj2, comparisonContext);
            return differences.Any();
        }

        public bool Compare<T>(T obj1, T obj2, out IEnumerable<Difference> differences, ComparisonContext comparisonContext)
        {
            differences = CalculateDifferences(typeof(T), obj1, obj2, comparisonContext);
            return differences.Any();
        }
    }
}
