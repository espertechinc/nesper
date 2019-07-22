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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionBeanUndCastDotMethodConst : CodegenExpression
    {
        private readonly Type _clazz;
        private readonly object _constant;
        private readonly CodegenExpression _expression;
        private readonly string _method;

        public CodegenExpressionBeanUndCastDotMethodConst(
            Type clazz,
            CodegenExpression expression,
            string method,
            object constant)
        {
            this._clazz = clazz;
            this._expression = expression;
            this._method = method;
            this._constant = constant;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass)
        {
            builder.Append("((");
            AppendClassName(builder, _clazz);
            builder.Append(")");
            _expression.Render(builder, isInnerClass);
            builder.Append(".getUnderlying()).");
            builder.Append(_method).Append("(");
            RenderConstant(builder, _constant);
            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _expression.MergeClasses(classes);
            classes.Add(_clazz);
        }
    }
} // end of namespace