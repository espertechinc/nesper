///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

// import static com.espertech.esper.util.JavaClassHelper.isNumeric;

namespace com.espertech.esper.epl.index.service
{
    public class AdvancedIndexValidationHelper
    {
        public static void ValidateColumnCount(
            int expected, string indexTypeName, int colCount)
        {
            if (expected != colCount)
                throw new ExprValidationException("Index of type '" + indexTypeName + "' requires " + expected +
                                                  " expressions as index columns but received " + colCount);
        }

        public static void ValidateParameterCount(
            int minExpected, int maxExpected, string indexTypeName, int paramCount)
        {
            if (paramCount < minExpected || paramCount > maxExpected)
                throw new ExprValidationException("Index of type '" + indexTypeName + "' requires at least " +
                                                  minExpected + " parameters but received " + paramCount);
        }

        public static void ValidateParameterCountEither(
            int expectedOne, int expectedTwo, string indexTypeName, int paramCount)
        {
            if (paramCount != expectedOne && paramCount != expectedTwo)
                throw new ExprValidationException("Index of type '" + indexTypeName + "' requires at either " +
                                                  expectedOne + " or " + expectedTwo + " parameters but received " +
                                                  paramCount);
        }

        public static void ValidateColumnReturnTypeNumber(
            string indexTypeName, int colnum, ExprNode expr, string name)
        {
            var receivedType = expr.ExprEvaluator.ReturnType;
            if (!receivedType.IsNumeric()) throw MakeEx(indexTypeName, true, colnum, name, "numeric", receivedType);
        }

        public static void ValidateParameterReturnType(
            Type expectedReturnType, string indexTypeName, int paramnum, ExprNode expr, string name)
        {
            var receivedType = expr.ExprEvaluator.ReturnType.GetBoxedType();
            if (!TypeHelper.IsSubclassOrImplementsInterface(receivedType, expectedReturnType))
                throw MakeEx(indexTypeName, false, paramnum, name, expectedReturnType, receivedType);
        }

        public static void ValidateParameterReturnTypeNumber(
            string indexTypeName, int paramnum, ExprNode expr, string name)
        {
            var receivedType = expr.ExprEvaluator.ReturnType;
            if (!receivedType.IsNumeric()) throw MakeEx(indexTypeName, false, paramnum, name, "numeric", receivedType);
        }

        private static ExprValidationException MakeEx(
            string indexTypeName, bool isColumn, int num, string name, Type expectedType, Type receivedType)
        {
            return MakeEx(indexTypeName, isColumn, num, name,
                expectedType.GetCleanName(),
                receivedType.GetCleanName());
        }

        private static ExprValidationException MakeEx(
            string indexTypeName, bool isColumn, int num, string name, string expectedType, Type receivedType)
        {
            return MakeEx(indexTypeName, isColumn, num, name,
                expectedType,
                receivedType.GetCleanName());
        }

        private static ExprValidationException MakeEx(
            string indexTypeName, bool isColumn, int num, string name, string expectedType, string receivedType)
        {
            var columnOrParameter = isColumn ? "column " : "parameter ";
            return new ExprValidationException(
                "Index of type '" + indexTypeName + "' for " +
                columnOrParameter +
                +num + " that is providing " + name + "-values expecting type " +
                expectedType +
                " but received type " +
                receivedType);
        }
    }
} // end of namespace