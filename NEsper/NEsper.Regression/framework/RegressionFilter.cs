///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.framework
{
    public class RegressionFilter
    {
        private const string TEST_SYSTEM_PROPERTY = "esper_test";

        public static ICollection<T> FilterBySystemProperty<T>(ICollection<T> executions)
            where T : RegressionExecution
        {
            var property = Environment.GetEnvironmentVariable(TEST_SYSTEM_PROPERTY);
            if (property == null) {
                return executions;
            }

            IList<T> filtered = new List<T>();
            foreach (var execution in executions) {
                var simpleName = execution.GetType().Name;
                if (simpleName.Equals(property)) {
                    filtered.Add(execution);
                }
            }

            return filtered;
        }
    }
} // end of namespace