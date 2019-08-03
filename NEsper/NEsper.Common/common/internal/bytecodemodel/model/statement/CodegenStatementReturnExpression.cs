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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementReturnExpression : CodegenStatementBase,
        CodegenStatement
    {
        private readonly CodegenExpression expression;

        public CodegenStatementReturnExpression(CodegenExpression expression)
        {
            this.expression = expression ?? throw new ArgumentException("No expression provided");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            expression.MergeClasses(classes);
        }

        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
            builder.Append("return ");
            expression.Render(builder, isInnerClass, 4, new CodegenIndent(true));
        }
    }
} // end of namespace