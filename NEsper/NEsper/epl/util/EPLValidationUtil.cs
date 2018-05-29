///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.util
{
    /// <summary>
    /// Defines the <see cref="EPLValidationUtil" />
    /// </summary>
    public class EPLValidationUtil
    {
        public static void ValidateParameterNumber(
            string invocableName,
            string invocableCategory,
            bool isFunction, 
            int expectedEnum, 
            int receivedNum)
        {
            if (expectedEnum != receivedNum)
            {
                throw new ExprValidationException(GetInvokablePrefix(invocableName, invocableCategory, isFunction) + "expected " + expectedEnum + " parameters but received " + receivedNum + " parameters");
            }
        }

        public static void ValidateParameterType(
            string invocableName, 
            string invocableCategory,
            bool isFunction, 
            EPLExpressionParamType expectedTypeEnum,
            Type[] expectedTypeClasses, 
            Type providedType,
            int parameterNum,
            ExprNode parameterExpression)
        {
            if (expectedTypeEnum == EPLExpressionParamType.BOOLEAN && (!providedType.IsBoolean()))
            {
                throw new ExprValidationException(GetInvokablePrefix(invocableName, invocableCategory, isFunction) + "expected a bool-type result for expression parameter " + parameterNum + " but received " + providedType.GetCleanName());
            }
            if (expectedTypeEnum == EPLExpressionParamType.NUMERIC && (!providedType.IsNumeric()))
            {
                throw new ExprValidationException(GetInvokablePrefix(invocableName, invocableCategory, isFunction) + "expected a number-type result for expression parameter " + parameterNum + " but received " + providedType.GetCleanName());
            }
            if (expectedTypeEnum == EPLExpressionParamType.SPECIFIC)
            {
                var boxedProvidedType = providedType.GetBoxedType();
                var found = false;
                foreach (var expectedTypeClass in expectedTypeClasses)
                {
                    var boxedExpectedType = expectedTypeClass.GetBoxedType();
                    if (boxedProvidedType != null && TypeHelper.IsSubclassOrImplementsInterface(boxedProvidedType, boxedExpectedType))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    string expected;
                    if (expectedTypeClasses.Length == 1)
                    {
                        expected = "a " + TypeHelper.GetParameterAsString(expectedTypeClasses);
                    }
                    else
                    {
                        expected = "any of [" + TypeHelper.GetParameterAsString(expectedTypeClasses) + "]";
                    }
                    throw new ExprValidationException(GetInvokablePrefix(invocableName, invocableCategory, isFunction) + "expected " + expected + "-type result for expression parameter " + parameterNum + " but received " + providedType.GetCleanName());
                }
            }
            if (expectedTypeEnum == EPLExpressionParamType.TIME_PERIOD_OR_SEC)
            {
                if (parameterExpression is ExprTimePeriod || parameterExpression is ExprStreamUnderlyingNode)
                {
                    return;
                }
                if (!(TypeHelper.IsNumeric(providedType)))
                {
                    throw new ExprValidationException(GetInvokablePrefix(invocableName, invocableCategory, isFunction) + "expected a time-period expression or a numeric-type result for expression parameter " + parameterNum + " but received " + providedType.GetCleanName());
                }
            }
            if (expectedTypeEnum == EPLExpressionParamType.DATETIME)
            {
                if (!TypeHelper.IsDateTime(providedType))
                {
                    throw new ExprValidationException(GetInvokablePrefix(invocableName, invocableCategory, isFunction) + "expected a long-typed, Date-typed or Calendar-typed result for expression parameter " + parameterNum + " but received " + providedType.GetCleanName());
                }
            }
        }

        public static void ValidateTableExists(TableService tableService, string name)
        {
            if (tableService.GetTableMetadata(name) != null)
            {
                throw new ExprValidationException($"A table by name '{name}' already exists");
            }
        }

        public static void ValidateContextName(bool table, string tableOrNamedWindowName, string tableOrNamedWindowContextName, string optionalContextName, bool mustMatchContext)
        {
            if (tableOrNamedWindowContextName != null)
            {
                if (optionalContextName == null || !optionalContextName.Equals(tableOrNamedWindowContextName))
                {
                    throw GetCtxMessage(table, tableOrNamedWindowName, tableOrNamedWindowContextName);
                }
            }
            else
            {
                if (mustMatchContext && optionalContextName != null)
                {
                    throw GetCtxMessage(table, tableOrNamedWindowName, tableOrNamedWindowContextName);
                }
            }
        }

        private static ExprValidationException GetCtxMessage(bool table, string tableOrNamedWindowName, string tableOrNamedWindowContextName)
        {
            string prefix = table ? "Table" : "Named window";
            return new ExprValidationException($"{prefix} by name '{tableOrNamedWindowName}' has been declared for context '{tableOrNamedWindowContextName}' and can only be used within the same context");
        }

        public static string GetInvokablePrefix(string invocableName, string invocableType, bool isFunction)
        {
            return "Error validating " + invocableType + " " + (isFunction ? "function '" : "method '") + invocableName + "', ";
        }

        public static void ValidateParametersTypePredefined(
            IList<ExprNode> expressions, string invocableName, string invocableCategory, EPLExpressionParamType type)
        {
            for (int i = 0; i < expressions.Count; i++)
            {
                EPLValidationUtil.ValidateParameterType(
                    invocableName, 
                    invocableCategory, true, type, null, 
                    expressions[i].ExprEvaluator.ReturnType,
                    i, expressions[i]);
            }
        }
    }
}
