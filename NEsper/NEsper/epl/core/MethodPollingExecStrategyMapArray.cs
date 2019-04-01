///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.variable;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core
{
    public class MethodPollingExecStrategyMapArray : MethodPollingExecStrategyBaseArray
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MethodPollingExecStrategyMapArray(EventAdapterService eventAdapterService, FastMethod method, EventType eventType, Object invocationTarget, MethodPollingExecStrategyEnum strategy, VariableReader variableReader, String variableName, VariableService variableService)
            : base(eventAdapterService, method, eventType, invocationTarget, strategy, variableReader, variableName, variableService)
        {
        }
    
        protected override EventBean GetEventBean(Object value)
        {
            try
            {
                var valueDataMap = value.UnwrapStringDictionary();
                return EventAdapterService.AdapterForTypedMap(valueDataMap, EventType);
            }
            catch (ArgumentException)
            {
                Log.Warn("Expected map-type for value, but received type '" + value.GetType().GetCleanName() + "'");
                throw new EPException("Expected map-type for value, but received type '" + value.GetType().GetCleanName() + "'");
            }
        }
    }
}
