///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.support.filter
{
    public class FilterTestCaseSingleField
    {
        public FilterTestCaseSingleField(
            string filterExpr,
            string fieldName,
            object[] values,
            bool[] isInvoked)
        {
            FilterExpr = filterExpr;
            FieldName = fieldName;
            Values = values;
            IsInvoked = isInvoked;
        }

        public string FilterExpr { get; }

        public string FieldName { get; }

        public object[] Values { get; }

        public bool[] IsInvoked { get; set; }

        public static void AddCase(
            IList<FilterTestCaseSingleField> testCases,
            string filterExpr,
            string fieldName,
            object[] values,
            bool[] isInvoked)
        {
            testCases.Add(new FilterTestCaseSingleField(filterExpr, fieldName, values, isInvoked));
        }
    }
} // end of namespace