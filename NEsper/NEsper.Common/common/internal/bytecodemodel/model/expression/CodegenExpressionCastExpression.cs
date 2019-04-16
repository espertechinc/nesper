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

            this._clazz = clazz;
            _typeName = null;
            this._expression = expression;
        }

        public CodegenExpressionCastExpression(
            string typeName,
            CodegenExpression expression)
        {
            if (typeName == null) {
                throw new ArgumentException("Cast-to class is a null value");
            }

            _clazz = null;
            this._typeName = typeName;
            this._expression = expression;
        }

        public void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass)
        {
            builder.Append("((");
            if (_clazz != null) {
                AppendClassName(builder, _clazz, null, imports);
            }
            else {
                builder.Append(_typeName);
            }

            builder.Append(")");
            _expression.Render(builder, imports, isInnerClass);
            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            if (_clazz != null) {
                classes.Add(_clazz);
            }

            _expression.MergeClasses(classes);
        }
    }
} // end of namespace