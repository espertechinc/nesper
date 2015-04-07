///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.time;
using com.espertech.esper.filter;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class ContextDetailConditionTimePeriod : ContextDetailCondition
    {
        public ContextDetailConditionTimePeriod(ExprTimePeriod timePeriod, bool immediate)
        {
            TimePeriod = timePeriod;
            IsImmediate = immediate;
        }

        public ExprTimePeriod TimePeriod { get; set; }

        public IList<FilterSpecCompiled> FilterSpecIfAny
        {
            get { return null; }
        }

        public bool IsImmediate { get; private set; }
    }
}
