///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
    public class DataInputOutputSerdeForgeEventSerde : DataInputOutputSerdeForge
    {
        private readonly DataInputOutputSerdeForgeEventSerdeMethod method;
        private readonly EventType eventType;
        private readonly Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression>[] functions;

        public DataInputOutputSerdeForgeEventSerde(
            DataInputOutputSerdeForgeEventSerdeMethod method,
            EventType eventType,
            params Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression>[] functions)
        {
            this.method = method;
            this.eventType = eventType;
            this.functions = functions;
        }

        public string ForgeClassName => nameof(DataInputOutputSerde);

        public EventType EventType => eventType;

        public DataInputOutputSerdeForgeEventSerdeMethod Method => method;

        public CodegenExpression Codegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression optionalEventTypeResolver)
        {
            var @params = new CodegenExpression[functions.Length];
            var vars =
                new DataInputOutputSerdeForgeParameterizedVars(method, classScope, optionalEventTypeResolver);
            for (var i = 0; i < @params.Length; i++) {
                @params[i] = functions[i].Invoke(vars);
            }

            return ExprDotMethodChain(optionalEventTypeResolver)
                .Get(EventTypeResolverConstants.EVENTSERDEFACTORY)
                .Add(this.method.GetName(), @params);
        }
    }
} // end of namespace