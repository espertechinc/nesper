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
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionIncrementDecrement : CodegenExpression
    {
        private readonly bool _increment;

        private readonly CodegenExpression _expr;

        public CodegenExpressionIncrementDecrement(
            CodegenExpression expr,
            bool increment)
        {
            _expr = expr;
            _increment = increment;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            _expr.Render(builder, isInnerClass, level, indent);
            builder.Append(_increment ? "++" : "--");
        }

        public void MergeClasses(ISet<Type> classes)
        {
        }
        
        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
        }
    }
} // end of namespace