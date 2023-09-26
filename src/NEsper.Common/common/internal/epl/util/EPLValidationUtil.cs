///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.epl.join.hint;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeOrigin;

using TypeExtensions = System.Reflection.TypeExtensions;

namespace com.espertech.esper.common.@internal.epl.util
{
    public class EPLValidationUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static ExprNode ValidateEventPrecedence(
            bool insertingIntoTable,
            ExprNode eventPrecedence,
            EventType resultEventType,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(resultEventType, null, true);
            var validationContext =
                new ExprValidationContextBuilder(streamTypeService, statementRawInfo, services).Build();
            ExprNode validated;
            try {
                validated = ExprNodeUtilityValidate.GetValidatedSubtree(
                    EVENTPRECEDENCE,
                    eventPrecedence,
                    validationContext);
            }
            catch (ExprValidationException ex) {
                throw new ExprValidationException(
                    "Failed to validate event-precedence considering only the output event type '" +
                    resultEventType.Metadata.Name +
                    "': " +
                    ex.Message +
                    " (NOTE: this validation only considers the result event itself and not incoming streams)",
                    ex);
            }

            var returned = validated.Forge.EvaluationType.GetBoxedType();
            if (typeof(int?) != returned) {
                throw new ExprValidationException(
                    "Event-precedence expected an expression returning an integer value but the expression '" +
                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(eventPrecedence) +
                    "' returns " +
                    returned.CleanName());
            }

            if (insertingIntoTable) {
                throw new ExprValidationException("Event-precedence is not allowed when inserting into a table");
            }

            return validated;
        }

        public static void ValidateParametersTypePredefined(
            ExprNode[] expressions,
            string invocableName,
            string invocableCategory,
            EPLExpressionParamType type)
        {
            for (var i = 0; i < expressions.Length; i++) {
                ValidateParameterType(
                    invocableName,
                    invocableCategory,
                    true,
                    type,
                    null,
                    expressions[i].Forge.EvaluationType,
                    i,
                    expressions[i]);
            }
        }

        public static void ValidateTableExists(
            TableCompileTimeResolver tableCompileTimeResolver,
            string name)
        {
            if (tableCompileTimeResolver.Resolve(name) != null) {
                throw new ExprValidationException("A table by name '" + name + "' already exists");
            }
        }

        public static ExprNode ValidateSimpleGetSubtree(
            ExprNodeOrigin origin,
            ExprNode expression,
            EventType optionalEventType,
            bool allowBindingConsumption,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            ExprNodeUtilityValidate.ValidatePlainExpression(origin, expression);

            StreamTypeServiceImpl streamTypes;
            if (optionalEventType != null) {
                streamTypes = new StreamTypeServiceImpl(optionalEventType, null, true);
            }
            else {
                streamTypes = new StreamTypeServiceImpl(false);
            }

            var validationContext = new ExprValidationContextBuilder(streamTypes, statementRawInfo, services)
                .WithAllowBindingConsumption(allowBindingConsumption)
                .Build();
            return ExprNodeUtilityValidate.GetValidatedSubtree(origin, expression, validationContext);
        }

        public static QueryGraphForge ValidateFilterGetQueryGraphSafe(
            ExprNode filterExpression,
            StreamTypeServiceImpl typeService,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            ExcludePlanHint excludePlanHint = null;
            try {
                excludePlanHint = ExcludePlanHint.GetHint(typeService.StreamNames, statementRawInfo, services);
            }
            catch (ExprValidationException ex) {
                Log.Warn("Failed to consider exclude-plan hint: " + ex.Message, ex);
            }

            var queryGraph = new QueryGraphForge(1, excludePlanHint, false);
            if (filterExpression != null) {
                ValidateFilterWQueryGraphSafe(queryGraph, filterExpression, typeService, statementRawInfo, services);
            }

            return queryGraph;
        }

        public static void ValidateFilterWQueryGraphSafe(
            QueryGraphForge queryGraph,
            ExprNode filterExpression,
            StreamTypeServiceImpl typeService,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            try {
                var validationContext = new ExprValidationContextBuilder(typeService, statementRawInfo, services)
                    .WithAllowBindingConsumption(true)
                    .WithIsFilterExpression(true)
                    .Build();
                var validated = ExprNodeUtilityValidate.GetValidatedSubtree(
                    FILTER,
                    filterExpression,
                    validationContext);
                FilterExprAnalyzer.Analyze(validated, queryGraph, false);
            }
            catch (Exception ex) {
                Log.Warn(
                    "Unexpected exception analyzing filterable expression '" +
                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(filterExpression) +
                    "': " +
                    ex.Message,
                    ex);
            }
        }

        public static void ValidateParameterNumber(
            string invocableName,
            string invocableCategory,
            bool isFunction,
            int expectedEnum,
            int receivedNum)
        {
            if (expectedEnum != receivedNum) {
                throw new ExprValidationException(
                    GetInvokablePrefix(invocableName, invocableCategory, isFunction) +
                    "expected " +
                    expectedEnum +
                    " parameters but received " +
                    receivedNum +
                    " parameters");
            }
        }

