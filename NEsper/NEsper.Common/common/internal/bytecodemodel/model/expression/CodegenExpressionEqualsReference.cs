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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionEqualsReference : CodegenExpression
    {
        private readonly bool _isNot;
        private readonly CodegenExpression _lhs;
        private readonly CodegenExpression _rhs;

        public CodegenExpressionEqualsReference(
            CodegenExpression lhs,
            CodegenExpression rhs,
            bool isNot)
        {
            this._lhs = lhs;
            this._rhs = rhs;
            this._isNot = isNot;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("(");
            _lhs.Render(builder, isInnerClass, level, indent);
            builder.Append(_isNot ? "!=" : "==");
            _rhs.Render(builder, isInnerClass, level, indent);
            builder.Append(")");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _lhs.MergeClasses(classes);
            _rhs.MergeClasses(classes);
        }
    }
} // end of namespace