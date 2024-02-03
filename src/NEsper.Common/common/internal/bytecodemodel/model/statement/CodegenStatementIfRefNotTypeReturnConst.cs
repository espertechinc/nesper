///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionUtil;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementIfRefNotTypeReturnConst : CodegenStatementBase
    {
        private readonly object _constant;
        private readonly Type _type;

        private readonly string _var;

        public CodegenStatementIfRefNotTypeReturnConst(
            string var,
            Type type,
            object constant)
        {
            _var = var;
            _type = type;
            _constant = constant;
        }

        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
            builder.Append("if (!(").Append(_var).Append(" is ");
            AppendClassName(builder, _type);
            builder.Append(")) return ");
            RenderConstant(builder, _constant);
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            classes.AddToSet(_type);
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
        }
    }
} // end of namespace