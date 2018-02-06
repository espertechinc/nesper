///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    public class AggregationStateSortedSpec
    {
        public AggregationStateSortedSpec(
            int streamId,
            ExprEvaluator[] criteria,
            IComparer<object> comparator,
            object criteriaKeyBinding,
            ExprEvaluator optionalFilter)
        {
            StreamId = streamId;
            Criteria = criteria;
            Comparator = comparator;
            CriteriaKeyBinding = criteriaKeyBinding;
            OptionalFilter = optionalFilter;
        }

        public int StreamId { get; }

        public ExprEvaluator[] Criteria { get; }

        public IComparer<object> Comparator { get; }

        public object CriteriaKeyBinding { get; set; }

        public ExprEvaluator OptionalFilter { get; }
    }
} // end of namespace