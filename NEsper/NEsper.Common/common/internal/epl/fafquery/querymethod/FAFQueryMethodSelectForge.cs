///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.faf;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
	/// <summary>
	/// Starts and provides the stop method for EPL statements.
	/// </summary>
	public class FAFQueryMethodSelectForge : FAFQueryMethodForge {
	    private readonly FAFQueryMethodSelectDesc desc;
	    private readonly string classNameResultSetProcessor;
	    private readonly StatementRawInfo statementRawInfo;

	    public FAFQueryMethodSelectForge(FAFQueryMethodSelectDesc desc, string classNameResultSetProcessor, StatementRawInfo statementRawInfo) {
	        this.desc = desc;
	        this.classNameResultSetProcessor = classNameResultSetProcessor;
	        this.statementRawInfo = statementRawInfo;
	    }

	    public IList<StmtClassForgable> MakeForgables(string queryMethodProviderClassName, string classPostfix, CodegenPackageScope packageScope) {
	        IList<StmtClassForgable> forgables = new List<StmtClassForgable>();

	        // generate RSP
	        forgables.Add(new StmtClassForgableRSPFactoryProvider(classNameResultSetProcessor, desc.ResultSetProcessor, packageScope, statementRawInfo));

	        // generate faf-select
	        forgables.Add(new StmtClassForgableQueryMethodProvider(queryMethodProviderClassName, packageScope, this));

	        return forgables;
	    }

	    public void MakeMethod(CodegenMethod method, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	        CodegenExpressionRef select = @Ref("select");
	        method.Block
	                .DeclareVar(typeof(FAFQueryMethodSelect), select.Ref, NewInstance(typeof(FAFQueryMethodSelect)))
	                .ExprDotMethod(select, "setAnnotations", LocalMethod(AnnotationUtil.MakeAnnotations(typeof(Attribute[]), desc.Annotations, method, classScope)))
	                .ExprDotMethod(select, "setProcessors", FireAndForgetProcessorForge.MakeArray(desc.Processors, method, symbols, classScope))
	                .DeclareVar(classNameResultSetProcessor, "rsp", CodegenExpressionBuilder.NewInstance(classNameResultSetProcessor, symbols.GetAddInitSvc(method)))
	                .ExprDotMethod(select, "setResultSetProcessorFactoryProvider", @Ref("rsp"))
	                .ExprDotMethod(select, "setQueryGraph", desc.QueryGraph.Make(method, symbols, classScope))
	                .ExprDotMethod(select, "setWhereClause", desc.WhereClause == null ? ConstantNull() : ExprNodeUtilityCodegen.CodegenEvaluator(desc.WhereClause.Forge, method, this.GetType(), classScope))
	                .ExprDotMethod(select, "setJoinSetComposerPrototype", desc.Joins == null ? ConstantNull() : desc.Joins.Make(method, symbols, classScope))
	                .ExprDotMethod(select, "setConsumerFilters", ExprNodeUtilityCodegen.CodegenEvaluators(desc.ConsumerFilters, method, this.GetType(), classScope))
	                .ExprDotMethod(select, "setContextName", Constant(desc.ContextName))
	                .ExprDotMethod(select, "setTableAccesses", ExprTableEvalStrategyUtil.CodegenInitMap(desc.TableAccessForges, this.GetType(), method, symbols, classScope))
	                .ExprDotMethod(select, "setHasTableAccess", Constant(desc.HasTableAccess))
	                .ExprDotMethod(select, "setDistinct", Constant(desc.IsDistinct))
	                .MethodReturn(select);
	    }
	}
} // end of namespace