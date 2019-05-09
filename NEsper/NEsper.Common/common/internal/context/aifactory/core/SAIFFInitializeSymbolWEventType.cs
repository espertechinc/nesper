///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.core
{
    public class SAIFFInitializeSymbolWEventType : SAIFFInitializeSymbol
    {
        public readonly static CodegenExpressionRef REF_EVENTTYPE = @Ref("eventType");

        private CodegenExpressionRef optionalEventTypeRef;

        public CodegenExpressionRef GetAddEventType(CodegenMethodScope scope)
        {
            if (optionalEventTypeRef == null) {
                optionalEventTypeRef = REF_EVENTTYPE;
            }

            scope.AddSymbol(optionalEventTypeRef);
            return optionalEventTypeRef;
        }

        public override void Provide(IDictionary<string, Type> symbols)
        {
            base.Provide(symbols);
            if (optionalEventTypeRef != null) {
                symbols.Put(optionalEventTypeRef.Ref, typeof(EventType));
            }
        }
    }
} // end of namespace