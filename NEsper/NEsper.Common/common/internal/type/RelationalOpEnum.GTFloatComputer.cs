///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.type
{
    public partial class RelationalOpEnum
    {
        /// <summary>
        /// Computer for relational op compare.
        /// </summary>
        public class GTFloatComputer : Computer {
            public bool Compare(object objOne, object objTwo) {
                object s1 = (object) objOne;
                object s2 = (object) objTwo;
                return s1.AsFloat() > s2.AsFloat();
            }

            public CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType) {
                return CodegenFloat(lhs, lhsType, rhs, rhsType, GT);
            }
        }
    }
}