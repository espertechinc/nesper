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
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterSpecLookupableAdvancedIndexForge : ExprFilterSpecLookupableForge
    {
        private readonly EventPropertyGetterSPI _height;
        private readonly EventPropertyGetterSPI _width;
        private readonly EventPropertyGetterSPI _x;
        private readonly EventPropertyGetterSPI _y;

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
            : base(
                expression,
                new ExprEventEvaluatorForgeFromProp(getter),
                null,
                returnType,
                true,
                DataInputOutputSerdeForgeSkip.INSTANCE)
        {
            QuadTreeConfig = quadTreeConfig;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            IndexType = indexType;
        }

        public EventPropertyGetter X => _x;

        public EventPropertyGetter Y => _y;

        public EventPropertyGetter Width => _width;

        public EventPropertyGetter Height => _height;

        public AdvancedIndexConfigContextPartitionQuadTree QuadTreeConfig { get; }

        public string IndexType { get; }

        public override CodegenMethod MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(FilterSpecLookupableAdvancedIndex),
                typeof(FilterSpecLookupableAdvancedIndexForge),
                classScope);
            Func<EventPropertyGetterSPI, CodegenExpression> toEval = getter =>
                EventTypeUtility.CodegenGetterWCoerce(getter, typeof(double?), null, method, GetType(), classScope);
            method.Block
                .DeclareVar<FilterSpecLookupableAdvancedIndex>(
                    "lookupable",
                    NewInstance<FilterSpecLookupableAdvancedIndex>(
                        Constant(Expression),
                        ConstantNull(),
                        Typeof(ReturnType)))
                .SetProperty(Ref("lookupable"), "QuadTreeConfig", QuadTreeConfig.Make())
                .SetProperty(Ref("lookupable"), "X", toEval.Invoke(_x))
                .SetProperty(Ref("lookupable"), "Y", toEval.Invoke(_y))
                .SetProperty(Ref("lookupable"), "Width", toEval.Invoke(_width))
                .SetProperty(Ref("lookupable"), "Height", toEval.Invoke(_height))
                .SetProperty(Ref("lookupable"), "IndexType", Constant(IndexType))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.FILTERSHAREDLOOKUPABLEREGISTERY)
                        .Add(
                            "RegisterLookupable",
                            symbols.GetAddEventType(method),
                            Ref("lookupable")))
                .MethodReturn(Ref("lookupable"));
            return method;
        }
    }
} // end of namespace