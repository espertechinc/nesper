///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.type
{
    public partial class RelationalOpEnum
    {
        /// <summary>
        /// Computer for relational op compare.
        /// </summary>
        public class GTStringComputer : Computer
        {
            public bool Compare(
                object objOne,
                object objTwo)
            {
                string s1 = (string) objOne;
                string s2 = (string) objTwo;
                int result = s1.CompareTo(s2);
                return result > 0;
            }

            public CodegenExpression Codegen(
                CodegenExpressionRef lhs,
                Type lhsType,
                CodegenExpression rhs,
                Type rhsType)
            {
                return CodegenStringCompare(lhs, lhsType, rhs, rhsType, CodegenExpressionRelational.CodegenRelational.GT);
            }
        }
    }
}