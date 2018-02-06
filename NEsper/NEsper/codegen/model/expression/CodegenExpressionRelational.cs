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

namespace com.espertech.esper.codegen.model.expression
{
    public class CodegenExpressionRelational : ICodegenExpression
    {
        private readonly ICodegenExpression _lhs;
        private readonly CodegenRelational _op;
        private readonly ICodegenExpression _rhs;

        public CodegenExpressionRelational(ICodegenExpression lhs, CodegenRelational op, ICodegenExpression rhs)
        {
            _lhs = lhs;
            _op = op;
            _rhs = rhs;
        }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            _lhs.Render(builder, imports);
            builder.Append(_op.GetOp());
            _rhs.Render(builder, imports);
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            _lhs.MergeClasses(classes);
            _rhs.MergeClasses(classes);
        }
    }
} // end of namespace