        public static void ValidateParameterType(
            String invocableName,
            String invocableCategory,
            bool isFunction,
            EPLExpressionParamType expectedTypeEnum,
            Type[] expectedTypeClasses,
            Type providedTypeCanNull,
            int parameterNum,
            ExprNode parameterExpression)
        {
            if (expectedTypeEnum == EPLExpressionParamType.ANY) {
                return;
            }

            if (providedTypeCanNull == null) {
                throw new ExprValidationException(
                    GetInvokablePrefix(invocableName, invocableCategory, isFunction) +
                    "expected a non-null result for expression parameter " +
                    parameterNum +
                    " but received a null-typed expression");
            }

            var providedType = providedTypeCanNull;
            if (expectedTypeEnum == EPLExpressionParamType.BOOLEAN && (!providedType.IsTypeBoolean())) {
                throw new ExprValidationException(
                    GetInvokablePrefix(invocableName, invocableCategory, isFunction) +
                    "expected a boolean-type result for expression parameter " +
                    parameterNum +
                    " but received " +
                    providedType.CleanName());
            }

            if (expectedTypeEnum == EPLExpressionParamType.NUMERIC && (!providedType.IsTypeNumeric())) {
                throw new ExprValidationException(
                    GetInvokablePrefix(invocableName, invocableCategory, isFunction) +
                    "expected a number-type result for expression parameter " +
                    parameterNum +
                    " but received " +
                    providedType.CleanName());
            }

            if (expectedTypeEnum == EPLExpressionParamType.SPECIFIC) {
                var boxedProvidedType = providedType.GetBoxedType();
                var found = false;
                foreach (var expectedTypeClass in expectedTypeClasses) {
                    var boxedExpectedType = expectedTypeClass.GetBoxedType();
                    if (boxedProvidedType != null &&
                        TypeHelper.IsSubclassOrImplementsInterface(boxedProvidedType, boxedExpectedType)) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    String expected;
                    if (expectedTypeClasses.Length == 1) {
                        expected = "a " + TypeHelper.GetParameterAsString(expectedTypeClasses);
                    }
                    else {
                        expected = "any of [" + TypeHelper.GetParameterAsString(expectedTypeClasses) + "]";
                    }

                    throw new ExprValidationException(
                        GetInvokablePrefix(invocableName, invocableCategory, isFunction) +
                        "expected " +
                        expected +
                        "-type result for expression parameter " +
                        parameterNum +
                        " but received " +
                        providedType.CleanName());
                }
            }

            if (expectedTypeEnum == EPLExpressionParamType.TIME_PERIOD_OR_SEC) {
                if (parameterExpression is ExprTimePeriod || parameterExpression is ExprStreamUnderlyingNode) {
                    return;
                }

                if (!(providedType.IsTypeNumeric())) {
                    throw new ExprValidationException(
                        GetInvokablePrefix(invocableName, invocableCategory, isFunction) +
                        "expected a time-period expression or a numeric-type result for expression parameter " +
                        parameterNum +
                        " but received " +
                        providedType.CleanName());
                }
            }

            if (expectedTypeEnum == EPLExpressionParamType.DATETIME) {
                if (!(TypeHelper.IsDateTime(providedType))) {
                    throw new ExprValidationException(
                        GetInvokablePrefix(invocableName, invocableCategory, isFunction) +
                        "expected a long-typed, Date-typed or Calendar-typed result for expression parameter " +
                        parameterNum +
                        " but received " +
                        providedType.CleanName());
                }
            }
        }

        public static string GetInvokablePrefix(
            string invocableName,
            string invocableType,
            bool isFunction)
        {
            return "Failed to validate " +
                   invocableType +
                   " " +
                   (isFunction ? "function '" : "method '") +
                   invocableName +
                   "', ";
        }

        public static void ValidateContextName(
            bool table,
            string tableOrNamedWindowName,
            string tableOrNamedWindowContextName,
            string optionalContextName,
            bool mustMatchContext)
        {
            if (tableOrNamedWindowContextName != null) {
                if (optionalContextName == null || !optionalContextName.Equals(tableOrNamedWindowContextName)) {
                    throw GetCtxMessage(table, tableOrNamedWindowName, tableOrNamedWindowContextName);
                }
            }
            else {
                if (mustMatchContext && optionalContextName != null) {
                    throw GetCtxMessage(table, tableOrNamedWindowName, tableOrNamedWindowContextName);
                }
            }
        }

        private static ExprValidationException GetCtxMessage(
            bool table,
            string tableOrNamedWindowName,
            string tableOrNamedWindowContextName)
        {
            var prefix = table ? "Table" : "Named window";
            return new ExprValidationException(
                prefix +
                " by name '" +
                tableOrNamedWindowName +
                "' has been declared for context '" +
                tableOrNamedWindowContextName +
                "' and can only be used within the same context");
        }

        public static void ValidateAlreadyExistsTableOrVariable(
            string name,
            VariableCompileTimeResolver variableCompileTimeResolver,
            TableCompileTimeResolver tableCompileTimeResolver,
            EventTypeCompileTimeResolver eventTypeCompileTimeResolver)
        {
            var existingTable = tableCompileTimeResolver.Resolve(name);
            if (existingTable != null) {
                throw new ExprValidationException("A table by name '" + name + "' has already been declared");
            }

            var existingVariable = variableCompileTimeResolver.Resolve(name);
            if (existingVariable != null) {
                throw new ExprValidationException("A variable by name '" + name + "' has already been declared");
            }

            var existingEventType = eventTypeCompileTimeResolver.GetTypeByName(name);
            if (existingEventType != null) {
                throw new ExprValidationException("An event type by name '" + name + "' has already been declared");
            }
        }
    }
} // end of namespace