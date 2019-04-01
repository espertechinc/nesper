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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
	public class ExprMathNodeForge : ExprForgeInstrumentable {
	    private readonly ExprMathNode parent;
	    private readonly MathArithTypeEnum.Computer arithTypeEnumComputer;
	    private readonly Type resultType;

	    public ExprMathNodeForge(ExprMathNode parent, MathArithTypeEnum.Computer arithTypeEnumComputer, Type resultType) {
	        this.parent = parent;
	        this.arithTypeEnumComputer = arithTypeEnumComputer;
	        this.resultType = resultType;
	    }

	    public ExprEvaluator ExprEvaluator
	    {
	        get => new ExprMathNodeForgeEval(this, parent.ChildNodes[0].Forge.ExprEvaluator, parent.ChildNodes[1].Forge.ExprEvaluator);
	    }

	    public Type EvaluationType
	    {
	        get => resultType;
	    }

	    public CodegenExpression EvaluateCodegenUninstrumented(Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        CodegenMethod methodNode = ExprMathNodeForgeEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope, parent.ChildNodes[0], parent.ChildNodes[1]);
	        return LocalMethod(methodNode);
	    }

	    public CodegenExpression EvaluateCodegen(Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return new InstrumentationBuilderExpr(this.GetType(), this, "ExprMath", requiredType, codegenMethodScope, exprSymbol, codegenClassScope).Qparam(Constant(parent.MathArithTypeEnum.ExpressionText)).Build();
	    }

	    private MathArithTypeEnum.Computer ArithTypeEnumComputer {
	        get { return arithTypeEnumComputer; }
	    }

	    ExprNodeRenderable ExprForge.ForgeRenderable => ForgeRenderable;

	    public ExprMathNode ForgeRenderable
	    {
	        get => parent;
	    }

	    public ExprForgeConstantType ForgeConstantType
	    {
	        get => ExprForgeConstantType.NONCONST;
	    }
	}
} // end of namespace