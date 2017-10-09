///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.@base;

namespace com.espertech.esper.supportunit.epl
{
    public class SupportJoinSetProcessor : JoinSetProcessor
    {
        private ICollection<MultiKey<EventBean>> _lastNewEvents;
        private ICollection<MultiKey<EventBean>> _lastOldEvents;

        public void Process(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            _lastNewEvents = newEvents;
            _lastOldEvents = oldEvents;
        }

        public ICollection<MultiKey<EventBean>> LastNewEvents
        {
            get { return _lastNewEvents; }
        }

        public ICollection<MultiKey<EventBean>> LastOldEvents
        {
            get { return _lastOldEvents; }
        }

        public void Reset()
        {
            _lastNewEvents = null;
            _lastOldEvents = null;
        }
    }
}
