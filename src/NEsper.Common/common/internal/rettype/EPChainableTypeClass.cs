///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.rettype
{
    /// <summary>
    ///     Any primitive type as well as any class and other non-array or non-collection type
    /// </summary>
    public class EPChainableTypeClass : EPChainableType
    {
        public EPChainableTypeClass(Type type)
        {
            Clazz = type;
        }

        public Type Clazz { get; }

        public static Type FromInputOrNull(EPChainableType inputType)
        {
            return inputType is EPChainableTypeClass typeClass ? typeClass.Clazz : null;
        }

        public CodegenExpression Codegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression typeInitSvcRef)
        {
            return CodegenExpressionBuilder.NewInstance<EPChainableTypeClass>(CodegenExpressionBuilder.Constant(Clazz));
        }
    }
}