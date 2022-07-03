﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectsComparer.DifferenceTreeExtensions;
using ObjectsComparer.Utils;

namespace ObjectsComparer
{
    internal abstract class AbstractDynamicObjectsComprer<T>: AbstractComparer, IComparerWithCondition, IDifferenceTreeBuilder, IDifferenceTreeBuilder<T>
    {
        protected AbstractDynamicObjectsComprer(ComparisonSettings settings, BaseComparer parentComparer, IComparersFactory factory) : base(settings, parentComparer, factory)
        {
        }

        public override IEnumerable<Difference> CalculateDifferences(Type type, object obj1, object obj2)
        {
            return AsDifferenceTreeBuilder().BuildDifferenceTree(type, obj1, obj2, DifferenceTreeNodeProvider.CreateImplicitRootNode(Settings))
                .Select(differenceLocation => differenceLocation.Difference);
        }

        IEnumerable<DifferenceLocation> IDifferenceTreeBuilder<T>.BuildDifferenceTree(T obj1, T obj2, IDifferenceTreeNode differenceTreeNode)
        {
            return AsDifferenceTreeBuilder().BuildDifferenceTree(typeof(T), obj1, obj2, DifferenceTreeNodeProvider.CreateImplicitRootNode(Settings));
        }

        IDifferenceTreeBuilder AsDifferenceTreeBuilder()
        {
            return this;
        }

        IEnumerable<DifferenceLocation> IDifferenceTreeBuilder.BuildDifferenceTree(Type type, object obj1, object obj2, IDifferenceTreeNode differenceTreeNode)
        {
            var castedObject1 = (T)obj1;
            var castedObject2 = (T)obj2;
            var propertyKeys1 = GetProperties(castedObject1);
            var propertyKeys2 = GetProperties(castedObject2);

            var propertyKeys = propertyKeys1.Union(propertyKeys2);

            foreach (var propertyKey in propertyKeys)
            {
                var existsInObject1 = propertyKeys1.Contains(propertyKey);
                var existsInObject2 = propertyKeys2.Contains(propertyKey);
                object value1 = null;
                MemberInfo member1 = null;
                if (existsInObject1)
                {
                    TryGetMemberValue(castedObject1, propertyKey, out value1);
                    TryGetMember(castedObject1, propertyKey, out member1);
                }

                object value2 = null;
                MemberInfo member2 = null;
                if (existsInObject2)
                {
                    TryGetMemberValue(castedObject2, propertyKey, out value2);
                    TryGetMember(castedObject1, propertyKey, out member2);
                }

                var keyDifferenceTreeNode = DifferenceTreeNodeProvider.CreateNode(Settings, differenceTreeNode, member1 ?? member2, propertyKey);

                var propertyType = (value1 ?? value2)?.GetType() ?? typeof(object);
                var customComparer = OverridesCollection.GetComparer(propertyType) ??
                                     OverridesCollection.GetComparer(propertyKey);
                var valueComparer = customComparer ?? DefaultValueComparer;

                if (Settings.UseDefaultIfMemberNotExist)
                {
                    if (!existsInObject1)
                    {
                        value1 = propertyType.GetDefaultValue();
                    }

                    if (!existsInObject2)
                    {
                        value2 = propertyType.GetDefaultValue();
                    }
                }

                if (!Settings.UseDefaultIfMemberNotExist)
                {
                    if (!existsInObject1)
                    {
                        var differenceLocation = AddDifferenceToTree(keyDifferenceTreeNode, propertyKey, string.Empty, valueComparer.ToString(value2), DifferenceTypes.MissedMemberInFirstObject, null, value2);

                        yield return differenceLocation;
                        continue;
                    }

                    if (!existsInObject2)
                    {
                        var differenceLocation = AddDifferenceToTree(
                            keyDifferenceTreeNode,
                            propertyKey,
                            valueComparer.ToString(value1),
                            string.Empty,
                            DifferenceTypes.MissedMemberInSecondObject,
                            value1,
                            null);

                        yield return differenceLocation;
                        continue;
                    }
                }

                if (value1 != null && value2 != null && value1.GetType() != value2.GetType())
                {
                    var valueComparer2 = OverridesCollection.GetComparer(value2.GetType()) ??
                        OverridesCollection.GetComparer(propertyKey) ?? 
                        DefaultValueComparer;

                    var differenceLocation = AddDifferenceToTree(keyDifferenceTreeNode, propertyKey, valueComparer.ToString(value1), valueComparer2.ToString(value2), DifferenceTypes.TypeMismatch, value1, value2);

                    yield return differenceLocation;
                    continue;
                }

                //null cannot be casted to ValueType
                if (value1 == null && value2 != null && value2.GetType().GetTypeInfo().IsValueType ||
                    value2 == null && value1 != null && value1.GetType().GetTypeInfo().IsValueType)
                {
                    var valueComparer2 = value2 != null ? 
                        OverridesCollection.GetComparer(value2.GetType()) ?? OverridesCollection.GetComparer(propertyKey) ?? DefaultValueComparer :
                        DefaultValueComparer;

                    var differenceLocation = AddDifferenceToTree(keyDifferenceTreeNode, propertyKey, valueComparer.ToString(value1), valueComparer2.ToString(value2), DifferenceTypes.TypeMismatch, value1, value2);

                    yield return differenceLocation;
                    continue;
                }

                if (customComparer != null)
                {
                    if (!customComparer.Compare(value1, value2, Settings))
                    {
                        var differenceLocation = AddDifferenceToTree(keyDifferenceTreeNode, propertyKey, customComparer.ToString(value1), customComparer.ToString(value2), DifferenceTypes.ValueMismatch, value1, value2);

                        yield return differenceLocation;
                    }

                    continue;
                }

                var comparer = Factory.GetObjectsComparer(propertyType, Settings, this);
                foreach (var failure in comparer.TryBuildDifferenceTree(propertyType, value1, value2, keyDifferenceTreeNode))
                {
                    InsertPathToDifference(failure.Difference, propertyKey, keyDifferenceTreeNode,failure.TreeNode);
                    yield return failure;
                }
            }
        }
        
        public abstract bool IsMatch(Type type, object obj1, object obj2);

        public abstract bool IsStopComparison(Type type, object obj1, object obj2);

        public abstract bool SkipMember(Type type, MemberInfo member);

        protected abstract IList<string> GetProperties(T obj);
        
        protected abstract bool TryGetMemberValue(T obj, string propertyName, out object value);

        protected abstract bool TryGetMember(T obj, string propertyName, out MemberInfo value);
    }
}