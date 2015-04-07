///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.service
{
    /// <summary>Interface for a service that routes events within the engine for further processing. </summary>
    public interface InternalEventRouter
    {
        InternalEventRouterDesc GetValidatePreprocessing(EventType eventType, UpdateDesc desc, Attribute[] annotations);
    
        void AddPreprocessing(InternalEventRouterDesc internalEventRouterDesc, InternalRoutePreprocessView outputView, IReaderWriterLock agentInstanceLock, Boolean hasSubselect);

        /// <summary>Remove preprocessing. </summary>
        /// <param name="eventType">type to remove for</param>
        /// <param name="desc">Update statement specification</param>
        void RemovePreprocessing(EventType eventType, UpdateDesc desc);

        /// <summary>
        /// Route the event such that the event is processed as required.
        /// </summary>
        /// <param name="theEvent">to route</param>
        /// <param name="statementHandle">provides statement resources</param>
        /// <param name="routeDest">routing destination</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <param name="addToFront">if set to <c>true</c> [add to front].</param>
        void Route(EventBean theEvent, EPStatementHandle statementHandle, InternalEventRouteDest routeDest, ExprEvaluatorContext exprEvaluatorContext, bool addToFront);

        bool HasPreprocessing { get; }

        EventBean Preprocess(EventBean theEvent, ExprEvaluatorContext engineFilterAndDispatchTimeContext);

        InsertIntoListener InsertIntoListener { set; }
    }
}
