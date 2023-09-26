///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.view.expression
{
    public class ExpressionWindowViewForge : ExpressionViewForgeBase
    {
        public override string ViewName => "Expression";

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            if (parameters.Count != 1) {
                var errorMessage = ViewName + " view requires a single expression as a parameter";
                throw new ViewParameterException(errorMessage);
            }

            ExpiryExpression = parameters[0];
        }

        internal override Type TypeOfFactory => typeof(ExpressionWindowViewFactory);

        protected override void MakeSetters(
            CodegenExpressionRef factory,
            CodegenBlock block)
        {
        }

        internal override string FactoryMethod => "Expr";

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_EXPRESSION;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
} // end of namespace