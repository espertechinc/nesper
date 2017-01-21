///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.updatehelper;

namespace com.espertech.esper.epl.named
{
    public class NamedWindowOnMergeActionUpd : NamedWindowOnMergeAction
    {
        private readonly EventBeanUpdateHelper _updateHelper;

        public NamedWindowOnMergeActionUpd(ExprEvaluator optionalFilter, EventBeanUpdateHelper updateHelper)
            : base(optionalFilter)
        {
            _updateHelper = updateHelper;
        }
    
        public override void Apply(EventBean matchingEvent, EventBean[] eventsPerStream, OneEventCollection newData, OneEventCollection oldData, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean copy = _updateHelper.UpdateWCopy(matchingEvent, eventsPerStream, exprEvaluatorContext);
            newData.Add(copy);
            oldData.Add(matchingEvent);
        }
    
        public override String GetName()
        {
            return "Update";
        }
    }
}
