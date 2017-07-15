///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.script
{
    /// <summary>Context-partition local script context.</summary>
    public class AgentInstanceScriptContext : EPLScriptContext {
    
        private readonly EventBeanService eventBeanService;
        private IDictionary<string, Object> scriptProperties;
    
        private AgentInstanceScriptContext(EventBeanService eventBeanService) {
            this.eventBeanService = eventBeanService;
        }
    
        public static AgentInstanceScriptContext From(EventAdapterService eventAdapterService) {
            return new AgentInstanceScriptContext(eventAdapterService);
        }

        public EventBeanService EventBeanService
        {
            get { return eventBeanService; }
        }

        public void SetScriptAttribute(string attribute, Object value) {
            AllocateScriptProperties();
            scriptProperties.Put(attribute, value);
        }
    
        public Object GetScriptAttribute(string attribute) {
            AllocateScriptProperties();
            return ScriptProperties.Get(attribute);
        }
    
        private void AllocateScriptProperties() {
            if (scriptProperties == null) {
                scriptProperties = new HashMap<>();
            }
        }
    }
} // end of namespace
