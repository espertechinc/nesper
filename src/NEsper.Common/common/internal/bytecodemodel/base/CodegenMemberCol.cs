///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
            Members[member] = type;
            return member;
        }

        public Type Get(CodegenExpressionMemberWCol member)
        {
            Members.TryGetValue(member, out var type);
            return type;
        }

        public void Put(
            CodegenExpressionMemberWCol member,
            Type type)
        {
            Members[member] = type;
        }

        public IOrderedDictionary<int, IList<CodegenExpressionMemberWCol>> MembersPerColumn {
            get {
                var columns = new OrderedListDictionary<int, IList<CodegenExpressionMemberWCol>>();
                foreach (var keypair in Members) {
                    var col = keypair.Key.Col;
                    if (!columns.TryGetValue(col, out var members)) {
                        columns[col] = members = new List<CodegenExpressionMemberWCol>();
                    }

                    members.Add(keypair.Key);
                }

                return columns;
            }
        }
    }
} // end of namespace