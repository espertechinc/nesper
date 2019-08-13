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

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionRef : CodegenExpression
    {
        protected internal string _ref;

        public CodegenExpressionRef(string @ref)
        {
            if (@ref == "refSet0") {
                Console.WriteLine("stop");
            }
            _ref = @ref;
        }

        public virtual string Ref => _ref;

        public virtual void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append(_ref);
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

            return _ref.Equals(that._ref);
        }

        public override int GetHashCode()
        {
            return _ref.GetHashCode();
        }
    }
} // end of namespace