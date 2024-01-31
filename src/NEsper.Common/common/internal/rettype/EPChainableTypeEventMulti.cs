///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.rettype
{
    /// <summary>
    ///     An array or collection of native values. Always has a component type.
    ///     Either: - array then "clazz.Array" returns true. - collection then clazz : collection
    /// </summary>
    public class EPChainableTypeEventMulti : EPChainableType
    {
        public EPChainableTypeEventMulti(
            Type container,
            EventType component)
        {
            Container = container;
            Component = component;
        }

        public Type Container { get; }

        public EventType Component { get; }

        public CodegenExpression Codegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression typeInitSvcRef)
        {
            return CodegenExpressionBuilder.NewInstance(
                typeof(EPChainableTypeEventMulti),
                CodegenExpressionBuilder.Constant(Container),
                EventTypeUtility.ResolveTypeCodegen(Component, typeInitSvcRef));
        }

        public override string ToString()
        {
            return $"{nameof(Container)}: {Container}, {nameof(Component)}: {Component}";
        }
    }
}