﻿using ObjectsComparer.DifferenceTreeExtensions;
using ObjectsComparer.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ObjectsComparer
{
    internal abstract class EnumerablesComparerBase : AbstractComparer
    {
        public EnumerablesComparerBase(ComparisonSettings settings, BaseComparer parentComparer, IComparersFactory factory) : base(settings, parentComparer, factory)
        {
        }

        /// <summary>
        /// Selects calculation operation based on the current value of the <see cref="ListComparisonOptions.ElementSearchMode"/> property.
        /// </summary>
        protected virtual IEnumerable<DifferenceLocation> CalculateDifferences<T>(IList<T> list1, IList<T> list2, IDifferenceTreeNode listDifferenceTreeNode, ListComparisonOptions listComparisonOptions)
        {
            if (listComparisonOptions.ElementSearchMode == ListElementSearchMode.Key)
            {
                return CalculateDifferencesByKey(list1, list2, listDifferenceTreeNode, listComparisonOptions);
            }
            else if (listComparisonOptions.ElementSearchMode == ListElementSearchMode.Index)
            {
                return CalculateDifferencesByIndex(list1, list2, listDifferenceTreeNode, listComparisonOptions);
            }
            else
            {
                throw new NotImplementedException($"{listComparisonOptions.ElementSearchMode} not implemented yet.");
            }
        }

        /// <summary>
        /// Calculates differences using <see cref="ListElementSearchMode.Key"/> comparison mode.
        /// </summary>
        protected virtual IEnumerable<DifferenceLocation> CalculateDifferencesByKey<T>(IList<T> array1, IList<T> array2, IDifferenceTreeNode listDifferenceTreeNode, ListComparisonOptions listComparisonOptions)
        {
            Debug.WriteLine($"{GetType().Name}.{nameof(CalculateDifferencesByKey)}: {array1?.GetType().Name}");

            var keyOptions = ListElementComparisonByKeyOptions.Default();
            listComparisonOptions.KeyOptionsAction?.Invoke(keyOptions);

            for (int element1Index = 0; element1Index < array1.Count(); element1Index++)
            {
                var element1 = array1[element1Index];
                var elementDifferenceTreeNode = DifferenceTreeNodeProvider.CreateNode(Settings, listDifferenceTreeNode);

                if (element1 == null)
                {
                    if (array2.Any(elm2 => elm2 == null))
                    {
                        continue;
                    }

                    var nullElementIdentifier = keyOptions.GetNullElementIdentifier(new FormatNullElementIdentifierArgs(element1Index));

                    yield return AddDifferenceToTree(elementDifferenceTreeNode, $"[{nullElementIdentifier}]", string.Empty, string.Empty, DifferenceTypes.MissedElementInSecondObject);
                    continue;
                }

                var element1Key = keyOptions.ElementKeyProviderAction(new ListElementKeyProviderArgs(element1));

                if (element1Key == null)
                {
                    if (keyOptions.ThrowKeyNotFoundEnabled)
                    {
                        throw new ElementKeyNotFoundException(element1, elementDifferenceTreeNode);
                    }

                    continue;
                }

                var formattedElement1Key = keyOptions.GetFormattedElementKey(new FormatListElementKeyArgs(element1Index, element1Key, element1));

                if (array2.Any(elm2 => elm2 != null && object.Equals(element1Key, keyOptions.ElementKeyProviderAction(new ListElementKeyProviderArgs(elm2)))))
                {
                    var element2 = array2.First(elm2 => elm2 != null && object.Equals(element1Key, keyOptions.ElementKeyProviderAction(new ListElementKeyProviderArgs(elm2))));
                    var comparer = Factory.GetObjectsComparer(element1.GetType(), Settings, this);

                    foreach (var failure in comparer.TryBuildDifferenceTree(element1.GetType(), element1, element2, elementDifferenceTreeNode))
                    {
                        failure.Difference.InsertPath($"[{formattedElement1Key}]");
                        yield return failure;
                    }
                }
                else
                {
                    var valueComparer1 = OverridesCollection.GetComparer(element1.GetType()) ?? DefaultValueComparer;
                    yield return AddDifferenceToTree(elementDifferenceTreeNode, $"[{formattedElement1Key}]", valueComparer1.ToString(element1), string.Empty, DifferenceTypes.MissedElementInSecondObject, element1);
                }
            }

            for (int element2Index = 0; element2Index < array2.Count(); element2Index++)
            {
                var element2 = array2[element2Index];
                var elementDifferenceTreeNode = DifferenceTreeNodeProvider.CreateNode(Settings, listDifferenceTreeNode);

                if (element2 == null)
                {
                    if (array1.Any(elm1 => elm1 == null))
                    {
                        continue;
                    }

                    var nullElementIdentifier = keyOptions.GetNullElementIdentifier(new FormatNullElementIdentifierArgs(element2Index));

                    yield return AddDifferenceToTree(elementDifferenceTreeNode, $"[{nullElementIdentifier}]", string.Empty, string.Empty, DifferenceTypes.MissedElementInFirstObject);
                    continue;
                }

                var element2Key = keyOptions.ElementKeyProviderAction(new ListElementKeyProviderArgs(element2));

                if (element2Key == null)
                {
                    if (keyOptions.ThrowKeyNotFoundEnabled)
                    {
                        throw new ElementKeyNotFoundException(element2, elementDifferenceTreeNode);
                    }

                    continue;
                }

                if (array1.Any(elm1 => elm1 != null && object.Equals(element2Key, keyOptions.ElementKeyProviderAction(new ListElementKeyProviderArgs(elm1)))) == false)
                {
                    var formattedElement2Key = keyOptions.GetFormattedElementKey(new FormatListElementKeyArgs(element2Index, element2Key, element2));
                    var valueComparer2 = OverridesCollection.GetComparer(element2.GetType()) ?? DefaultValueComparer;
                    yield return AddDifferenceToTree(elementDifferenceTreeNode, $"[{formattedElement2Key}]", string.Empty, valueComparer2.ToString(element2), DifferenceTypes.MissedElementInFirstObject, null, element2);
                }
            }
        }

        /// <summary>
        /// Calculates differences using <see cref="ListElementSearchMode.Index"/> comparison mode.
        /// </summary>
        protected virtual IEnumerable<DifferenceLocation> CalculateDifferencesByIndex<T>(IList<T> array1, IList<T> array2, IDifferenceTreeNode listDifferenceTreeNode, ListComparisonOptions listComparisonOptions)
        {
            Debug.WriteLine($"{GetType().Name}.{nameof(CalculateDifferencesByIndex)}: {array1?.GetType().Name}");

            int array1Count = array1.Count();
            int array2Count = array2.Count();
            int smallerCount = array1Count <= array2Count ? array1Count : array2Count;

            //ToDo Extract type
            for (var i = 0; i < smallerCount; i++)
            {
                var elementDifferenceTreeNode = DifferenceTreeNodeProvider.CreateNode(Settings, listDifferenceTreeNode);

                if (array1[i] == null && array2[i] == null)
                {
                    continue;
                }

                var valueComparer1 = array1[i] != null ? OverridesCollection.GetComparer(array1[i].GetType()) ?? DefaultValueComparer : DefaultValueComparer;
                var valueComparer2 = array2[i] != null ? OverridesCollection.GetComparer(array2[i].GetType()) ?? DefaultValueComparer : DefaultValueComparer;

                if (array1[i] == null)
                {
                    yield return AddDifferenceToTree(elementDifferenceTreeNode, $"[{i}]", string.Empty, valueComparer2.ToString(array2[i]), DifferenceTypes.ValueMismatch, null, array2[i]);
                    continue;
                }

                if (array2[i] == null)
                {
                    yield return AddDifferenceToTree(elementDifferenceTreeNode, $"[{i}]", valueComparer1.ToString(array1[i]), string.Empty, DifferenceTypes.ValueMismatch, array1[i]);
                    continue;
                }

                if (array1[i].GetType() != array2[i].GetType())
                {
                    yield return AddDifferenceToTree(elementDifferenceTreeNode, $"[{i}]", valueComparer1.ToString(array1[i]), valueComparer2.ToString(array2[i]), DifferenceTypes.TypeMismatch, array1[i], array2[i]);
                    continue;
                }

                var comparer = Factory.GetObjectsComparer(array1[i].GetType(), Settings, this);

                foreach (var failure in comparer.TryBuildDifferenceTree(array1[i].GetType(), array1[i], array2[i], elementDifferenceTreeNode))
                {
                    failure.Difference.InsertPath($"[{i}]");
                    yield return failure;
                }
            }

            //Add a "missed element" difference for each element that is in array1 and that is not in array2 or vice versa.
            if (array1Count != array2Count)
            {
                var largerArray = array1Count > array2Count ? array1 : array2;

                for (int i = smallerCount; i < largerArray.Count(); i++)
                {
                    var valueComparer = largerArray[i] != null ? OverridesCollection.GetComparer(largerArray[i].GetType()) ?? DefaultValueComparer : DefaultValueComparer;

                    yield return AddDifferenceToTree(DifferenceTreeNodeProvider.CreateNode(Settings, listDifferenceTreeNode),
                        memberPath: $"[{i}]",
                        value1: array1Count > array2Count ? valueComparer.ToString(largerArray[i]) : string.Empty,
                        value2: array2Count > array1Count ? valueComparer.ToString(largerArray[i]) : string.Empty,
                        differenceType: array1Count > array2Count ? DifferenceTypes.MissedElementInSecondObject : DifferenceTypes.MissedElementInFirstObject,
                        array1Count > array2Count ? largerArray[i] : (object)null,
                        array2Count > array1Count ? largerArray[i] : (object)null);
                }
            }
        }
    }
}
