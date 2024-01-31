///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    public class VariableTriggerWriteDesc : VariableTriggerWrite
    {
        public string VariableName { get; set; }

        public EventPropertyWriter Writer { get; set; }

        public EventType Type { get; set; }

        public EventPropertyValueGetter Getter { get; set; }
    }
} // end of namespace