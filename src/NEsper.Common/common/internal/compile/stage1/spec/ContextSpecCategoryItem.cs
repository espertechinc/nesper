///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.category;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class ContextSpecCategoryItem
    {
        public ContextSpecCategoryItem(
            ExprNode expression,
            string name)
        {
            Expression = expression;
            Name = name;
        }

        public ExprNode Expression { get; }

        public string Name { get; }

        public FilterSpecPlanForge FilterPlan { get; set; }

        public CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            CodegenMethod method = parent.MakeChild(typeof(ContextControllerDetailCategoryItem), GetType(), classScope)
                .AddParam(typeof(EventType), SAIFFInitializeSymbolWEventType.REF_EVENTTYPE.Ref)
                .AddParam(typeof(EPStatementInitServices), SAIFFInitializeSymbol.REF_STMTINITSVC.Ref);

            CodegenMethod makeFilter = FilterPlan.CodegenWithEventType(method, classScope);

            method.Block
                .DeclareVar<FilterSpecPlan>(
                    "filterPlan",
                    LocalMethod(
                        makeFilter,
                        SAIFFInitializeSymbolWEventType.REF_EVENTTYPE,
                        SAIFFInitializeSymbol.REF_STMTINITSVC))
                .DeclareVar<ContextControllerDetailCategoryItem>(
                    "item",
                    NewInstance(typeof(ContextControllerDetailCategoryItem)))
                .SetProperty(Ref("item"), "FilterPlan", Ref("filterPlan"))
                .SetProperty(Ref("item"), "Name", Constant(Name))
                .MethodReturn(Ref("item"));
            return method;
        }
    }
} // end of namespace