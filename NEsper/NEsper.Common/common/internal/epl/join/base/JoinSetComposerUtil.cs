///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.queryplan;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    public class JoinSetComposerUtil
    {
        private static readonly EventTable[] EMPTY = new EventTable[0];

        public static bool IsNonUnidirectionalNonSelf(
            bool isOuterJoins,
            bool isUnidirectional,
            bool isPureSelfJoin)
        {
            return !isUnidirectional &&
                   (!isPureSelfJoin || isOuterJoins);
        }

        public static void Filter(
            ExprEvaluator filterExprNode,
            ISet<MultiKey<EventBean>> events,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            events
                .Where(
                    value => {
                        var matched = filterExprNode.Evaluate(value.Array, isNewData, exprEvaluatorContext);
                        return matched == null || false.Equals(matched);
                    })
                .ToList()
                .ForEach(value => events.Remove(value));
        }

        public static EventTable[][] ToArray(IDictionary<TableLookupIndexReqKey, EventTable>[] repositories)
        {
            return ToArray(repositories, repositories.Length);
        }

        public static EventTable[][] ToArray(
            IDictionary<TableLookupIndexReqKey, EventTable>[] repositories,
            int length)
        {
            if (repositories == null) {
                return GetDefaultTablesArray(length);
            }

            var tables = new EventTable[repositories.Length][];
            for (var i = 0; i < repositories.Length; i++) {
                tables[i] = ToArray(repositories[i]);
            }

            return tables;
        }

        private static EventTable[] ToArray(IDictionary<TableLookupIndexReqKey, EventTable> repository)
        {
            if (repository == null) {
                return EMPTY;
            }

            var tables = new EventTable[repository.Count];
            var count = 0;
            foreach (var entries in repository) {
                tables[count] = entries.Value;
                count++;
            }

            return tables;
        }

        private static EventTable[][] GetDefaultTablesArray(int length)
        {
            var result = new EventTable[length][];
            for (var i = 0; i < result.Length; i++) {
                result[i] = EMPTY;
            }

            return result;
        }
    }
} // end of namespace