///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.client.hook.vdw
{
    /// <summary>
    /// Context for use with virtual data window forge <seealso cref="com.espertech.esper.common.client.hook.vdw.VirtualDataWindowForge" /> provides
    /// contextual information about the named window and the type of events held.
    /// </summary>
    public class VirtualDataWindowForgeContext
    {
        private readonly EventType eventType;
        private readonly object[] parameters;
        private readonly ExprNode[] parameterExpressions;
        private readonly string namedWindowName;
        private readonly ViewForgeEnv viewForgeEnv;
        private readonly object customConfiguration;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventType">the event type that the named window is declared to hold.</param>
        /// <param name="parameters">the parameters passed when declaring the named window, for example "create window ABC.my:vdw("10.0.0.1")" passes one paramater here.</param>
        /// <param name="namedWindowName">the name of the named window</param>
        /// <param name="parameterExpressions">parameter expressions passed to the virtual data window</param>
        /// <param name="customConfiguration">additional configuration</param>
        /// <param name="viewForgeEnv">view forge environment</param>
        public VirtualDataWindowForgeContext(
            EventType eventType,
            object[] parameters,
            ExprNode[] parameterExpressions,
            string namedWindowName,
            ViewForgeEnv viewForgeEnv,
            object customConfiguration)
        {
            this.eventType = eventType;
            this.parameters = parameters;
            this.parameterExpressions = parameterExpressions;
            this.namedWindowName = namedWindowName;
            this.viewForgeEnv = viewForgeEnv;
            this.customConfiguration = customConfiguration;
        }

        /// <summary>
        /// Returns the event type of the events held in the virtual data window as per declaration of the named window.
        /// </summary>
        /// <returns>event type</returns>
        public EventType EventType {
            get => eventType;
        }

        /// <summary>
        /// Returns the parameters passed; for example "create window ABC.my:vdw("10.0.0.1")" passes one paramater here.
        /// </summary>
        /// <returns>parameters</returns>
        public object[] Parameters {
            get => parameters;
        }

        /// <summary>
        /// Returns the name of the named window used in connection with the virtual data window.
        /// </summary>
        /// <returns>named window</returns>
        public string NamedWindowName {
            get => namedWindowName;
        }

        /// <summary>
        /// Returns the expressions passed as parameters to the virtual data window.
        /// </summary>
        /// <returns>parameter expressions</returns>
        public ExprNode[] ParameterExpressions {
            get => parameterExpressions;
        }

        /// <summary>
        /// Returns any additional configuration provided.
        /// </summary>
        /// <returns>additional config</returns>
        public object CustomConfiguration {
            get => customConfiguration;
        }

        /// <summary>
        /// Returns the view forge environment
        /// </summary>
        /// <returns>view forge environment</returns>
        public ViewForgeEnv ViewForgeEnv {
            get => viewForgeEnv;
        }
    }
} // end of namespace