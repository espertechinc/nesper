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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
	public class ContextSpecHashItem {

	    private readonly ExprChainedSpec function;
	    private readonly FilterSpecRaw filterSpecRaw;

	    private FilterSpecCompiled filterSpecCompiled;
	    private ExprFilterSpecLookupableForge lookupable;

	    public ContextSpecHashItem(ExprChainedSpec function, FilterSpecRaw filterSpecRaw) {
	        this.function = function;
	        this.filterSpecRaw = filterSpecRaw;
	    }

	    public ExprChainedSpec Function {
	        get => function;
	    }

	    public FilterSpecRaw FilterSpecRaw {
	        get => filterSpecRaw;
	    }

	    public FilterSpecCompiled FilterSpecCompiled {
	        get => filterSpecCompiled;
	    }

	    public void SetFilterSpecCompiled(FilterSpecCompiled filterSpecCompiled) {
	        this.filterSpecCompiled = filterSpecCompiled;
	    }

	    public ExprFilterSpecLookupableForge Lookupable {
	        get => lookupable;
	    }

	    public void SetLookupable(ExprFilterSpecLookupableForge lookupable) {
	        this.lookupable = lookupable;
	    }

	    public CodegenExpression MakeCodegen(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	        CodegenMethod method = parent.MakeChild(typeof(ContextControllerDetailHashItem), this.GetType(), classScope);
	        method.Block.DeclareVar(typeof(EventType), "eventType", EventTypeUtility.ResolveTypeCodegen(filterSpecCompiled.FilterForEventType, symbols.GetAddInitSvc(method)));

	        SAIFFInitializeSymbolWEventType symbolsWithType = new SAIFFInitializeSymbolWEventType();
	        CodegenMethod methodLookupableMake = parent.MakeChildWithScope(typeof(ExprFilterSpecLookupable), this.GetType(), symbolsWithType, classScope).AddParam(typeof(EventType), "eventType").AddParam(typeof(EPStatementInitServices), SAIFFInitializeSymbolWEventType.REF_STMTINITSVC.Ref);
	        CodegenMethod methodLookupable = lookupable.MakeCodegen(methodLookupableMake, symbolsWithType, classScope);
	        methodLookupableMake.Block.MethodReturn(LocalMethod(methodLookupable));

	        method.Block
	                .DeclareVar(typeof(ContextControllerDetailHashItem), "item", NewInstance(typeof(ContextControllerDetailHashItem)))
	                .DeclareVar(typeof(ExprFilterSpecLookupable), "lookupable", LocalMethod(methodLookupableMake, @Ref("eventType"), symbols.GetAddInitSvc(method)))
	                .ExprDotMethod(@Ref("item"), "setFilterSpecActivatable", LocalMethod(filterSpecCompiled.MakeCodegen(method, symbols, classScope)))
	                .ExprDotMethod(@Ref("item"), "setLookupable", @Ref("lookupable"))
	                .Expression(ExprDotMethodChain(symbols.GetAddInitSvc(method)).Add(EPStatementInitServicesConstants.GETFILTERSHAREDLOOKUPABLEREGISTERY).Add("registerLookupable", @Ref("eventType"), @Ref("lookupable")))
	                .MethodReturn(@Ref("item"));

	        return LocalMethod(method);
	    }
	}
} // end of namespace