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

namespace com.espertech.esper.epl.script
{
    /// <summary>Context-partition local script context. </summary>
    public class AgentInstanceScriptContext : EPLScriptContext
    {
        private readonly IDictionary<String, Object> _scriptProperties = new Dictionary<String, Object>();

        public void SetScriptAttribute(String attribute, Object value)
        {
            _scriptProperties.Put(attribute, value);
        }

        public Object GetScriptAttribute(String attribute)
        {
            return _scriptProperties.Get(attribute);
        }
    }
}