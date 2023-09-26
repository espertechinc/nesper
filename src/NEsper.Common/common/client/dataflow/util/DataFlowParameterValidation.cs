///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.dataflow.util
{
    /// <summary>
    /// Utility for validation data flow forge parameters
    /// </summary>
    public class DataFlowParameterValidation
    {
        /// <summary>
        /// Validate the provided expression.
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="expr">expression</param>
        /// <param name="expectedReturnType">expected result type</param>
        /// <param name="context">forge initialization context</param>
        /// <returns>validated expression node</returns>
        /// <throws>ExprValidationException when validation failed</throws>
        public static ExprNode Validate(
            string name,
            ExprNode expr,
            Type expectedReturnType,
            DataFlowOpForgeInitializeContext context)
        {
            if (expr == null) {
                return null;
            }

            return Validate(name, expr, null, expectedReturnType, context);
        }

        /// <summary>
        /// Validate the provided expression.
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="eventType">event type</param>
        /// <param name="expr">expression</param>
        /// <param name="expectedReturnType">expected result type</param>
        /// <param name="context">forge initialization context</param>
        /// <returns>validated expression node</returns>
        /// <throws>ExprValidationException when validation failed</throws>
        public static ExprNode Validate(
            string name,
            ExprNode expr,
            EventType eventType,
            Type expectedReturnType,
            DataFlowOpForgeInitializeContext context)
        {
            if (expr == null) {
                return null;
            }

            var validated = EPLValidationUtil.ValidateSimpleGetSubtree(
                ExprNodeOrigin.DATAFLOWFILTER,
                expr,
                eventType,
                false,
                context.StatementRawInfo,
                context.Services);
            ValidateReturnType(name, validated, expectedReturnType);
            return validated;
        }

        private static void ValidateReturnType(
            string name,
            ExprNode validated,
            Type expectedReturnType)
        {
            var returnType = validated.Forge.EvaluationType;
            if (returnType == null) {
                throw MakeValidateReturnTypeEx(name, "null", expectedReturnType);
            }

            if (!returnType.GetBoxedType().IsAssignmentCompatible(expectedReturnType)) {
                throw MakeValidateReturnTypeEx(name, returnType.CleanName(), expectedReturnType);
            }
        }

        private static ExprValidationException MakeValidateReturnTypeEx(
            string name,
            string received,
            Type expected)
        {
            var message = "Failed to validate return type of parameter '" +
                          name +
                          "', expected '" +
                          expected.CleanName() +
                          "' but received '" +
                          received +
                          "'";
            return new ExprValidationException(message);
        }
    }
} // end of namespace