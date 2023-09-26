///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.context.controller.keyed;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class ContextSpecKeyed : ContextSpec
    {
        public ContextSpecKeyed(
            IList<ContextSpecKeyedItem> items,
            IList<ContextSpecConditionFilter> optionalInit,
            ContextSpecCondition optionalTermination)
        {
            Items = items;
            OptionalInit = optionalInit;
            OptionalTermination = optionalTermination;
        }

        public IList<ContextSpecKeyedItem> Items { get; }

        public ContextSpecCondition OptionalTermination { get; set; }

        public IList<ContextSpecConditionFilter> OptionalInit { get; }

        public MultiKeyClassRef MultiKeyClassRef { get; set; }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ContextControllerDetailKeyed), GetType(), classScope);

            method.Block.DeclareVar<ContextControllerDetailKeyedItem[]>(
                "items",
                NewArrayByLength(typeof(ContextControllerDetailKeyedItem), Constant(Items.Count)));
            for (var i = 0; i < Items.Count; i++) {
                method.Block.AssignArrayElement(
                    "items",
                    Constant(i),
                    Items[i].MakeCodegen(method, symbols, classScope));
            }

            method.Block
                .DeclareVar<ContextControllerDetailKeyed>("detail", NewInstance(typeof(ContextControllerDetailKeyed)))
                .SetProperty(Ref("detail"), "Items", Ref("items"))
                .SetProperty(
                    Ref("detail"),
                    "MultiKeyFromObjectArray",
                    MultiKeyCodegen.CodegenMultiKeyFromArrayTransform(MultiKeyClassRef, method, classScope));

            if (OptionalInit != null && !OptionalInit.IsEmpty()) {
                method.Block.DeclareVar<ContextConditionDescriptorFilter[]>(
                    "init",
                    NewArrayByLength(typeof(ContextConditionDescriptorFilter), Constant(OptionalInit.Count)));
                for (var i = 0; i < OptionalInit.Count; i++) {
                    method.Block.AssignArrayElement(
                        "init",
                        Constant(i),
                        Cast(
                            typeof(ContextConditionDescriptorFilter),
                            OptionalInit[i].Make(method, symbols, classScope)));
                }

                method.Block.SetProperty(Ref("detail"), "OptionalInit", Ref("init"));
            }

            if (OptionalTermination != null) {
                method.Block.SetProperty(
                    Ref("detail"),
                    "OptionalTermination",
                    OptionalTermination.Make(method, symbols, classScope));
            }

            method.Block.Expression(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add("AddReadyCallback", Ref("detail")))
                .MethodReturn(Ref("detail"));
            return LocalMethod(method);
        }
    }
} // end of namespace