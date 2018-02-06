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
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.table
{
    public class ExprTableNodeUtil
    {
        public static void ValidateExpressions(
            string tableName,
            Type[] providedTypes,
            string providedName,
            IList<ExprNode> providedExpr,
            Type[] expectedTypes,
            string expectedName)
        {
            if (expectedTypes.Length != providedTypes.Length)
            {
                string actual = (providedTypes.Length == 0 ? "no" : "" + providedTypes.Length) + " " + providedName + " expressions";
                string expected = (expectedTypes.Length == 0 ? "no" : "" + expectedTypes.Length) + " " + expectedName + " expressions";
                throw new ExprValidationException(
                    string.Format(
                        "Incompatible number of {0} expressions for use with table '{1}', the table expects {2} and provided are {3}",
                        providedName, tableName, expected, actual));
            }

            for (int i = 0; i < expectedTypes.Length; i++)
            {
                var actual = providedTypes[i].GetBoxedType();
                var expected = expectedTypes[i].GetBoxedType();
                if (!TypeHelper.IsSubclassOrImplementsInterface(actual, expected))
                {
                    throw new ExprValidationException(
                        string.Format(
                            "Incompatible type returned by a {0} expression for use with table '{1}', the {0} expression '{2}' returns '{3}' but the table expects '{4}'",
                            providedName,
                            tableName,
                            ExprNodeUtility.ToExpressionStringMinPrecedenceAsList(providedExpr),
                            actual.GetCleanName(),
                            expected.GetCleanName()));
                }
            }
        }
    }
}
