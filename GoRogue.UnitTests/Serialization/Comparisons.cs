﻿using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.Components;
using GoRogue.DiceNotation;
using GoRogue.Factories;
using GoRogue.SerializedTypes.Components;
using GoRogue.SerializedTypes.Factories;
using GoRogue.UnitTests.Mocks;

namespace GoRogue.UnitTests.Serialization
{
    public static class Comparisons
    {
        // Dictionary of object types mapping them to custom methods to use in order to determine equality.
        private static readonly Dictionary<Type, Func<object, object, bool>> _equalityMethods =
            new Dictionary<Type, Func<object, object, bool>>()
            {
                { typeof(ComponentCollection), CompareComponentCollections },
                { typeof(ComponentCollectionSerialized), CompareComponentCollectionSerialized },
                { typeof(DiceExpression), CompareDiceExpressions },
                { typeof(Factory<FactoryItem>), CompareFactory },
                { typeof(FactorySerialized<FactoryItem>), CompareFactorySerialized },
                { typeof(AdvancedFactory<int, FactoryItem>), CompareAdvancedFactory },
                { typeof(AdvancedFactorySerialized<int, FactoryItem>), CompareAdvancedFactorySerialized },
            };

        public static Func<object, object, bool> GetComparisonFunc(object obj)
            => _equalityMethods.GetValueOrDefault(obj.GetType(), (o1, o2) => o1.Equals(o2))!;


        private static bool CompareDiceExpressions(object o1, object o2)
        {
            var e1 = (DiceExpression)o1;
            var e2 = (DiceExpression)o2;

            // ToString returns parsable expressions so this should suffice
            return e1.ToString() == e2.ToString();
        }

        private static bool CompareComponentCollections(object o1, object o2)
        {
            var c1 = (ComponentCollection)o1;
            var c2 = (ComponentCollection)o2;

            var hash1 = c1.ToHashSet();
            var hash2 = c2.ToHashSet();

            return HashSetEquality(hash1, hash2);
        }

        private static bool CompareComponentCollectionSerialized(object o1, object o2)
        {
            var c1 = (ComponentCollectionSerialized)o1;
            var c2 = (ComponentCollectionSerialized)o2;

            return ElementWiseEquality(c1.Components, c2.Components);
        }

        private static bool CompareFactory(object o1, object o2)
        {
            var f1 = (Factory<FactoryItem>)o1;
            var f2 = (Factory<FactoryItem>)o2;

            return HashSetEquality(f1.ToHashSet(), f2.ToHashSet());
        }

        private static bool CompareFactorySerialized(object o1, object o2)
        {
            var f1 = (FactorySerialized<FactoryItem>)o1;
            var f2 = (FactorySerialized<FactoryItem>)o2;

            return HashSetEquality(f1.Blueprints.ToHashSet(), f2.Blueprints.ToHashSet());
        }

        private static bool CompareAdvancedFactory(object o1, object o2)
        {
            var f1 = (AdvancedFactory<int, FactoryItem>)o1;
            var f2 = (AdvancedFactory<int, FactoryItem>)o2;

            return HashSetEquality(f1.ToHashSet(), f2.ToHashSet());
        }

        private static bool CompareAdvancedFactorySerialized(object o1, object o2)
        {
            var f1 = (AdvancedFactorySerialized<int, FactoryItem>)o1;
            var f2 = (AdvancedFactorySerialized<int, FactoryItem>)o2;

            return HashSetEquality(f1.Blueprints.ToHashSet(), f2.Blueprints.ToHashSet());
        }


        private static bool HashSetEquality<T>(HashSet<T> h1, HashSet<T> h2)
        {
            foreach (var value in h1)
                if (!h2.Contains(value))
                    return false;

            foreach (var value in h2)
                if (!h1.Contains(value))
                    return false;

            return true;
        }

        private static bool ElementWiseEquality<T>(IEnumerable<T> e1, IEnumerable<T> e2,
                                                   Func<T, T, bool>? compareFunc = null)
        {
            compareFunc ??= (o1, o2) => o1?.Equals(o2) ?? o2 == null;

            var l1 = e1.ToList();
            var l2 = e2.ToList();

            if (l1.Count != l2.Count)
                return false;

            for (int i = 0; i < l1.Count; i++)
                if (!compareFunc(l1[i], l2[i]))
                    return false;

            return true;
        }
    }
}
