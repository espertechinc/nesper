///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.table
{
	public class AggregationServiceFactoryForgeTable : AggregationServiceFactoryForgeWProviderGen {
	    private readonly TableMetaData metadata;
	    private readonly TableColumnMethodPairForge[] methodPairs;
	    private readonly int[] accessColumnsZeroOffset;
	    private readonly AggregationAgentForge[] accessAgents;
	    private readonly AggregationGroupByRollupDesc groupByRollupDesc;

	    public AggregationServiceFactoryForgeTable(TableMetaData metadata, TableColumnMethodPairForge[] methodPairs, int[] accessColumnsZeroOffset, AggregationAgentForge[] accessAgents, AggregationGroupByRollupDesc groupByRollupDesc) {
	        this.metadata = metadata;
	        this.methodPairs = methodPairs;
	        this.accessColumnsZeroOffset = accessColumnsZeroOffset;
	        this.accessAgents = accessAgents;
	        this.groupByRollupDesc = groupByRollupDesc;
	    }

	    public CodegenExpression MakeProvider(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	        CodegenMethod method = parent.MakeChild(typeof(AggregationServiceFactoryTable), this.GetType(), classScope);
	        method.Block
	                .DeclareVar(typeof(AggregationServiceFactoryTable), "factory", NewInstance(typeof(AggregationServiceFactoryTable)))
	                .ExprDotMethod(@Ref("factory"), "setTable", TableDeployTimeResolver.MakeResolveTable(metadata, symbols.GetAddInitSvc(method)))
	                .ExprDotMethod(@Ref("factory"), "setMethodPairs", TableColumnMethodPairForge.MakeArray(methodPairs, method, symbols, classScope))
	                .ExprDotMethod(@Ref("factory"), "setAccessColumnsZeroOffset", Constant(accessColumnsZeroOffset))
	                .ExprDotMethod(@Ref("factory"), "setAccessAgents", AggregationAgentUtil.MakeArray(accessAgents, method, symbols, classScope))
	                .ExprDotMethod(@Ref("factory"), "setGroupByRollupDesc", groupByRollupDesc == null ? ConstantNull() : groupByRollupDesc.Codegen())
	                .MethodReturn(@Ref("factory"));
	        return LocalMethod(method);
	    }
	}
} // end of namespace