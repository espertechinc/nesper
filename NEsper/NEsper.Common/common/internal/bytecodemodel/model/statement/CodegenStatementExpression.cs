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
    public class CodegenStatementExpression : CodegenStatementBase
    {
        private readonly CodegenExpression _expression;

        public CodegenStatementExpression(CodegenExpression expression)
        {
            _expression = expression;
        }

        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
            _expression.Render(builder, isInnerClass, 1, new CodegenIndent(true));
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            _expression.MergeClasses(classes);
        }
        
        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_expression);}
    }
} // end of namespace