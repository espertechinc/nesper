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
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.statement
{
    public class CodegenStatementDeclareVarWCast : CodegenStatementBase
    {
        private readonly Type _clazz;
        private readonly string _rhsName;
        private readonly string _var;

        public CodegenStatementDeclareVarWCast(
            Type clazz,
            string var,
            string rhsName)
        {
            _var = var;
            _clazz = clazz;
            _rhsName = rhsName;
        }

        public override void RenderStatement(
            StringBuilder builder,
            bool isInnerClass)
        {
            AppendClassName(builder, _clazz);
            builder
                .Append(" ")
                .Append(_var)
                .Append("=")
                .Append("(");

            AppendClassName(builder, _clazz);

            builder
                .Append(")")
                .Append(_rhsName);
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            classes.AddToSet(_clazz);
        }
        
        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
        }
    }
} // end of namespace