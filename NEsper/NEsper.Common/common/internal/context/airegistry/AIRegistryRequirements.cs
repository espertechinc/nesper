///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.airegistry
{
    public class AIRegistryRequirements
    {
        public AIRegistryRequirements(
            bool[] priorFlagsPerStream, bool[] previousFlagsPerStream, AIRegistryRequirementSubquery[] subqueries,
            int tableAccessCount, bool isRowRecogWithPrevious)
        {
            PriorFlagsPerStream = priorFlagsPerStream;
            PreviousFlagsPerStream = previousFlagsPerStream;
            Subqueries = subqueries;
            TableAccessCount = tableAccessCount;
            IsRowRecogWithPrevious = isRowRecogWithPrevious;
        }

        public bool[] PriorFlagsPerStream { get; }

        public bool[] PreviousFlagsPerStream { get; }

        public AIRegistryRequirementSubquery[] Subqueries { get; }

        public int TableAccessCount { get; }

        public bool IsRowRecogWithPrevious { get; }

        public static AIRegistryRequirements NoRequirements()
        {
            return new AIRegistryRequirements(null, null, null, 0, false);
        }

        public static AIRegistryRequirementSubquery[] GetSubqueryRequirements(
            IDictionary<int, SubSelectFactory> subselects)
        {
            if (subselects == null || subselects.IsEmpty()) {
                return null;
            }

            var subqueries = new AIRegistryRequirementSubquery[subselects.Count];
            foreach (var entry in subselects) {
                subqueries[entry.Key] = entry.Value.RegistryRequirements;
            }

            return subqueries;
        }
    }
} // end of namespace