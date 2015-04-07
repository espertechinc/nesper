///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;

namespace com.espertech.esper.core.context.activator
{
    public interface ViewableActivator
    {
        ViewableActivationResult Activate(AgentInstanceContext agentInstanceContext, bool isSubselect, bool isRecoveringResilient);
    }

    public class ProxyViewableActivator : ViewableActivator
    {
        /// <summary>
        /// Gets or sets the proc activate.
        /// </summary>
        /// <value>
        /// The proc activate.
        /// </value>
        public Func<AgentInstanceContext, bool, bool, ViewableActivationResult> ProcActivate { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyViewableActivator"/> class.
        /// </summary>
        /// <param name="procActivate">The proc activate.</param>
        public ProxyViewableActivator(Func<AgentInstanceContext, bool, bool, ViewableActivationResult> procActivate)
        {
            ProcActivate = procActivate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyViewableActivator"/> class.
        /// </summary>
        public ProxyViewableActivator()
        {
        }

        /// <summary>
        /// Activates the specified agent instance context.
        /// </summary>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="isSubselect">if set to <c>true</c> [is subselect].</param>
        /// <param name="isRecoveringResilient">if set to <c>true</c> [is recovering resilient].</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public ViewableActivationResult Activate(
            AgentInstanceContext agentInstanceContext,
            bool isSubselect,
            bool isRecoveringResilient)
        {
            return ProcActivate.Invoke(agentInstanceContext, isSubselect, isRecoveringResilient);
        }
    }
}
