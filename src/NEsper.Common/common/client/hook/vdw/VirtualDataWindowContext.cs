///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.client.hook.vdw
{
    /// <summary>
    /// Context for use with virtual data window factory <seealso cref="VirtualDataWindowFactory" /> provides
    /// contextual information about the named window and the type of events held,
    /// handle for posting insert and remove streams and factory for event bean instances.
    /// </summary>
    public class VirtualDataWindowContext
    {
        private readonly VirtualDWViewFactory factory;
        private readonly AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext;
        private readonly EventBeanFactory eventBeanFactory;
        private readonly VirtualDataWindowOutStreamImpl outputStream;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="factory">factory</param>
        /// <param name="agentInstanceViewFactoryContext">context</param>
        /// <param name="eventBeanFactory">event bean factory</param>
        /// <param name="outputStream">output stream</param>
        public VirtualDataWindowContext(
            VirtualDWViewFactory factory,
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
            EventBeanFactory eventBeanFactory,
            VirtualDataWindowOutStreamImpl outputStream)
        {
            this.factory = factory;
            this.agentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            this.eventBeanFactory = eventBeanFactory;
            this.outputStream = outputStream;
        }

        /// <summary>
        /// Returns the statement context which holds statement information (name, expression, id) and statement-level services.
        /// </summary>
        /// <returns>statement context</returns>
        public StatementContext StatementContext =>
            agentInstanceViewFactoryContext.AgentInstanceContext.StatementContext;

        /// <summary>
        /// Returns the event type of the events held in the virtual data window as per declaration of the named window.
        /// </summary>
        /// <returns>event type</returns>
        public EventType EventType => factory.EventType;

        /// <summary>
        /// Returns the parameters passed; for example "create window ABC.my:vdw("10.0.0.1")" passes one paramater here.
        /// </summary>
        /// <returns>parameters</returns>
        public object[] Parameters => factory.Parameters;

        /// <summary>
        /// Returns the factory for creating instances of EventBean from rows.
        /// </summary>
        /// <returns>event bean factory</returns>
        public EventBeanFactory EventFactory => eventBeanFactory;

        /// <summary>
        /// Returns a handle for use to send insert and remove stream data to consuming statements.
        /// <para />Typically use "context.getOutputStream().update(newData, oldData);" in the update method of the virtual data window.
        /// </summary>
        /// <returns>handle for posting insert and remove stream</returns>
        public VirtualDataWindowOutStream OutputStream => outputStream;

        /// <summary>
        /// Returns the name of the named window used in connection with the virtual data window.
        /// </summary>
        /// <returns>named window</returns>
        public string NamedWindowName => factory.NamedWindowName;

        /// <summary>
        /// Returns the expressions passed as parameters to the virtual data window.
        /// </summary>
        /// <returns>parameter expressions</returns>
        public ExprEvaluator[] ParameterExpressions => factory.ParameterExpressions;

        /// <summary>
        /// Returns the agent instance (context partition) context.
        /// </summary>
        /// <returns>context</returns>
        public AgentInstanceContext AgentInstanceContext => agentInstanceViewFactoryContext.AgentInstanceContext;

        /// <summary>
        /// Returns the factory
        /// </summary>
        /// <returns>factory</returns>
        public VirtualDWViewFactory Factory => factory;

        /// <summary>
        /// Returns the agent instance context
        /// </summary>
        /// <returns>agent instance context</returns>
        public AgentInstanceViewFactoryChainContext AgentInstanceViewFactoryContext => agentInstanceViewFactoryContext;
    }
} // end of namespace