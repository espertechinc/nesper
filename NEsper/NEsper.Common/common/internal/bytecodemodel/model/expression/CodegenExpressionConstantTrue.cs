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
    public class CodegenExpressionConstantTrue : CodegenExpression
    {
        protected internal static readonly CodegenExpressionConstantTrue INSTANCE = new CodegenExpressionConstantTrue();

        private CodegenExpressionConstantTrue()
        {
        }

        public void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass)
        {
            builder.Append("true");
        }

        public void MergeClasses(ISet<Type> classes)
        {
        }
    }
} // end of namespace