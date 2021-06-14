///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.script.core
{
    /// <summary>
    ///     Context-partition local script context.
    /// </summary>
    public class AgentInstanceScriptContext : EPLScriptContext
    {
        private IDictionary<string, object> scriptProperties;

        private readonly StatementContext statementContext;

        public AgentInstanceScriptContext(StatementContext statementContext)
        {
            this.statementContext = statementContext;
        }

        public EventBeanService EventBeanService => statementContext.EventBeanService;

        public void SetScriptAttribute(
            string attribute,
            object value)
        {
            AllocateScriptProperties();
            scriptProperties.Put(attribute, value);
        }

        public object GetScriptAttribute(string attribute)
        {
            AllocateScriptProperties();
            return scriptProperties.Get(attribute);
        }

        private void AllocateScriptProperties()
        {
            if (scriptProperties == null) {
                scriptProperties = new Dictionary<string, object>();
            }
        }

        public static AgentInstanceScriptContext From(StatementContext statementContext)
        {
            return new AgentInstanceScriptContext(statementContext);
        }
    }
} // end of namespace