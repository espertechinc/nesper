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
    public class CodegenExpressionAssign : CodegenExpression
    {
        private readonly CodegenExpression _lhs;
        private readonly CodegenExpression _rhs;

        public CodegenExpressionAssign(
            CodegenExpression lhs,
            CodegenExpression rhs)
        {
            this._lhs = lhs;
            this._rhs = rhs;
        }

        public void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass)
        {
            _lhs.Render(builder, imports, isInnerClass);
            builder.Append("=");
            _rhs.Render(builder, imports, isInnerClass);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _lhs.MergeClasses(classes);
            _rhs.MergeClasses(classes);
        }
    }
} // end of namespace