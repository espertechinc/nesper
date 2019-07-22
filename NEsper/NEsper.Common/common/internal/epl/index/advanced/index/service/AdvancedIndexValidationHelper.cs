///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.util.TypeHelper;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.service
{
    public class AdvancedIndexValidationHelper
    {
        public static void ValidateColumnCount(
            int expected,
            string indexTypeName,
            int colCount)
        {
            if (expected != colCount) {
                throw new ExprValidationException(
                    "Index of type '" +
                    indexTypeName +
                    "' requires " +
                    expected +
                    " expressions as index columns but received " +
                    colCount);
            }
        }

        public static void ValidateParameterCount(
            int minExpected,
            int maxExpected,
            string indexTypeName,
            int paramCount)
        {
            if (paramCount < minExpected || paramCount > maxExpected) {
                throw new ExprValidationException(
                    "Index of type '" +
                    indexTypeName +
                    "' requires at least " +
                    minExpected +
                    " parameters but received " +
                    paramCount);
            }
        }

        public static void ValidateParameterCountEither(
            int expectedOne,
            int expectedTwo,
            string indexTypeName,
            int paramCount)
        {
            if (paramCount != expectedOne && paramCount != expectedTwo) {
                throw new ExprValidationException(
                    "Index of type '" +
                    indexTypeName +
                    "' requires at either " +
                    expectedOne +
                    " or " +
                    expectedTwo +
                    " parameters but received " +
                    paramCount);
            }
        }

        public static void ValidateColumnReturnTypeNumber(
            string indexTypeName,
            int colnum,
            ExprNode expr,
            string name)
        {
            var receivedType = expr.Forge.EvaluationType;
            if (!TypeHelper.IsNumeric(receivedType)) {
                throw MakeEx(indexTypeName, true, colnum, name, typeof(object), receivedType);
            }
        }

        public static void ValidateParameterReturnType(
            Type expectedReturnType,
            string indexTypeName,
            int paramnum,
            ExprNode expr,
            string name)
        {
            Type receivedType = Boxing.GetBoxedType(expr.Forge.EvaluationType);
            if (!IsSubclassOrImplementsInterface(receivedType, expectedReturnType)) {
                throw MakeEx(indexTypeName, false, paramnum, name, expectedReturnType, receivedType);
            }
        }

        public static void ValidateParameterReturnTypeNumber(
            string indexTypeName,
            int paramnum,
            ExprNode expr,
            string name)
        {
            var receivedType = expr.Forge.EvaluationType;
            if (!TypeHelper.IsNumeric(receivedType)) {
                throw MakeEx(indexTypeName, false, paramnum, name, typeof(object), receivedType);
            }
        }

        private static ExprValidationException MakeEx(
            string indexTypeName,
            bool isColumn,
            int num,
            string name,
            Type expectedType,
            Type receivedType)
        {
            return new ExprValidationException(
                "Index of type '" +
                indexTypeName +
                "' for " +
                (isColumn ? "column " : "parameter ") +
                +num +
                " that is providing " +
                name +
                "-values expecting type " +
                TypeHelper.GetCleanName(expectedType) +
                " but received type " +
                TypeHelper.GetCleanName(receivedType));
        }
    }
} // end of namespace