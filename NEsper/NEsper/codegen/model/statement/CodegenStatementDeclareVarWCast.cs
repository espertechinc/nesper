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

namespace com.espertech.esper.codegen.model.statement
{
    public class CodegenStatementDeclareVarWCast : CodegenStatementBase
    {
        private readonly string var;
        private readonly Type clazz;
        private readonly string rhsName;

        public CodegenStatementDeclareVarWCast(Type clazz, string var, string rhsName)
        {
            this.var = var;
            this.clazz = clazz;
            this.rhsName = rhsName;
        }

        public override void RenderStatement(StringBuilder builder, IDictionary<Type, string> imports)
        {
            CodeGenerationHelper.AppendClassName(builder, clazz, null, imports);
            builder.Append(" ").Append(var).Append("=").Append("(");
            CodeGenerationHelper.AppendClassName(builder, clazz, null, imports);
            builder.Append(")").Append(rhsName);
        }

        public override void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(clazz);
        }
    }
} // end of namespace