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
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.script
{
    /// <summary>Context-partition local script context.</summary>
    public class AgentInstanceScriptContext : EPLScriptContext
    {
        private readonly EventBeanService _eventBeanService;
        private IDictionary<string, Object> _scriptProperties;

        private AgentInstanceScriptContext(EventBeanService eventBeanService)
        {
            _eventBeanService = eventBeanService;
        }

        public static AgentInstanceScriptContext From(EventAdapterService eventAdapterService)
        {
            return new AgentInstanceScriptContext(eventAdapterService);
        }

        public EventBeanService EventBeanService
        {
            get { return _eventBeanService; }
        }

        public void SetScriptAttribute(string attribute, Object value)
        {
            AllocateScriptProperties();
            _scriptProperties.Put(attribute, value);
        }

        public Object GetScriptAttribute(string attribute)
        {
            AllocateScriptProperties();
            return _scriptProperties.Get(attribute);
        }

        private void AllocateScriptProperties()
        {
            if (_scriptProperties == null)
            {
                _scriptProperties = new Dictionary<string, object>();
            }
        }
    }
} // end of namespace
