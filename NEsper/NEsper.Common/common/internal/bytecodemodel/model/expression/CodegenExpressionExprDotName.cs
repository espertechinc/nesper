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
    public class CodegenExpressionExprDotName : CodegenExpression
    {
        private readonly CodegenExpression _lhs;
        private readonly string _name;

        public CodegenExpressionExprDotName(
            CodegenExpression lhs,
            string name)
        {
            this._lhs = lhs;
            this._name = name;
        }

        public void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass)
        {
            if (_lhs is CodegenExpressionRef) {
                _lhs.Render(builder, imports, isInnerClass);
            }
            else {
                builder.Append("(");
                _lhs.Render(builder, imports, isInnerClass);
                builder.Append(")");
            }

            builder.Append('.').Append(_name);
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _lhs.MergeClasses(classes);
        }
    }
} // end of namespace