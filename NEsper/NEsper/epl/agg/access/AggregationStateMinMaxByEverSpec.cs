///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.access
{
    public class AggregationStateMinMaxByEverSpec
    {
        public AggregationStateMinMaxByEverSpec(int streamId, ExprEvaluator[] criteria, bool max, IComparer<Object> comparator, Object criteriaKeyBinding)
        {
            StreamId = streamId;
            Criteria = criteria;
            IsMax = max;
            Comparator = comparator;
            CriteriaKeyBinding = criteriaKeyBinding;
        }

        public int StreamId { get; private set; }

        public ExprEvaluator[] Criteria { get; private set; }

        public bool IsMax { get; private set; }

        public IComparer<object> Comparator { get; private set; }

        public object CriteriaKeyBinding { get; set; }
    }
}
