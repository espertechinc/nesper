///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core.eval
{
    public abstract class EvalBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EventType _resultEventType;
        private readonly SelectExprContext _selectExprContext;

        protected EvalBase(SelectExprContext selectExprContext, EventType resultEventType)
        {
            _selectExprContext = selectExprContext;
            _resultEventType = resultEventType;
        }

        internal SelectExprContext SelectExprContext
        {
            get { return _selectExprContext; }
        }

        public EventAdapterService EventAdapterService
        {
            get { return _selectExprContext.EventAdapterService; }
        }

        public EventType ResultEventType
        {
            get { return _resultEventType; }
        }

        public ExprEvaluator[] ExprNodes
        {
            get { return _selectExprContext.ExpressionNodes; }
        }
    }
} // end of namespace