///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.resultset.select.typable
{
    public class SelectExprProcessorTypableMapForge : SelectExprProcessorTypableForge
    {
        internal readonly EventType mapType;
        internal readonly ExprForge innerForge;

        public SelectExprProcessorTypableMapForge(
            EventType mapType,
            ExprForge innerForge)
        {
            this.mapType = mapType;
            this.innerForge = innerForge;
        }

        public ExprEvaluator ExprEvaluator => new SelectExprProcessorTypableMapEval(this);

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return SelectExprProcessorTypableMapEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public Type UnderlyingEvaluationType => typeof(IDictionary<object, object>);

        public Type EvaluationType => typeof(EventBean);

        public ExprForge InnerForge => innerForge;

        public ExprNodeRenderable ExprForgeRenderable => innerForge.ExprForgeRenderable;

        public EventType MapType => mapType;
    }
} // end of namespace