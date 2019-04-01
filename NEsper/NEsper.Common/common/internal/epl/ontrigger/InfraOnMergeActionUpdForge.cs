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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.updatehelper;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
	public class InfraOnMergeActionUpdForge : InfraOnMergeActionForge {
	    private readonly EventBeanUpdateHelperForge updateHelper;
	    private readonly TableMetaData table;

	    public InfraOnMergeActionUpdForge(ExprNode optionalFilter, EventBeanUpdateHelperForge updateHelper, TableMetaData table)

	    	 : base(optionalFilter)

	    {
	        this.updateHelper = updateHelper;
	        this.table = table;
	    }

	    protected override CodegenExpression Make(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	        CodegenMethod method = parent.MakeChild(typeof(InfraOnMergeActionUpd), this.GetType(), classScope);
	        if (table == null) {
	            method.Block.MethodReturn(NewInstance(typeof(InfraOnMergeActionUpd), MakeFilter(method, classScope), updateHelper.MakeWCopy(method, classScope)));
	        } else {
	            method.Block
	                    .DeclareVar(typeof(InfraOnMergeActionUpd), "upd", NewInstance(typeof(InfraOnMergeActionUpd), MakeFilter(method, classScope), updateHelper.MakeNoCopy(method, classScope),
	                            TableDeployTimeResolver.MakeResolveTable(table, symbols.GetAddInitSvc(method))))
	                    .ExprDotMethod(symbols.GetAddInitSvc(method), "addReadyCallback", @Ref("upd"))
	                    .MethodReturn(@Ref("upd"));
	        }

	        return LocalMethod(method);
	    }
	}
} // end of namespace