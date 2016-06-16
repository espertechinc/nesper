///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.view
{
    /// <summary>Service interface for creating views. </summary>
    public interface ViewService
    {
        /// <summary>
        /// Returns a chain of view factories that can be used to obtain the readonly event type, and that can later be used 
        /// to actually create the chain of views or reuse existing views.
        /// <para />
        /// Does not actually hook up the view factories or views against the event stream, but creates view factories and 
        /// sets parameters on each view factory as supplied. Determines if view factories are compatible in the chain via 
        /// the attach method.
        /// </summary>
        /// <param name="streamNum">the stream number starting at zero, a join would have Count streams</param>
        /// <param name="parentEventType">is the event type of the event stream that originates the raw events</param>
        /// <param name="viewSpecList">the specification for each view factory in the chain to be created</param>
        /// <param name="options">stream options such as unidirectional, retain-union etc</param>
        /// <param name="context">dependent services</param>
        /// <param name="isSubquery">if set to <c>true</c> [is subquery].</param>
        /// <param name="subqueryNumber">The subquery number.</param>
        /// <returns>
        /// chain of view factories
        /// </returns>
        /// <throws>ViewProcessingException thrown if a view factory doesn't take parameters as supplied,or cannot hook onto it's parent view or event stream </throws>
        ViewFactoryChain CreateFactories(
            int streamNum,
            EventType parentEventType,
            ViewSpec[] viewSpecList,
            StreamSpecOptions options,
            StatementContext context,
            bool isSubquery,
            int subqueryNumber);

        /// <summary>
        /// Creates the views given a chain of view factories. <para/>
        /// Attempts to reuse compatible views under then parent event stream viewable as indicated by 
        /// each view factories reuse method.
        /// </summary>
        /// <param name="eventStreamViewable">is the event stream to hook into</param>
        /// <param name="viewFactoryChain">defines the list of view factorys to call makeView or canReuse on</param>
        /// <param name="viewFactoryChainContext">provides services</param>
        /// <param name="hasPreviousNode">if set to <c>true</c> [has previous node].</param>
        /// <returns>
        /// last viewable in chain, or the eventStreamViewable if no view factories are supplied
        /// </returns>
        ViewServiceCreateResult CreateViews(
            Viewable eventStreamViewable,
            IList<ViewFactory> viewFactoryChain,
            AgentInstanceViewFactoryChainContext viewFactoryChainContext,
            bool hasPreviousNode);

        /// <summary>
        /// Removes a view discoupling the view and any of it's parent views up the tree to the last shared parent view.
        /// </summary>
        /// <param name="eventStream">the event stream that originates the raw events</param>
        /// <param name="view">the view (should be the last in a chain) to remove</param>
        void Remove(EventStream eventStream, Viewable view);
    }
}
