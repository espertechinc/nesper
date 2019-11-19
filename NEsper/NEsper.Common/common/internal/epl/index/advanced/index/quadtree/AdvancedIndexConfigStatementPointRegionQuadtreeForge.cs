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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class AdvancedIndexConfigStatementPointRegionQuadtreeForge : EventAdvancedIndexConfigStatementForge
    {
        public AdvancedIndexConfigStatementPointRegionQuadtreeForge(
            ExprForge xEval,
            ExprForge yEval)
        {
            XEval = xEval;
            YEval = yEval;
        }

        public ExprForge XEval { get; }

        public ExprForge YEval { get; }

        public CodegenExpression CodegenMake(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(AdvancedIndexConfigStatementPointRegionQuadtree),
                GetType(),
                classScope);
            Func<ExprForge, CodegenExpression> expr = forge =>
                ExprNodeUtilityCodegen.CodegenEvaluator(forge, method, GetType(), classScope);
            method.Block
                .DeclareVar<AdvancedIndexConfigStatementPointRegionQuadtree>(
                    "factory",
                    NewInstance(typeof(AdvancedIndexConfigStatementPointRegionQuadtree)))
                .SetProperty(Ref("factory"), "XEval", expr.Invoke(XEval))
                .SetProperty(Ref("factory"), "YEval", expr.Invoke(YEval))
                .MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }

        public EventAdvancedIndexConfigStatement ToRuntime()
        {
            var cfg = new AdvancedIndexConfigStatementPointRegionQuadtree();
            cfg.XEval = XEval.ExprEvaluator;
            cfg.YEval = YEval.ExprEvaluator;
            return cfg;
        }
    }
} // end of namespace