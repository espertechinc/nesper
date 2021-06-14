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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.view.expression
{
    /// <summary>
    ///     Factory for <seealso cref="ExpressionBatchView" />.
    /// </summary>
    public class ExpressionBatchViewForge : ExpressionViewForgeBase,
        DataWindowBatchingViewForge
    {
        internal bool includeTriggeringEvent = true;

        public override string ViewName => "Expression-batch";

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            if (parameters.Count != 1 && parameters.Count != 2) {
                var errorMessage =
                    ViewName + " view requires a single expression as a parameter, or an expression and boolean flag";
                throw new ViewParameterException(errorMessage);
            }

            ExpiryExpression = parameters[0];

            if (parameters.Count > 1) {
                var result = ViewForgeSupport.EvaluateAssertNoProperties(ViewName, parameters[1], 1);
                includeTriggeringEvent = (bool) result;
            }
        }

        internal override Type TypeOfFactory()
        {
            return typeof(ExpressionBatchViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "Exprbatch";
        }

        internal override void MakeSetters(
            CodegenExpressionRef factory,
            CodegenBlock block)
        {
            block.SetProperty(factory, "IncludeTriggeringEvent", Constant(includeTriggeringEvent));
        }
    }
} // end of namespace