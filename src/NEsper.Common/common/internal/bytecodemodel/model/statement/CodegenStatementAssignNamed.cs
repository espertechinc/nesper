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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementAssignNamed : CodegenStatementBase
    {
        private readonly CodegenExpression _assignment;
        private readonly CodegenExpression _lhs;

        public CodegenStatementAssignNamed(
            CodegenExpression lhs,
            CodegenExpression assignment)
        {
            _lhs = lhs;
            _assignment = assignment ?? throw new ArgumentException("Assignment not provided");
        }

        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
            var codegenIndent = new CodegenIndent(true);
            _lhs.Render(builder, isInnerClass, 1, codegenIndent);
            builder.Append('=');
            _assignment.Render(builder, isInnerClass, 1, codegenIndent);
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            _assignment.MergeClasses(classes);
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_lhs);
            consumer.Invoke(_assignment);
        }
    }
} // end of namespace