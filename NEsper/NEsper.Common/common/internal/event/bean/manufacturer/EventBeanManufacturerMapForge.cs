///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.compat.collections;

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
            CodegenBlock codegenBlock,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            //var init = codegenClassScope.NamespaceScope.InitMethod;

            var factory = codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(mapEventType, EPStatementInitServicesConstants.REF));

            var makeUndFunc = new CodegenExpressionLambda(codegenBlock)
                .WithParam<object[]>("properties")
                .WithBody(block => MakeUnderlyingCodegen(block, codegenClassScope));

            var manufacturer = NewInstance<ProxyMapEventBeanManufacturer>(eventType, factory, makeUndFunc);

            //var manufacturer = NewAnonymousClass(init.Block, typeof(EventBeanManufacturer));

            //var makeUndMethod = CodegenMethod
            //    .MakeMethod(typeof(IDictionary<object, object>), GetType(), codegenClassScope)
            //    .AddParam(typeof(object[]), "properties");
            //manufacturer.AddMethod("MakeUnderlying", makeUndMethod);
            //MakeUnderlyingCodegen(makeUndMethod, codegenClassScope);

            //var makeMethod = CodegenMethod.MakeMethod(typeof(EventBean), GetType(), codegenClassScope)
            //    .AddParam(typeof(object[]), "properties");
            //manufacturer.AddMethod("Make", makeMethod);
            //makeMethod.Block
            //    .DeclareVar<IDictionary<object, object>>("und", LocalMethod(makeUndMethod, Ref("properties")))
            //    .MethodReturn(ExprDotMethod(factory, "AdapterForTypedMap", Ref("und"), eventType));

            return codegenClassScope.AddDefaultFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
        }

        public EventBeanManufacturer GetManufacturer(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new EventBeanManufacturerMap(mapEventType, eventBeanTypedEventFactory, writables);
        }

        private void MakeUnderlyingCodegen(
            CodegenBlock block,
            CodegenClassScope codegenClassScope)
        {
            block.DeclareVar<IDictionary<string, object>>(
                "values",
                NewInstance(typeof(HashMap<string, object>)));
            for (var i = 0; i < writables.Length; i++) {
                block.ExprDotMethod(
                    Ref("values"),
                    "Put",
                    Constant(writables[i].PropertyName),
                    ArrayAtIndex(Ref("properties"), Constant(i)));
            }

            block.BlockReturn(Ref("values"));
        }
    }
} // end of namespace