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

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementBaseCtor : CodegenStatementBase
    {
        private readonly CodegenExpression[] parameters;

        public CodegenStatementBaseCtor(params CodegenExpression[] parameters)
        {
            this.parameters = parameters;
        }

        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
            builder.Append("base(");
            RenderExpressions(builder, parameters, isInnerClass);
            builder.Append(")");
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            MergeClassesExpressions(classes, parameters);
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            TraverseMultiple(parameters, consumer);
        }
    }
}