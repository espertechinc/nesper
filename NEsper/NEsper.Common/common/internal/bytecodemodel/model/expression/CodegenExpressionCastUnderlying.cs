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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionCastUnderlying : CodegenExpression
    {
        private readonly Type _clazz;
        private readonly CodegenExpression _expression;

        public CodegenExpressionCastUnderlying(
            Type clazz,
            CodegenExpression expression)
        {
            this._clazz = clazz;
            this._expression = expression;
        }

        public void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass)
        {
            builder.Append("((");
            AppendClassName(builder, _clazz, null, imports);
            builder.Append(")");
            _expression.Render(builder, imports, isInnerClass);
            builder.Append(".").Append("getUnderlying())");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            classes.Add(_clazz);
            _expression.MergeClasses(classes);
        }
    }
} // end of namespace