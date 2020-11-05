///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementExprDotMethod : CodegenStatementBase
    {
        private readonly CodegenExpression _expression;
        private readonly string _method;
        private readonly CodegenExpression[] _params;

        public CodegenStatementExprDotMethod(
            CodegenExpression expression,
            string method,
            CodegenExpression[] @params)
        {
            _expression = expression;
            _method = method;
            _params = @params;
        }

        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
            if (_expression is CodegenExpressionRef) {
                _expression.Render(builder, isInnerClass, 1, new CodegenIndent(true));
            }
            else {
                builder.Append("(");
                _expression.Render(builder, isInnerClass, 1, new CodegenIndent(true));
                builder.Append(")");
            }

            builder.Append('.').Append(_method).Append("(");
            RenderExpressions(builder, _params, isInnerClass);
            builder.Append(")");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            _expression.MergeClasses(classes);
            MergeClassesExpressions(classes, _params);
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
        }
    }
} // end of namespace