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
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionDefault : CodegenExpression
    {
        private readonly Type _type;

        public CodegenExpressionDefault()
        {
            _type = null;
        }
        
        public CodegenExpressionDefault(Type type)
        {
            _type = type;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("default");
            if (_type != null) {
                builder.Append("(");
                builder.Append(_type.CleanName());
                builder.Append(")");
            }
        }

        public void MergeClasses(ISet<Type> classes)
        {
            if (_type == null) {
                return;
            }
            classes.AddToSet(_type);
        }
        
        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
        }
    }
} // end of namespace