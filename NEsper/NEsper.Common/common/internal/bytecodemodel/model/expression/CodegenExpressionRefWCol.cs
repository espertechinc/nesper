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
    public class CodegenExpressionRefWCol : CodegenExpressionRef
    {
        private readonly int _col;

        public CodegenExpressionRefWCol(
            string @ref,
            int col)
            : base(@ref)
        {
            _col = col;
        }

        public override string Ref => _ref + _col;

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            base.Render(builder, isInnerClass, level, indent);
            builder.Append(_col);
        }

        public override void MergeClasses(ISet<Type> classes)
        {
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            if (!base.Equals(o)) {
                return false;
            }

            var that = (CodegenExpressionRefWCol) o;

            return _col == that._col;
        }

        public override int GetHashCode()
        {
            var result = base.GetHashCode();
            result = 31 * result + _col;
            return result;
        }
    }
} // end of namespace