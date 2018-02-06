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

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionCastUnderlying : ICodegenExpression
    {
        private readonly Type _clazz;
        private readonly ICodegenExpression _expression;

        public CodegenExpressionCastUnderlying(Type clazz, ICodegenExpression expression)
        {
            this._clazz = clazz;
            this._expression = expression;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            builder.Append("((");
            CodeGenerationHelper.AppendClassName(builder, _clazz, null, imports);
            builder.Append(")");
            _expression.Render(builder, imports);
            builder.Append(".").Append("GetUnderlying())");
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(_clazz);
            _expression.MergeClasses(classes);
        }
    }
} // end of namespace