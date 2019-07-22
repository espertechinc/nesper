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
    public class CodegenExpressionArrayAtIndex : CodegenExpression
    {
        private readonly CodegenExpression _expression;
        private readonly CodegenExpression _index;

        public CodegenExpressionArrayAtIndex(
            CodegenExpression expression,
            CodegenExpression index)
        {
            this._expression = expression;
            this._index = index;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass)
        {
            _expression.Render(builder, isInnerClass);
            builder.Append("[");
            _index.Render(builder, isInnerClass);
            builder.Append("]");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _expression.MergeClasses(classes);
            _index.MergeClasses(classes);
        }
    }
} // end of namespace