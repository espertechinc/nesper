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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionNot : CodegenExpression
    {
        private readonly bool _isNot;

        public CodegenExpressionNot(bool isNot, CodegenExpression expression)
        {
            this._isNot = isNot;
            Expression = expression;
        }

        public CodegenExpression Expression { get; }

        public void MergeClasses(ISet<Type> classes)
        {
            Expression.MergeClasses(classes);
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass)
        {
            if (_isNot) {
                builder.Append("!(");
                Expression.Render(builder, imports, isInnerClass);
                builder.Append(")");
            }
            else {
                Expression.Render(builder, imports, isInnerClass);
            }
        }
    }
} // end of namespace