///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public interface AggregationMethodForge
    {
        Type ResultType { get; }

        CodegenExpression CodegenCreateReader(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);
    }

    public class ProxyAggregationMethodForge : AggregationMethodForge
    {
        public Func<Type> ProcResultType { get; set; }
        public Func<CodegenMethodScope, SAIFFInitializeSymbol, CodegenClassScope, CodegenExpression> ProcCodegenCreateReader { get; set; }

        public ProxyAggregationMethodForge()
        {
        }

        public ProxyAggregationMethodForge(
            Func<Type> procResultType,
            Func<CodegenMethodScope, SAIFFInitializeSymbol, CodegenClassScope, CodegenExpression> procCodegenCreateReader)
        {
            ProcResultType = procResultType;
            ProcCodegenCreateReader = procCodegenCreateReader;
        }

        public Type ResultType => ProcResultType();

        public CodegenExpression CodegenCreateReader(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope) => ProcCodegenCreateReader(parent, symbols, classScope);
    }
} // end of namespace