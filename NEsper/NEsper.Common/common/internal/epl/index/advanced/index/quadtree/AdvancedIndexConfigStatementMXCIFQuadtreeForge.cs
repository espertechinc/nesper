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
    public class AdvancedIndexConfigStatementMXCIFQuadtreeForge : EventAdvancedIndexConfigStatementForge
    {
        public AdvancedIndexConfigStatementMXCIFQuadtreeForge(
            ExprForge xEval,
            ExprForge yEval,
            ExprForge widthEval,
            ExprForge heightEval)
        {
            XEval = xEval;
            YEval = yEval;
            WidthEval = widthEval;
            HeightEval = heightEval;
        }

        public ExprForge XEval { get; set; }

        public ExprForge YEval { get; set; }

        public ExprForge WidthEval { get; set; }

        public ExprForge HeightEval { get; set; }

        public CodegenExpression CodegenMake(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AdvancedIndexConfigStatementMXCIFQuadtree), GetType(), classScope);
            Func<ExprForge, CodegenExpression> expr = forge =>
                ExprNodeUtilityCodegen.CodegenEvaluator(forge, method, GetType(), classScope);
            method.Block
                .DeclareVar<AdvancedIndexConfigStatementMXCIFQuadtree>(
                    "factory",
                    NewInstance(typeof(AdvancedIndexConfigStatementMXCIFQuadtree)))
                .SetProperty(Ref("factory"), "xEval", expr.Invoke(XEval))
                .SetProperty(Ref("factory"), "yEval", expr.Invoke(YEval))
                .SetProperty(Ref("factory"), "WidthEval", expr.Invoke(WidthEval))
                .SetProperty(Ref("factory"), "HeightEval", expr.Invoke(HeightEval))
                .MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }

        public EventAdvancedIndexConfigStatement ToRuntime()
        {
            var config = new AdvancedIndexConfigStatementMXCIFQuadtree();
            config.XEval = XEval.ExprEvaluator;
            config.YEval = XEval.ExprEvaluator;
            config.WidthEval = WidthEval.ExprEvaluator;
            config.HeightEval = HeightEval.ExprEvaluator;
            return config;
        }
    }
} // end of namespace