///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.codegen.core;

//import static com.espertech.esper.codegen.core.CodeGenerationHelper.appendClassName;
//import static com.espertech.esper.codegen.model.expression.CodegenExpressionUtil.renderConstant;

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementIfRefNotTypeReturnConst : CodegenStatementBase
    {
        private readonly string var;
        private readonly Type type;
        private readonly Object constant;

        public CodegenStatementIfRefNotTypeReturnConst(string var, Type type, Object constant)
        {
            this.var = var;
            this.type = type;
            this.constant = constant;
        }

        public override void RenderStatement(StringBuilder builder, IDictionary<Type, string> imports)
        {
            builder.Append("if (!(").Append(var).Append(" is ");
            CodeGenerationHelper.AppendClassName(builder, type, null, imports).Append(")) return ");
            CodegenExpressionUtil.RenderConstant(builder, constant);
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(type);
        }
    }
} // end of namespace