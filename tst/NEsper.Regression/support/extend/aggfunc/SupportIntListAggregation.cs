///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.compat;

namespace com.espertech.esper.regressionlib.support.extend.aggfunc
{
    public class SupportIntListAggregation : AggregationFunction
    {
        private readonly IList<int> values = new List<int>();

        public void Enter(object value)
        {
            values.Add(value.AsInt32());
        }

        public void Leave(object value)
        {
        }

        public object Value => new List<int>(values);

        public void Clear()
        {
            values.Clear();
        }
    }
} // end of namespace