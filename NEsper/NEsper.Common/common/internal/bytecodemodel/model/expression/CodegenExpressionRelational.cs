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
    public class CodegenExpressionRelational : CodegenExpression
    {
        private readonly CodegenExpression _lhs;
        private readonly CodegenRelational _op;
        private readonly CodegenExpression _rhs;

        public CodegenExpressionRelational(CodegenExpression lhs, CodegenRelational op, CodegenExpression rhs)
        {
            this._lhs = lhs;
            this._op = op;
            this._rhs = rhs;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports, bool isInnerClass)
        {
            _lhs.Render(builder, imports, isInnerClass);
            builder.Append(_op.Op);
            _rhs.Render(builder, imports, isInnerClass);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _lhs.MergeClasses(classes);
            _rhs.MergeClasses(classes);
        }

        public class CodegenRelational
        {
            public static readonly CodegenRelational GE = new CodegenRelational(">=");
            public static readonly CodegenRelational GT = new CodegenRelational(">");
            public static readonly CodegenRelational LE = new CodegenRelational("<=");
            public static readonly CodegenRelational LT = new CodegenRelational("<");

            private CodegenRelational(string op)
            {
                Op = op;
            }

            public string Op { get; }
        }
    }
} // end of namespace