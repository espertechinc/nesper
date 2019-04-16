///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.category;
using com.espertech.esper.common.@internal.filterspec;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class ContextSpecCategory : ContextSpec
    {
        [NonSerialized] private FilterSpecCompiled filterSpecCompiled;

        public ContextSpecCategory(
            IList<ContextSpecCategoryItem> items,
            FilterSpecRaw filterSpecRaw)
        {
            Items = items;
            FilterSpecRaw = filterSpecRaw;
        }

        public FilterSpecRaw FilterSpecRaw { get; }

        public IList<ContextSpecCategoryItem> Items { get; }

        public FilterSpecCompiled FilterSpecCompiled {
            get => filterSpecCompiled;
            set => filterSpecCompiled = value;
        }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ContextControllerDetailCategory), GetType(), classScope);

            var makeFilter = filterSpecCompiled.MakeCodegen(method, symbols, classScope);
            method.Block
                .DeclareVar(typeof(FilterSpecActivatable), "filterSpec", LocalMethod(makeFilter))
                .DeclareVar(typeof(EventType), "eventType", ExprDotMethod(Ref("filterSpec"), "getFilterForEventType"));

            method.Block.DeclareVar(
                typeof(ContextControllerDetailCategoryItem[]), "items",
                NewArrayByLength(typeof(ContextControllerDetailCategoryItem), Constant(Items.Count)));
            for (var i = 0; i < Items.Count; i++) {
                method.Block.AssignArrayElement(
                    "items", Constant(i),
                    LocalMethod(
                        Items[i].MakeCodegen(classScope, method), Ref("eventType"), symbols.GetAddInitSvc(method)));
            }

            method.Block
                .DeclareVar(
                    typeof(ContextControllerDetailCategory), "detail",
                    NewInstance(typeof(ContextControllerDetailCategory)))
                .ExprDotMethod(Ref("detail"), "setFilterSpecActivatable", Ref("filterSpec"))
                .ExprDotMethod(Ref("detail"), "setItems", Ref("items"))
                .MethodReturn(Ref("detail"));
            return LocalMethod(method);
        }
    }
} // end of namespace