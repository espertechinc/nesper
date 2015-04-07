///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// Context for use with virtual data window factory <seealso cref="VirtualDataWindowFactory" /> provides contextual 
    /// information about the named window and the type of events held, handle for posting insert and remove streams and 
    /// factory for event bean instances.
    /// </summary>
    public class VirtualDataWindowContext
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="agentInstanceContext">statement services and statement information such as statement name, statement id, EPL expression</param>
        /// <param name="eventType">the event type that the named window is declared to hold.</param>
        /// <param name="parameters">the parameters passed when declaring the named window, for example "create window ABC.my:vdw("10.0.0.1")" passes one paramater here.</param>
        /// <param name="parameterExpressions">parameter expressions passed to the virtual data window</param>
        /// <param name="eventFactory">factory for converting row objects to EventBean instances</param>
        /// <param name="outputStream">forward the input and output stream received from the Update method here</param>
        /// <param name="namedWindowName">the name of the named window</param>
        /// <param name="viewFactoryContext">context of services</param>
        /// <param name="customConfiguration">additional configuration</param>
        public VirtualDataWindowContext(AgentInstanceContext agentInstanceContext,
                                        EventType eventType,
                                        Object[] parameters,
                                        ExprNode[] parameterExpressions,
                                        EventBeanFactory eventFactory,
                                        VirtualDataWindowOutStream outputStream,
                                        String namedWindowName,
                                        ViewFactoryContext viewFactoryContext,
                                        Object customConfiguration)
        {
            AgentInstanceContext = agentInstanceContext;
            EventType = eventType;
            Parameters = parameters;
            ParameterExpressions = parameterExpressions;
            EventFactory = eventFactory;
            OutputStream = outputStream;
            NamedWindowName = namedWindowName;
            ViewFactoryContext = viewFactoryContext;
            CustomConfiguration = customConfiguration;
        }

        /// <summary>Returns the statement context which holds statement information (name, expression, id) and statement-level services. </summary>
        /// <value>statement context</value>
        public StatementContext StatementContext
        {
            get { return AgentInstanceContext.StatementContext; }
        }

        /// <summary>Returns the event type of the events held in the virtual data window as per declaration of the named window. </summary>
        /// <value>event type</value>
        public EventType EventType { get; private set; }

        /// <summary>Returns the parameters passed; for example "create window ABC.my:vdw("10.0.0.1")" passes one paramater here. </summary>
        /// <value>parameters</value>
        public object[] Parameters { get; private set; }

        /// <summary>Returns the factory for creating instances of EventBean from rows. </summary>
        /// <value>event bean factory</value>
        public EventBeanFactory EventFactory { get; private set; }

        /// <summary>Returns a handle for use to send insert and remove stream data to consuming statements. <para /> Typically use "context.getOutputStream().Update(newData, oldData);" in the Update method of the virtual data window. </summary>
        /// <value>handle for posting insert and remove stream</value>
        public VirtualDataWindowOutStream OutputStream { get; private set; }

        /// <summary>Returns the name of the named window used in connection with the virtual data window. </summary>
        /// <value>named window</value>
        public string NamedWindowName { get; private set; }

        /// <summary>Returns the expressions passed as parameters to the virtual data window. </summary>
        /// <value>parameter expressions</value>
        public ExprNode[] ParameterExpressions { get; private set; }

        /// <summary>Returns the engine services context. </summary>
        /// <value>engine services context</value>
        public ViewFactoryContext ViewFactoryContext { get; private set; }

        /// <summary>Returns any additional configuration provided. </summary>
        /// <value>additional config</value>
        public object CustomConfiguration { get; private set; }

        /// <summary>Returns the agent instance (context partition) context. </summary>
        /// <value>context</value>
        public AgentInstanceContext AgentInstanceContext { get; private set; }
    }
}