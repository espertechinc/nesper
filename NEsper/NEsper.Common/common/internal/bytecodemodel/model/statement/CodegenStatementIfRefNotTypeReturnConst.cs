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

using com.espertech.esper.common.@internal.bytecodemodel.util;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionUtil;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementIfRefNotTypeReturnConst : CodegenStatementBase
    {
        private readonly object constant;
        private readonly Type type;

        private readonly string var;

        public CodegenStatementIfRefNotTypeReturnConst(
            string var,
            Type type,
            object constant)
        {
            this.var = var;
            this.type = type;
            this.constant = constant;
        }

        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
            builder.Append("if (!(").Append(var).Append(" is ");
            AppendClassName(builder, type);
            builder.Append(")) return ");
            RenderConstant(builder, constant);
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            classes.AddToSet(type);
        }
    }
} // end of namespace