///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; //constantNull

namespace com.espertech.esper.common.@internal.compile.multikey
{
    public class MultiKeyClassRefEmpty : MultiKeyClassRef
    {
        public static readonly MultiKeyClassRefEmpty INSTANCE = new MultiKeyClassRefEmpty();

        private MultiKeyClassRefEmpty()
        {
        }

        public NameOrType ClassNameMK => null;

        public Type[] MKTypes => new Type[0];

        public CodegenExpression GetExprMKSerde(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return ConstantNull();
        }
    }
} // end of namespace