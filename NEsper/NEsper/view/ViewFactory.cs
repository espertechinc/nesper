///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.view
{
    /// <summary>
    /// Factory interface for a factory responsible for creating a <seealso cref="View"/> instance and for determining if an existing view meets requirements.
    /// </summary>
    public interface ViewFactory
    {
        /// <summary>Indicates user EPL query view parameters to the view factory. </summary>
        /// <param name="viewFactoryContext">supplied context information for the view factory</param>
        /// <param name="viewParameters">is the objects representing the view parameters</param>
        /// <throws>ViewParameterException if the parameters don't match view parameter needs</throws>
        void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParameters);

        /// <summary>Attaches the factory to a parent event type such that the factory can validate attach requirements and determine an event type for resulting views. </summary>
        /// <param name="parentEventType">is the parent event stream's or view factory's event type</param>
        /// <param name="statementContext">contains the services needed for creating a new event type</param>
        /// <param name="optionalParentFactory">is null when there is no parent view factory, or contains theparent view factory </param>
        /// <param name="parentViewFactories">is a list of all the parent view factories or empty list if there are none</param>
        /// <throws>ViewParameterException is thrown to indicate that this view factories's view would not playwith the parent view factories view </throws>
        void Attach(
            EventType parentEventType,
            StatementContext statementContext,
            ViewFactory optionalParentFactory,
            IList<ViewFactory> parentViewFactories);

        /// <summary>Create a new view. </summary>
        /// <param name="agentInstanceViewFactoryContext"></param>
        View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext);

        /// <summary>Returns the event type that the view that is created by the view factory would create for events posted by the view. </summary>
        /// <value>event type of view&apos;s created by the view factory</value>
        EventType EventType { get; }

        /// <summary>Determines if the given view could be used instead of creating a new view, requires the view factory to compare view type, parameters and other capabilities provided. </summary>
        /// <param name="view">is the candidate view to compare to</param>
        /// <returns>true if the given view can be reused instead of creating a new view, or false to indicatethe view is not right for reuse </returns>
        bool CanReuse(View view);

        /// <summary>Returns the name of the view, not namespace+name but readable name. </summary>
        /// <value>readable name</value>
        string ViewName { get; }
    }
}