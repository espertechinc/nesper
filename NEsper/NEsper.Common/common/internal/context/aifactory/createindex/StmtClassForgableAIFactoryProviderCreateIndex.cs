///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.context.aifactory.core.SAIFFInitializeSymbol;

namespace com.espertech.esper.common.@internal.context.aifactory.createindex
{
	public class StmtClassForgableAIFactoryProviderCreateIndex : StmtClassForgableAIFactoryProviderBase {
	    private readonly StatementAgentInstanceFactoryCreateIndexForge forge;

	    public StmtClassForgableAIFactoryProviderCreateIndex(string className, CodegenPackageScope packageScope, StatementAgentInstanceFactoryCreateIndexForge forge)

	    	 : base(className, packageScope)

	    {
	        this.forge = forge;
	    }

	    protected override Type TypeOfFactory() {
	        return typeof(StatementAgentInstanceFactoryCreateIndex);
	    }

	    protected override CodegenMethod CodegenConstructorInit(CodegenMethodScope parent, CodegenClassScope classScope) {
	        SAIFFInitializeSymbol saiffInitializeSymbol = new SAIFFInitializeSymbol();
	        CodegenMethod method = parent.MakeChildWithScope(TypeOfFactory(), this.GetType(), saiffInitializeSymbol, classScope).AddParam(typeof(EPStatementInitServices), REF_STMTINITSVC.Ref);
	        method.Block.MethodReturn(LocalMethod(forge.InitializeCodegen(method, saiffInitializeSymbol, classScope)));
	        return method;
	    }
	}
} // end of namespace