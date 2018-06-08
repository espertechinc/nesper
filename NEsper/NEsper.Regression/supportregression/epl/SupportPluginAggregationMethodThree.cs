///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;

namespace com.espertech.esper.supportregression.epl
{
    [Serializable]
    public class SupportPluginAggregationMethodThree : AggregationMethod
    {
        private static readonly IList<AggregationValidationContext> ContextList =
            new List<AggregationValidationContext>();

        private int _count;

        public void Clear()
        {
            _count = 0;
        }

        public static object[] LastEnterParameters { get; private set; }

        public void Enter(Object value)
        {
            var paramList = (object[]) value;
            LastEnterParameters = paramList;
            var lower = (int) paramList[0];
            var upper = (int)paramList[1];
            var val = (int)paramList[2];
            if ((val >= lower) && (val <= upper))
            {
                _count++;
            }
        }

        public void Leave(Object value)
        {
            var paramList = (object[]) value;
            var lower = (int)paramList[0];
            var upper = (int)paramList[1];
            var val = (int)paramList[2];
            if ((val >= lower) && (val <= upper))
            {
                _count--;
            }
        }

        public object Value
        {
            get { return _count; }
        }
    }
}
