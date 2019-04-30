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
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.keyed;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
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

        public string AliasName { get; }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ContextControllerDetailKeyedItem), GetType(), classScope);
            var types = EventTypeUtility.GetPropertyTypes(
                filterSpecCompiled.FilterForEventType, PropertyNames.ToArray());

            method.Block
                .DeclareVar(
                    typeof(FilterSpecActivatable), "activatable",
                    LocalMethod(filterSpecCompiled.MakeCodegen(method, symbols, classScope)))
                .DeclareVar(
                    typeof(ExprFilterSpecLookupable[]), "lookupables",
                    NewArrayByLength(typeof(ExprFilterSpecLookupable), Constant(getters.Length)));
            for (var i = 0; i < getters.Length; i++) {
                var getter = EventTypeUtility.CodegenGetterWCoerce(
                    getters[i], types[i], types[i], method, GetType(), classScope);
                var lookupable = NewInstance(
                    typeof(ExprFilterSpecLookupable), Constant(PropertyNames[i]), getter,
                    Constant(types[i]), ConstantFalse());
                var eventType = ExprDotMethod(Ref("activatable"), "getFilterForEventType");
                method.Block
                    .AssignArrayElement(Ref("lookupables"), Constant(i), lookupable)
                    .Expression(
                        ExprDotMethodChain(symbols.GetAddInitSvc(method))
                            .Add(EPStatementInitServicesConstants.GETFILTERSHAREDLOOKUPABLEREGISTERY).Add(
                                "registerLookupable", eventType, ArrayAtIndex(Ref("lookupables"), Constant(i))));
            }

            method.Block
                .DeclareVar(
                    typeof(ContextControllerDetailKeyedItem), "item",
                    NewInstance(typeof(ContextControllerDetailKeyedItem)))
                .SetProperty(Ref("item"), "Getter",
                    EventTypeUtility.CodegenGetterMayMultiKeyWCoerce(
                        filterSpecCompiled.FilterForEventType, getters, types, null, method, GetType(), classScope))
                .SetProperty(Ref("item"), "Lookupables", Ref("lookupables"))
                .SetProperty(Ref("item"), "PropertyTypes", Constant(types))
                .SetProperty(Ref("item"), "FilterSpecActivatable", Ref("activatable"))
                .SetProperty(Ref("item"), "AliasName", Constant(AliasName))
                .MethodReturn(Ref("item"));
            return LocalMethod(method);
        }
    }
} // end of namespace