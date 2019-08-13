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
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(mapEventType, EPStatementInitServicesConstants.REF));

            var makeUndMethod = new CodegenExpressionLambda(init.Block)
                .WithParam<object[]>("properties");
            init.Block.DeclareVar<ProxyEventBeanManufacturer.MakeUnderlyingFunc>(
                "makeUndFunc", makeUndMethod);
            MakeUnderlyingCodegen(makeUndMethod.Block, codegenClassScope);

            var makeMethod = new CodegenExpressionLambda(init.Block)
                .WithParam<object[]>("properties");
            init.Block.DeclareVar<ProxyEventBeanManufacturer.MakeFunc>(
                "makeFunc", makeMethod);
            makeMethod.Block
                .DeclareVar<IDictionary<string, object>>(
                    "und",
                    Cast<IDictionary<string, object>>(
                        ExprDotMethod(
                            Ref("makeUndFunc"), "Invoke", Ref("properties"))))
                .BlockReturn(ExprDotMethod(factory, "AdapterForTypedMap", Ref("und"), eventType));

            var manufacturer = NewInstance<ProxyEventBeanManufacturer>(
                Ref("makeFunc"),
                Ref("makeUndFunc"));

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

            return codegenClassScope.AddFieldUnshared(true, typeof(EventBeanManufacturer), manufacturer);
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
                NewInstance(typeof(Dictionary<string, object>)));
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