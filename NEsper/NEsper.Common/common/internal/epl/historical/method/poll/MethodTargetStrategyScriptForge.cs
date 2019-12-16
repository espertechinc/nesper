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
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public class MethodTargetStrategyScriptForge : MethodTargetStrategyForge
    {
        private readonly ExprNodeScript script;

        public MethodTargetStrategyScriptForge(ExprNodeScript script)
        {
            this.script = script;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(MethodTargetStrategyScript), this.GetType(), classScope);
            method.Block
                .DeclareVar<MethodTargetStrategyScript>("target", NewInstance(typeof(MethodTargetStrategyScript)))
                .SetProperty(Ref("target"), "ScriptEvaluator", script.GetField(classScope))
                .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add("AddReadyCallback", Ref("target")))
                .MethodReturn(Ref("target"));
            return LocalMethod(method);
        }
    }
} // end of namespace