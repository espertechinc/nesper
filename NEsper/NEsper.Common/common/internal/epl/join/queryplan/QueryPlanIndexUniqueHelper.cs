///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.@join.querygraph;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    public class QueryPlanIndexUniqueHelper
    {
        public static ReducedHashKeys ReduceToUniqueIfPossible(
            string[] hashPropsProvided,
            Type[] hashCoercionTypes,
            IList<QueryGraphValueEntryHashKeyedForge> hashFunctions,
            string[][] hashPropsRequiredPerIndex)
        {
            if (hashPropsRequiredPerIndex == null || hashPropsRequiredPerIndex.Length == 0) {
                return null;
            }

            foreach (var hashPropsRequired in hashPropsRequiredPerIndex) {
                var indexes = CheckSufficientGetAssignment(hashPropsRequired, hashPropsProvided);
                if (indexes != null) {
                    var props = new string[indexes.Length];
                    var types = new Type[indexes.Length];
                    IList<QueryGraphValueEntryHashKeyedForge> functions = new List<QueryGraphValueEntryHashKeyedForge>();
                    for (var i = 0; i < indexes.Length; i++) {
                        props[i] = hashPropsProvided[indexes[i]];
                        types[i] = hashCoercionTypes == null ? null : hashCoercionTypes[indexes[i]];
                        functions.Add(hashFunctions[indexes[i]]);
                    }

                    return new ReducedHashKeys(props, types, functions);
                }
            }

            return null;
        }

        public static int[] CheckSufficientGetAssignment(
            string[] hashPropsRequired,
            string[] hashPropsProvided)
        {
            if (hashPropsProvided == null || hashPropsRequired == null || hashPropsProvided.Length < hashPropsRequired.Length) {
                return null;
            }

            // first pass: determine if possible
            foreach (var required in hashPropsRequired) {
                var found = false;
                foreach (var provided in hashPropsProvided) {
                    if (provided.Equals(required)) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    return null;
                }
            }

            // second pass: determine assignments
            var indexes = new int[hashPropsRequired.Length];
            for (var i = 0; i < indexes.Length; i++) {
                var foundIndex = -1;
                var required = hashPropsRequired[i];
                for (var j = 0; j < hashPropsProvided.Length; j++) {
                    if (hashPropsProvided[j].Equals(required)) {
                        foundIndex = j;
                        break;
                    }
                }

                indexes[i] = foundIndex;
            }

            return indexes;
        }

        public class ReducedHashKeys
        {
            internal ReducedHashKeys(
                string[] propertyNames,
                Type[] coercionTypes,
                IList<QueryGraphValueEntryHashKeyedForge> hashKeyFunctions)
            {
                PropertyNames = propertyNames;
                CoercionTypes = coercionTypes;
                HashKeyFunctions = hashKeyFunctions;
            }

            public string[] PropertyNames { get; }

            public Type[] CoercionTypes { get; }

            public IList<QueryGraphValueEntryHashKeyedForge> HashKeyFunctions { get; }
        }
    }
} // end of namespace