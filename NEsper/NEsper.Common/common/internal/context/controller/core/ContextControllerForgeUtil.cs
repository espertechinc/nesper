///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.core
{
    public class ContextControllerForgeUtil
    {
        public static void ValidateStatementKeyAndHash(
            IEnumerable<Supplier<EventType>> typeProvider,
            string contextName,
            StatementSpecCompiled spec,
            StatementCompileTimeServices compileTimeServices)
        {
            StatementSpecCompiledAnalyzerResult streamAnalysis = StatementSpecCompiledAnalyzer.AnalyzeFilters(spec);
            IList<FilterSpecCompiled> filters = streamAnalysis.Filters;

            var isCreateWindow = spec.Raw.CreateWindowDesc != null;

            // if no create-window: at least one of the filters must match one of the filters specified by the context
            if (!isCreateWindow) {
                foreach (var filter in filters) {
                    foreach (var item in typeProvider) {
                        EventType itemEventType = item.Invoke();
                        var stmtFilterType = filter.FilterForEventType;
                        if (ReferenceEquals(stmtFilterType, itemEventType)) {
                            return;
                        }

                        if (EventTypeUtility.IsTypeOrSubTypeOf(stmtFilterType, itemEventType)) {
                            return;
                        }

                        NamedWindowMetaData namedWindow =
                            compileTimeServices.NamedWindowCompileTimeResolver.Resolve(stmtFilterType.Name);
                        if (namedWindow != null) {
                            string namedWindowContextName = namedWindow.ContextName;
                            if (namedWindowContextName != null && namedWindowContextName.Equals(contextName)) {
                                return;
                            }
                        }
                    }
                }

                if (!filters.IsEmpty()) {
                    throw new ExprValidationException(
                        GetTypeValidationMessage(contextName, filters[0].FilterForEventType.Name));
                }

                return;
            }

            // validate create-window with column definition: not allowed, requires typed
            if (spec.Raw.CreateWindowDesc.Columns != null &&
                spec.Raw.CreateWindowDesc.Columns.Count > 0) {
                throw new ExprValidationException(
                    "Segmented context '" + contextName +
                    "' requires that named windows are associated to an existing event type and that the event type is listed among the partitions defined by the create-context statement");
            }

            // validate create-window declared type
            var declaredAsName = spec.Raw.CreateWindowDesc.AsEventTypeName;
            if (declaredAsName != null) {
                foreach (var item in typeProvider) {
                    EventType itemEventType = item.Invoke();
                    if (itemEventType.Name.Equals(declaredAsName)) {
                        return;
                    }
                }

                throw new ExprValidationException(GetTypeValidationMessage(contextName, declaredAsName));
            }
        }

        private static string GetTypeValidationMessage(
            string contextName,
            string typeNameEx)
        {
            return "Segmented context '" + contextName +
                   "' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type '" +
                   typeNameEx + "' is not one of the types listed";
        }
    }
} // end of namespace