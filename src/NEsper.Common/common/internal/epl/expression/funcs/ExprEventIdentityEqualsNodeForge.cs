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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprEventIdentityEqualsNodeForge : ExprForge
    {
        private readonly ExprEventIdentityEqualsNode node;

        public ExprEventIdentityEqualsNodeForge(
            ExprEventIdentityEqualsNode node,
            ExprStreamUnderlyingNode undLeft,
            ExprStreamUnderlyingNode undRight)
        {
            this.node = node;
            UndLeft = undLeft;
            UndRight = undRight;
        }

        public ExprStreamUnderlyingNode UndLeft { get; }

        public ExprStreamUnderlyingNode UndRight { get; }

        public Type EvaluationType => typeof(bool?);

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return ExprEventIdentityEqualsNodeEval.EvaluateCodegen(this, parent, symbols, classScope);
        }

        public ExprEvaluator ExprEvaluator => new ExprEventIdentityEqualsNodeEval(UndLeft.StreamId, UndRight.StreamId);

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public ExprNodeRenderable ExprForgeRenderable => node;
    }
} // end of namespace