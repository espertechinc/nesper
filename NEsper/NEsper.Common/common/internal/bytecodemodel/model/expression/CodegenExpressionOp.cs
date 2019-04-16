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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionOp : CodegenExpression
    {
        private readonly string _expressionText;
        private readonly CodegenExpression _left;
        private readonly CodegenExpression _right;

        public CodegenExpressionOp(
            CodegenExpression left,
            string expressionText,
            CodegenExpression right)
        {
            _left = left;
            _expressionText = expressionText;
            _right = right;
        }

        public void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass)
        {
            builder.Append("(");
            _left.Render(builder, imports, isInnerClass);
            builder.Append(_expressionText);
            _right.Render(builder, imports, isInnerClass);
            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _left.MergeClasses(classes);
            _right.MergeClasses(classes);
        }
    }
} // end of namespace