///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.filter;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.spec
{
    public class ContextDetailConditionCrontab : ContextDetailCondition
    {
        public ContextDetailConditionCrontab(IList<ExprNode> crontab, bool immediate)
        {
            ScheduleCallbackId = -1;
            Crontab = crontab;
            IsImmediate = immediate;
        }

        public IList<ExprNode> Crontab { get; private set; }

        public ScheduleSpec Schedule { get; set; }

        public IList<FilterSpecCompiled> FilterSpecIfAny
        {
            get { return null; }
        }

        public bool IsImmediate { get; private set; }

        public int ScheduleCallbackId { get; set; }
    }
}
