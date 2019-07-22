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
    public class CodegenExpressionIncrementDecrementName : CodegenExpression
    {
        private readonly bool _increment;

        private readonly string _ref;

        public CodegenExpressionIncrementDecrementName(
            string @ref,
            bool increment)
        {
            _ref = @ref;
            _increment = increment;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass)
        {
            builder.Append(_ref).Append(_increment ? "++" : "--");
        }

        public void MergeClasses(ISet<Type> classes)
        {
        }
    }
} // end of namespace