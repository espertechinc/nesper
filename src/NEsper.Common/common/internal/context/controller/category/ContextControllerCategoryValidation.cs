///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.category
{
    public class ContextControllerCategoryValidation : ContextControllerPortableInfo
    {
        private readonly EventType categoryEventType;

        public EventType CategoryEventType {
            get => categoryEventType;
            set => throw new System.NotImplementedException();
        }

        public ContextControllerCategoryValidation(EventType categoryEventType)
        {
            CategoryEventType = categoryEventType;
        }

        public CodegenExpression Make(CodegenExpressionRef addInitSvc)
        {
            return NewInstance<ContextControllerCategoryValidation>(
                EventTypeUtility.ResolveTypeCodegen(CategoryEventType, addInitSvc));
        }

        public void ValidateStatement(
            string contextName,
            StatementSpecCompiled spec,
            StatementCompileTimeServices compileTimeServices)
        {
            var streamAnalysis = StatementSpecCompiledAnalyzer.AnalyzeFilters(spec);
            var filters = streamAnalysis.Filters;

            // Category validation
            var isCreateWindow = spec.Raw.CreateWindowDesc != null;
            var message = "Category context '" +
                          contextName +
                          "' requires that any of the events types that are listed in the category context also appear in any of the filter expressions of the statement";

            // if no create-window: at least one of the filters must match one of the filters specified by the context
            if (!isCreateWindow) {
                foreach (var filter in filters) {
                    var stmtFilterType = filter.FilterForEventType;
                    if (stmtFilterType == CategoryEventType) {
                        return;
                    }

                    if (EventTypeUtility.IsTypeOrSubTypeOf(stmtFilterType, CategoryEventType)) {
                        return;
                    }
                }

                if (!filters.IsEmpty()) {
                    throw new ExprValidationException(message);
                }

                return;
            }

            // validate create-window
            var declaredAsName = spec.Raw.CreateWindowDesc.AsEventTypeName;
            if (declaredAsName != null) {
                if (CategoryEventType.Name.Equals(declaredAsName)) {
                    return;
                }

                throw new ExprValidationException(message);
            }
        }

        public void VisitFilterAddendumEventTypes(Consumer<EventType> consumer)
        {
            consumer.Invoke(CategoryEventType);
        }
    }
} // end of namespace