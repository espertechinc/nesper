///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    /// <summary>
    ///     Factory for Map-underlying events.
    /// </summary>
    public class EventBeanManufacturerMapForge : EventBeanManufacturerForge
    {
        private readonly MapEventType mapEventType;
        private readonly WriteablePropertyDescriptor[] writables;

        public EventBeanManufacturerMapForge(
            MapEventType mapEventType,
            WriteablePropertyDescriptor[] writables)
        {
            this.mapEventType = mapEventType;
            this.writables = writables;
        }

        public CodegenExpression Make(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var init = codegenClassScope.NamespaceScope.InitMethod;

            var factory = codegenClassScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddFieldUnshared(
                true, typeof(EventType), EventTypeUtility.ResolveTypeCodegen(mapEventType, EPStatementInitServicesConstants.REF));

            var manufacturer = NewAnonymousClass(init.Block, typeof(EventBeanManufacturer));

            var makeUndMethod = CodegenMethod.MakeParentNode(typeof(IDictionary<object, object>), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "properties");
            manufacturer.AddMethod("makeUnderlying", makeUndMethod);
            MakeUnderlyingCodegen(makeUndMethod, codegenClassScope);

            var makeMethod = CodegenMethod.MakeParentNode(typeof(EventBean), GetType(), codegenClassScope)
                .AddParam(typeof(object[]), "properties");
            manufacturer.AddMethod("make", makeMethod);
            makeMethod.Block
                .DeclareVar(typeof(IDictionary<object, object>), "und", LocalMethod(makeUndMethod, Ref("properties")))
                .MethodReturn(ExprDotMethod(factory, "adapterForTypedMap", Ref("und"), eventType));

            return codegenClassScope.AddFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
        }

        public EventBeanManufacturer GetManufacturer(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new EventBeanManufacturerMap(mapEventType, eventBeanTypedEventFactory, writables);
        }

        private void MakeUnderlyingCodegen(
            CodegenMethod method,
            CodegenClassScope codegenClassScope)
        {
            method.Block.DeclareVar(typeof(IDictionary<object, object>), "values", NewInstance(typeof(Dictionary<object, object>)));
            for (var i = 0; i < writables.Length; i++) {
                method.Block.ExprDotMethod(Ref("values"), "put", Constant(writables[i].PropertyName), ArrayAtIndex(Ref("properties"), Constant(i)));
            }

            method.Block.MethodReturn(Ref("values"));
        }
    }
} // end of namespace