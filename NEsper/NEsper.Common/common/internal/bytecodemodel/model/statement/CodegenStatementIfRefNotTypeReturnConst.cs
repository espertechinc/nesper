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
using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionUtil;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementIfRefNotTypeReturnConst : CodegenStatementBase
    {
        private readonly object constant;
        private readonly Type type;

        private readonly string var;

        public CodegenStatementIfRefNotTypeReturnConst(string var, Type type, object constant)
        {
            this.var = var;
            this.type = type;
            this.constant = constant;
        }

        public override void RenderStatement(
            StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass)
        {
            builder.Append("if (!(").Append(var).Append(" instanceof ");
            AppendClassName(builder, type, null, imports).Append(")) return ");
            RenderConstant(builder, constant, imports);
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            classes.Add(type);
        }
    }
} // end of namespace