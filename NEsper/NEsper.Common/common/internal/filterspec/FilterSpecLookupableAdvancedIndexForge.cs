///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterSpecLookupableAdvancedIndexForge : ExprFilterSpecLookupableForge
    {
        private readonly EventPropertyGetterSPI height;
        private readonly EventPropertyGetterSPI width;
        private readonly EventPropertyGetterSPI x;
        private readonly EventPropertyGetterSPI y;

        public FilterSpecLookupableAdvancedIndexForge(
            string expression,
            EventPropertyGetterSPI getter,
            Type returnType,
            AdvancedIndexConfigContextPartitionQuadTree quadTreeConfig,
            EventPropertyGetterSPI x,
            EventPropertyGetterSPI y,
            EventPropertyGetterSPI width,
            EventPropertyGetterSPI height,
            string indexType)
            : base(expression, getter, returnType, true)
        {
            QuadTreeConfig = quadTreeConfig;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            IndexType = indexType;
        }

        public EventPropertyGetter X => x;

        public EventPropertyGetter Y => y;

        public EventPropertyGetter Width => width;

        public EventPropertyGetter Height => height;

        public AdvancedIndexConfigContextPartitionQuadTree QuadTreeConfig { get; }

        public string IndexType { get; }

        public override CodegenMethod MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(FilterSpecLookupableAdvancedIndex), typeof(FilterSpecLookupableAdvancedIndexForge), classScope);
            Func<EventPropertyGetterSPI, CodegenExpression> toEval = getter =>
                EventTypeUtility.CodegenGetterWCoerce(getter, typeof(double?), null, method, GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(FilterSpecLookupableAdvancedIndex), "lookupable", NewInstance<FilterSpecLookupableAdvancedIndex>(
                        Constant(expression), ConstantNull(), EnumValue(returnType, "class")))
                .SetProperty(Ref("lookupable"), "QuadTreeConfig", QuadTreeConfig.Make())
                .SetProperty(Ref("lookupable"), "X", toEval.Invoke(x))
                .SetProperty(Ref("lookupable"), "Y", toEval.Invoke(y))
                .SetProperty(Ref("lookupable"), "Width", toEval.Invoke(width))
                .SetProperty(Ref("lookupable"), "Height", toEval.Invoke(height))
                .SetProperty(Ref("lookupable"), "IndexType", Constant(IndexType))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Add(EPStatementInitServicesConstants.GETFILTERSHAREDLOOKUPABLEREGISTERY).Add(
                            "registerLookupable", symbols.GetAddEventType(method), Ref("lookupable")))
                .MethodReturn(Ref("lookupable"));
            return method;
        }
    }
} // end of namespace