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
    public class CodegenStatementExpression : CodegenStatementBase
    {
        private readonly CodegenExpression expression;

        public CodegenStatementExpression(CodegenExpression expression)
        {
            this.expression = expression;
        }

        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
            expression.Render(builder, isInnerClass, 1, new CodegenIndent(true));
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            expression.MergeClasses(classes);
        }
    }
} // end of namespace