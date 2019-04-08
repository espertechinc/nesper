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
        /// Computer for relational op.
        /// </summary>
        public interface Computer {
            /// <summary>
            /// Compares objects and returns boolean indicating larger (true) or smaller (false).
            /// </summary>
            /// <param name="objOne">object to compare</param>
            /// <param name="objTwo">object to compare</param>
            /// <returns>true if larger, false if smaller</returns>
            bool Compare(object objOne, object objTwo);

            CodegenExpression Codegen(CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs, Type rhsType);
        }
    }
}