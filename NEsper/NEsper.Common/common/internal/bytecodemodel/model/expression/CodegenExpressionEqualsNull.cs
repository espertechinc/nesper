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
    public class CodegenExpressionEqualsNull : CodegenExpression
    {
        private readonly CodegenExpression _lhs;
        private readonly bool _not;

        public CodegenExpressionEqualsNull(CodegenExpression lhs, bool not)
        {
            this._lhs = lhs;
            this._not = not;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass)
        {
            _lhs.Render(builder, imports, isInnerClass);
            builder.Append(" ");
            builder.Append(_not ? "!=" : "==");
            builder.Append(" null");
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _lhs.MergeClasses(classes);
        }
    }
} // end of namespace