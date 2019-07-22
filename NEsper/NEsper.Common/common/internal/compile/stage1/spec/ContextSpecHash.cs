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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.hash;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class ContextSpecHash : ContextSpec
    {
        public ContextSpecHash(
            IList<ContextSpecHashItem> items,
            int granularity,
            bool preallocate)
        {
            Items = items;
            IsPreallocate = preallocate;
            Granularity = granularity;
        }

        public IList<ContextSpecHashItem> Items { get; }

        public bool IsPreallocate { get; }

        public int Granularity { get; }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ContextControllerDetailHash), GetType(), classScope);

            method.Block.DeclareVar<ContextControllerDetailHashItem[]>(
                "items",
                NewArrayByLength(typeof(ContextControllerDetailHashItem), Constant(Items.Count)));
            for (var i = 0; i < Items.Count; i++) {
                method.Block.AssignArrayElement(
                    "items",
                    Constant(i),
                    Items[i].MakeCodegen(method, symbols, classScope));
            }

            method.Block
                .DeclareVar<ContextControllerDetailHash>(
                    "detail",
                    NewInstance(typeof(ContextControllerDetailHash)))
                .SetProperty(Ref("detail"), "Items", Ref("items"))
                .SetProperty(Ref("detail"), "Granularity", Constant(Granularity))
                .SetProperty(Ref("detail"), "Preallocate", Constant(IsPreallocate))
                .MethodReturn(Ref("detail"));
            return LocalMethod(method);
        }
    }
} // end of namespace