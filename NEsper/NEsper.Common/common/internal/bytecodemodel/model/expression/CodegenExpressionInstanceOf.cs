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
    public class CodegenExpressionInstanceOf : CodegenExpression
    {
        private readonly Type _clazz;
        private readonly CodegenExpression _lhs;
        private readonly bool _not;

        public CodegenExpressionInstanceOf(
            CodegenExpression lhs,
            Type clazz,
            bool not)
        {
            this._lhs = lhs;
            this._clazz = clazz;
            this._not = not;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass)
        {
            if (_not) {
                builder.Append("!(");
            }

            _lhs.Render(builder, isInnerClass);
            builder.Append(" ").Append("is ");
            AppendClassName(builder, _clazz);
            if (_not) {
                builder.Append(")");
            }
        }

        public void MergeClasses(ISet<Type> classes)
        {
            _lhs.MergeClasses(classes);
            classes.Add(_clazz);
        }
    }
} // end of namespace