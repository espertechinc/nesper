///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
    public class GraphTypeDesc
    {
        public GraphTypeDesc()
        {
        }

        public GraphTypeDesc(
            bool wildcard,
            bool underlying,
            EventType eventType)
        {
            IsWildcard = wildcard;
            IsUnderlying = underlying;
            EventType = eventType;
        }

        public bool IsWildcard { get; set; }

        public bool IsUnderlying { get; set; }

        public EventType EventType { get; set; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return new SAIFFInitializeBuilder(typeof(GraphTypeDesc), GetType(), "gtd", parent, symbols, classScope)
                .Constant("wildcard", IsWildcard)
                .Constant("underlying", IsUnderlying)
                .Eventtype("eventType", EventType)
                .Build();
        }
    }
} // end of namespace