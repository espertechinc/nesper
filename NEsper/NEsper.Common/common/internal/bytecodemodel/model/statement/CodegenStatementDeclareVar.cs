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
using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementDeclareVar : CodegenStatementBase
    {
        private readonly Type clazz;
        private readonly CodegenExpression optionalInitializer;
        private readonly Type optionalTypeVariable;
        private readonly string typeName;
        private readonly string var;

        public CodegenStatementDeclareVar(
            Type clazz, Type optionalTypeVariable, string var, CodegenExpression optionalInitializer)
        {
            if (clazz == null) {
                throw new ArgumentException("Class cannot be null");
            }

            this.clazz = clazz;
            typeName = null;
            this.optionalTypeVariable = optionalTypeVariable;
            this.var = var;
            this.optionalInitializer = optionalInitializer;
        }

        public CodegenStatementDeclareVar(
            string typeName, Type optionalTypeVariable, string var, CodegenExpression optionalInitializer)
        {
            if (typeName == null) {
                throw new ArgumentException("Class cannot be null");
            }

            clazz = null;
            this.typeName = typeName;
            this.optionalTypeVariable = optionalTypeVariable;
            this.var = var;
            this.optionalInitializer = optionalInitializer;
        }

        public override void RenderStatement(
            StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass)
        {
            if (clazz != null) {
                AppendClassName(builder, clazz, optionalTypeVariable, imports);
            }
            else {
                builder.Append(typeName);
            }

            builder.Append(" ").Append(var);
            if (optionalInitializer != null) {
                builder.Append("=");
                optionalInitializer.Render(builder, imports, isInnerClass);
            }
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            if (clazz != null) {
                classes.Add(clazz);
            }

            if (optionalInitializer != null) {
                optionalInitializer.MergeClasses(classes);
            }
        }
    }
} // end of namespace