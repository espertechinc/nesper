///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.path;

namespace com.espertech.esper.common.@internal.context.module
{
    public class EPModuleVariableInitServicesImpl : EPModuleVariableInitServices
    {
        public EPModuleVariableInitServicesImpl(
            VariableCollector variableCollector, EventTypeResolver eventTypeResolver)
        {
            VariableCollector = variableCollector;
            EventTypeResolver = eventTypeResolver;
        }

        public VariableCollector VariableCollector { get; }

        public EventTypeResolver EventTypeResolver { get; }
    }
} // end of namespace