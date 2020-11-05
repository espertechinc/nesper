///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.keyed;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    [Serializable]
    public class ContextSpecKeyedItem
    {
        private FilterSpecCompiled filterSpecCompiled;
        private EventPropertyGetterSPI[] getters;

        public ContextSpecKeyedItem(
            FilterSpecRaw filterSpecRaw,
            IList<string> propertyNames,
            string aliasName)
        {
            FilterSpecRaw = filterSpecRaw;
            PropertyNames = propertyNames;
            AliasName = aliasName;
        }

        public FilterSpecRaw FilterSpecRaw { get; }

        public IList<string> PropertyNames { get; }

        public FilterSpecCompiled FilterSpecCompiled {
            get => filterSpecCompiled;
            set => filterSpecCompiled = value;
        }

        public EventPropertyGetterSPI[] Getters {
            set => getters = value;
            get => getters;
        }

        public MultiKeyClassRef KeyMultiKey { get; set; }
        
        public DataInputOutputSerdeForge[] LookupableSerdes { get; set; }

        public string AliasName { get; }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ContextControllerDetailKeyedItem), GetType(), classScope);
            var types = EventTypeUtility.GetPropertyTypes(
                filterSpecCompiled.FilterForEventType,
                PropertyNames.ToArray());

            method.Block
                .DeclareVar<FilterSpecActivatable>(
                    "activatable",
                    LocalMethod(filterSpecCompiled.MakeCodegen(method, symbols, classScope)))
                .DeclareVar<ExprFilterSpecLookupable[]>(
                    "lookupables",
                    NewArrayByLength(typeof(ExprFilterSpecLookupable), Constant(getters.Length)));
            for (var i = 0; i < getters.Length; i++) {
                CodegenExpression getterX = EventTypeUtility.CodegenGetterWCoerceWArray(
                    typeof(ExprEventEvaluator), 
                    getters[i],
                    types[i],
                    types[i],
                    method,
                    GetType(),
                    classScope);
                CodegenExpression lookupable = NewInstance<ExprFilterSpecLookupable>(
                    Constant(PropertyNames[i]),
                    getterX,
                    ConstantNull(),
                    Constant(types[i]),
                    ConstantFalse(),
                    LookupableSerdes[i].Codegen(method, classScope, null));

                var eventType = ExprDotName(Ref("activatable"), "FilterForEventType");
                method.Block
                    .AssignArrayElement(Ref("lookupables"), Constant(i), lookupable)
                    .Expression(
                        ExprDotMethodChain(symbols.GetAddInitSvc(method))
                            .Get(EPStatementInitServicesConstants.FILTERSHAREDLOOKUPABLEREGISTERY)
                            .Add(
                                "RegisterLookupable",
                                eventType,
                                ArrayAtIndex(Ref("lookupables"), Constant(i))));
            }

            CodegenExpression getter = MultiKeyCodegen.CodegenGetterMayMultiKey(
                filterSpecCompiled.FilterForEventType,
                getters,
                types,
                null,
                KeyMultiKey,
                method,
                classScope);

            method.Block
                .DeclareVar<ContextControllerDetailKeyedItem>(
                    "item",
                    NewInstance(typeof(ContextControllerDetailKeyedItem)))
                .SetProperty(Ref("item"), "Getter", getter)
                .SetProperty(Ref("item"), "Lookupables", Ref("lookupables"))
                .SetProperty(Ref("item"), "PropertyTypes", Constant(types))
                .SetProperty(Ref("item"), "KeySerde", KeyMultiKey.GetExprMKSerde(method, classScope))
                .SetProperty(Ref("item"), "FilterSpecActivatable", Ref("activatable"))
                .SetProperty(Ref("item"), "AliasName", Constant(AliasName))
                .MethodReturn(Ref("item"));
            return LocalMethod(method);
        }
    }
} // end of namespace