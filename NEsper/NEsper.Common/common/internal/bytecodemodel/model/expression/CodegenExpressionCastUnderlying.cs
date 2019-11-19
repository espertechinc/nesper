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
using com.espertech.esper.common.@internal.bytecodemodel.util;

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
            _clazz = clazz;
            _expression = expression;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("((");
            AppendClassName(builder, _clazz);
            builder.Append(")");
            _expression.Render(builder, isInnerClass, level, indent);
            builder.Append(".").Append("Underlying)");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            classes.AddToSet(_clazz);
            _expression.MergeClasses(classes);
        }
    }
} // end of namespace