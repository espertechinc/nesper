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
        public IDictionary<CodegenExpressionMemberWCol, Type> Members { get; } =
            new LinkedHashMap<CodegenExpressionMemberWCol, Type>();

        public CodegenExpressionMember AddMember(
            int column,
            Type type,
            string name)
        {
            if (type == null) {
                throw new ArgumentException("Null type");
            }

            var member = new CodegenExpressionMemberWCol(name, column);
            Members.Put(member, type);
            return member;
        }
    }
} // end of namespace