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

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionCastExpression : CodegenExpression
    {
        private readonly Type _clazz;
        private readonly CodegenExpression _expression;
        private readonly string _typeName;

        public CodegenExpressionCastExpression(
            Type clazz,
            CodegenExpression expression)
        {
            if (clazz == null) {
                throw new ArgumentException("Cast-to class is a null value");
            }

            _clazz = clazz;
            _typeName = null;
            _expression = expression;
        }

        public CodegenExpressionCastExpression(
            string typeName,
            CodegenExpression expression)
        {
            if (typeName == null) {
                throw new ArgumentException("Cast-to class is a null value");
            }

            _clazz = null;
            _typeName = typeName.CodeInclusionTypeName();
            _expression = expression;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("((");
            if (_clazz != null) {
                AppendClassName(builder, _clazz);
            }
            else {
                builder.Append(_typeName);
            }

            builder.Append(")");
            _expression.Render(builder, isInnerClass, level, indent);
            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            if (_clazz != null) {
                classes.AddToSet(_clazz);
            }

            _expression.MergeClasses(classes);
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            consumer.Invoke(_expression);
        }
    }
} // end of namespace