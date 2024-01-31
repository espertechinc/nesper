///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.client.hook.vdw
{
    /// <summary>
    ///     Context for use with virtual data window factory <seealso cref="VirtualDataWindowFactory" /> provides
    ///     contextual information as well as the handle for posting insert and remove streams and factory for event bean
    ///     instances.
    /// </summary>
    public class VirtualDataWindowFactoryContext
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventType">event type</param>
        /// <param name="parameters">parameter values</param>
        /// <param name="parameterExpressions">parameter expressions</param>
        /// <param name="namedWindowName">named window name</param>
        /// <param name="customConfiguration">custom configuration object that is passed along</param>
        /// <param name="viewFactoryContext">view context</param>
        /// <param name="services">services</param>
        public VirtualDataWindowFactoryContext(
            EventType eventType,
            object[] parameters,
            ExprEvaluator[] parameterExpressions,
            string namedWindowName,
            object customConfiguration,
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
            EventType = eventType;
            Parameters = parameters;
            ParameterExpressions = parameterExpressions;
            NamedWindowName = namedWindowName;
            CustomConfiguration = customConfiguration;
            ViewFactoryContext = viewFactoryContext;
            Services = services;
        }

        /// <summary>
        ///     Returns the event type
        /// </summary>
        /// <returns>event type</returns>
        public EventType EventType { get; }

        /// <summary>
        ///     Returns parameters
        /// </summary>
        /// <returns>parameters</returns>
        public object[] Parameters { get; }

        /// <summary>
        ///     Returns parameter expressions as expression evaluators
        /// </summary>
        /// <returns>expression evaluators</returns>
        public ExprEvaluator[] ParameterExpressions { get; }

        /// <summary>
        ///     Returns the named window name
        /// </summary>
        /// <returns>named window name</returns>
        public string NamedWindowName { get; }

        /// <summary>
        ///     Returns the view factory context
        /// </summary>
        /// <returns>view factory context</returns>
        public ViewFactoryContext ViewFactoryContext { get; }

        /// <summary>
        ///     Returns initialization-time services
        /// </summary>
        /// <returns>services</returns>
        public EPStatementInitServices Services { get; }

        /// <summary>
        ///     Returns the custom configuration object that gets passed along
        /// </summary>
        /// <returns>configuration object</returns>
        public object CustomConfiguration { get; }
    }
} // end of namespace