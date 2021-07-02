///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.hash;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class ContextSpecHashItem
    {
        public ContextSpecHashItem(
            Chainable function,
            FilterSpecRaw filterSpecRaw)
        {
            Function = function;
            FilterSpecRaw = filterSpecRaw;
        }

        public Chainable Function { get; }

        public FilterSpecRaw FilterSpecRaw { get; }

        public FilterSpecCompiled FilterSpecCompiled { get; set; }

        public ExprFilterSpecLookupableForge Lookupable { get; set; }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ContextControllerDetailHashItem), GetType(), classScope);
            method.Block.DeclareVar<EventType>(
                "eventType",
                EventTypeUtility.ResolveTypeCodegen(
                    FilterSpecCompiled.FilterForEventType,
                    symbols.GetAddInitSvc(method)));

            var symbolsWithType = new SAIFFInitializeSymbolWEventType();
            var methodLookupableMake = parent.MakeChildWithScope(
                    typeof(ExprFilterSpecLookupable),
                    GetType(),
                    symbolsWithType,
                    classScope)
                .AddParam(typeof(EventType), "eventType")
                .AddParam(typeof(EPStatementInitServices), SAIFFInitializeSymbol.REF_STMTINITSVC.Ref);
            var methodLookupable = Lookupable.MakeCodegen(methodLookupableMake, symbolsWithType, classScope);
            methodLookupableMake.Block.MethodReturn(LocalMethod(methodLookupable));

            method.Block
                .DeclareVarNewInstance<ContextControllerDetailHashItem>("item")
                .DeclareVar<ExprFilterSpecLookupable>(
                    "lookupable",
                    LocalMethod(methodLookupableMake, Ref("eventType"), symbols.GetAddInitSvc(method)))
                .SetProperty(
                    Ref("item"),
                    "FilterSpecActivatable",
                    LocalMethod(FilterSpecCompiled.MakeCodegen(method, symbols, classScope)))
                .SetProperty(Ref("item"), "Lookupable", Ref("lookupable"))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.FILTERSHAREDLOOKUPABLEREGISTERY)
                        .Add("RegisterLookupable", Ref("eventType"), Ref("lookupable")))
                .MethodReturn(Ref("item"));

            return LocalMethod(method);
        }
    }
} // end of namespace