///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class CodegenMemberCol
    {
        public IDictionary<CodegenExpressionRefWCol, Type> Members { get; } =
            new LinkedHashMap<CodegenExpressionRefWCol, Type>();

        public CodegenExpressionRef AddMember(
            int column,
            Type type,
            string name)
        {
            if (type == null) {
                throw new ArgumentException("Null type");
            }

            var @ref = new CodegenExpressionRefWCol(name, column);
            Members.Put(@ref, type);
            return @ref;
        }
    }
} // end of namespace