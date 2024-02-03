///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionMember : CodegenExpression
    {
        internal readonly string _name;

        public CodegenExpressionMember(string name)
        {
            _name = name;
        }

        public virtual string Ref => _name;

        public virtual void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append(_name);
        }

        public virtual void MergeClasses(ISet<Type> classes)
        {
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
        }

        public override bool Equals(object obj)
        {
            if (this == obj) {
                return true;
            }

            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            var that = (CodegenExpressionMember)obj;

            return _name.Equals(that._name);
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }
    }
} // end of namespace