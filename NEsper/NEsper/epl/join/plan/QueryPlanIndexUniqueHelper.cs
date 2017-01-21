///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.epl.join.plan
{
    public class QueryPlanIndexUniqueHelper
    {
        public static ReducedHashKeys ReduceToUniqueIfPossible(IList<string> hashPropsProvided,
                                                               IList<Type> hashCoercionTypes,
                                                               IList<QueryGraphValueEntryHashKeyed> hashFunctions,
                                                               String[][] hashPropsRequiredPerIndex)
        {
            if (hashPropsRequiredPerIndex == null || hashPropsRequiredPerIndex.Length == 0)
            {
                return null;
            }
            foreach (String[] hashPropsRequired in hashPropsRequiredPerIndex)
            {
                int[] indexes = CheckSufficientGetAssignment(hashPropsRequired, hashPropsProvided);
                if (indexes != null)
                {
                    var props = new String[indexes.Length];
                    var types = new Type[indexes.Length];
                    var functions = new List<QueryGraphValueEntryHashKeyed>();
                    for (int i = 0; i < indexes.Length; i++)
                    {
                        props[i] = hashPropsProvided[indexes[i]];
                        types[i] = hashCoercionTypes == null ? null : hashCoercionTypes[indexes[i]];
                        functions.Add(hashFunctions[indexes[i]]);
                    }
                    return new ReducedHashKeys(props, types, functions);
                }
            }
            return null;
        }

        public static int[] CheckSufficientGetAssignment(IList<String> hashPropsRequired, IList<string> hashPropsProvided)
        {
            if (hashPropsProvided == null || hashPropsRequired == null || hashPropsProvided.Count < hashPropsRequired.Count)
            {
                return null;
            }
            // first pass: determine if possible
            foreach (String required in hashPropsRequired)
            {
                bool found = false;
                foreach (String provided in hashPropsProvided)
                {
                    if (provided.Equals(required))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return null;
                }
            }

            // second pass: determine assignments
            int[] indexes = new int[hashPropsRequired.Count];
            for (int i = 0; i < indexes.Length; i++)
            {
                int foundIndex = -1;
                String required = hashPropsRequired[i];
                for (int j = 0; j < hashPropsProvided.Count; j++)
                {
                    if (hashPropsProvided[j].Equals(required))
                    {
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
            internal ReducedHashKeys(String[] propertyNames, Type[] coercionTypes, List<QueryGraphValueEntryHashKeyed> hashKeyFunctions)
            {
                PropertyNames = propertyNames;
                CoercionTypes = coercionTypes;
                HashKeyFunctions = hashKeyFunctions;
            }

            public string[] PropertyNames { get; private set; }

            public Type[] CoercionTypes { get; private set; }

            public List<QueryGraphValueEntryHashKeyed> HashKeyFunctions { get; private set; }
        }
    }
}
