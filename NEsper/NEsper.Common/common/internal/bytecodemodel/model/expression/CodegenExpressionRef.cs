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
    public class CodegenExpressionRef : CodegenExpression
    {
        public CodegenExpressionRef(string @ref)
        {
            Ref = @ref;
        }

        public virtual string Ref { get; internal set; }

        public virtual void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            bool isInnerClass)
        {
            builder.Append(Ref);
        }

        public virtual void MergeClasses(ISet<Type> classes)
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

            var that = (CodegenExpressionRef) obj;

            return Ref.Equals(that.Ref);
        }

        public override int GetHashCode()
        {
            return Ref.GetHashCode();
        }
    }
} // end of namespace