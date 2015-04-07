///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.support.core
{
    public class SupportInternalEventRouter : InternalEventRouter
    {
        private readonly List<EventBean> _routed  = new List<EventBean>();
    
        public List<EventBean> GetRouted()
        {
            return _routed;
        }
    
        public void Reset()
        {
            _routed.Clear();
        }
    
        public InternalEventRouterDesc GetValidatePreprocessing(EventType eventType, UpdateDesc desc, Attribute[] annotations)
        {
            return null;
        }

        public void AddPreprocessing(InternalEventRouterDesc internalEventRouterDesc, InternalRoutePreprocessView outputView, IReaderWriterLock agentInstanceLock, bool hasSubselect)
        {
        }
    
        public void RemovePreprocessing(EventType eventType, UpdateDesc desc) {
        }
    
        public void Route(EventBean theEvent, EPStatementHandle statementHandle, InternalEventRouteDest routeDest, ExprEvaluatorContext exprEvaluatorContext, bool addToFront) {
            _routed.Add(theEvent);
        }

        public bool HasPreprocessing
        {
            get { return false; }
        }

        public EventBean Preprocess(EventBean theEvent, ExprEvaluatorContext engineFilterAndDispatchTimeContext)
        {
            return null;
        }

        public InsertIntoListener InsertIntoListener
        {
            set { }
        }
    }
}
