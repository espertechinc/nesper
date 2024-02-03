///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.epl.expression.codegen
{
    public class EventTypeWithOptionalFlag
    {
        public EventTypeWithOptionalFlag(
            CodegenExpressionRef @ref,
            EventType eventType,
            bool optionalEvent)
        {
            Ref = @ref;
            EventType = eventType;
            IsOptionalEvent = optionalEvent;
        }

        public CodegenExpressionRef Ref { get; }

        public EventType EventType { get; }

        public bool IsOptionalEvent { get; }
    }
} // end of namespace