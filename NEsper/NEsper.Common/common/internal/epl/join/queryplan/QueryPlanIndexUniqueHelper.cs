///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
	public class QueryPlanIndexUniqueHelper {
	    public static ReducedHashKeys ReduceToUniqueIfPossible(string[] hashPropsProvided, Type[] hashCoercionTypes, IList<QueryGraphValueEntryHashKeyedForge> hashFunctions, string[][] hashPropsRequiredPerIndex) {
	        if (hashPropsRequiredPerIndex == null || hashPropsRequiredPerIndex.Length == 0) {
	            return null;
	        }
	        foreach (string[] hashPropsRequired in hashPropsRequiredPerIndex) {
	            int[] indexes = CheckSufficientGetAssignment(hashPropsRequired, hashPropsProvided);
	            if (indexes != null) {
	                string[] props = new string[indexes.Length];
	                Type[] types = new Type[indexes.Length];
	                IList<QueryGraphValueEntryHashKeyedForge> functions = new List<QueryGraphValueEntryHashKeyedForge>();
	                for (int i = 0; i < indexes.Length; i++) {
	                    props[i] = hashPropsProvided[indexes[i]];
	                    types[i] = hashCoercionTypes == null ? null : hashCoercionTypes[indexes[i]];
	                    functions.Add(hashFunctions.Get(indexes[i]));
	                }
	                return new ReducedHashKeys(props, types, functions);
	            }
	        }
	        return null;
	    }

	    public static int[] CheckSufficientGetAssignment(string[] hashPropsRequired, string[] hashPropsProvided) {
	        if (hashPropsProvided == null || hashPropsRequired == null || hashPropsProvided.Length < hashPropsRequired.Length) {
	            return null;
	        }
	        // first pass: determine if possible
	        foreach (string required in hashPropsRequired) {
	            bool found = false;
	            foreach (string provided in hashPropsProvided) {
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
	        int[] indexes = new int[hashPropsRequired.Length];
	        for (int i = 0; i < indexes.Length; i++) {
	            int foundIndex = -1;
	            string required = hashPropsRequired[i];
	            for (int j = 0; j < hashPropsProvided.Length; j++) {
	                if (hashPropsProvided[j].Equals(required)) {
	                    foundIndex = j;
	                    break;
	                }
	            }
	            indexes[i] = foundIndex;
	        }
	        return indexes;
	    }

	    public class ReducedHashKeys {
	        private readonly string[] propertyNames;
	        private readonly Type[] coercionTypes;
	        private readonly IList<QueryGraphValueEntryHashKeyedForge> hashKeyFunctions;

	        private ReducedHashKeys(string[] propertyNames, Type[] coercionTypes, IList<QueryGraphValueEntryHashKeyedForge> hashKeyFunctions) {
	            this.propertyNames = propertyNames;
	            this.coercionTypes = coercionTypes;
	            this.hashKeyFunctions = hashKeyFunctions;
	        }

	        public string[] PropertyNames
	        {
	            get => propertyNames;
	        }

	        public Type[] CoercionTypes
	        {
	            get => coercionTypes;
	        }

	        public IList<QueryGraphValueEntryHashKeyedForge> HashKeyFunctions
	        {
	            get => hashKeyFunctions;
	        }
	    }
	}
} // end of namespace