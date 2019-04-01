///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
	public class AgregationTAAReaderCountMinSketchTopKForge : AggregationTableAccessAggReaderForge {

	    public Type ResultType {
	        get => typeof(CountMinSketchTopK[]);
	    }

	    public CodegenExpression CodegenCreateReader(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	        CodegenMethod method = parent.MakeChild(typeof(AgregationTAAReaderCountMinSketchTopK), this.GetType(), classScope);
	        method.Block
	                .DeclareVar(typeof(AgregationTAAReaderCountMinSketchTopK), "strat", NewInstance(typeof(AgregationTAAReaderCountMinSketchTopK)))
	                .MethodReturn(@Ref("strat"));
	        return LocalMethod(method);
	    }
	}
} // end of namespace