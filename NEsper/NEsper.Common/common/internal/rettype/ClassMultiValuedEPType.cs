///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    ///     An array or collection of native values. Always has a component type.
    ///     Either: - array then "clazz.Array" returns true. - collection then clazz : collection
    /// </summary>
    public class ClassMultiValuedEPType : EPType
    {
        internal ClassMultiValuedEPType(
            Type container,
            Type component)
        {
            Container = container;
            Component = component;
        }

        public Type Container { get; }

        public Type Component { get; }

        public CodegenExpression Codegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression typeInitSvcRef)
        {
            return CodegenExpressionBuilder.NewInstance(
                typeof(ClassMultiValuedEPType),
                CodegenExpressionBuilder.Constant(Container),
                CodegenExpressionBuilder.Constant(Component));
        }
    }
}