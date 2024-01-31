///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.hook.type;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportSQLOutputRowConversion : SQLOutputRowConversion
    {
        static SupportSQLOutputRowConversion()
        {
            Reset();
        }

        public static IList<SQLOutputRowTypeContext> TypeContexts { get; private set; }

        public static IList<SQLOutputRowValueContext> ValueContexts { get; private set; }

        public Type GetOutputRowType(SQLOutputRowTypeContext sqlOutputRowTypeContext)
        {
            TypeContexts.Add(sqlOutputRowTypeContext);
            return typeof(SupportBean);
        }

        public object GetOutputRow(SQLOutputRowValueContext rowContext)
        {
            var myint = rowContext.Values.Get("myint").AsInt32();
            if (myint == 90) {
                return null;
            }

            ValueContexts.Add(rowContext);
            return new SupportBean(">" + myint + "<", 99000 + myint);
        }

        public static void Reset()
        {
            TypeContexts = new List<SQLOutputRowTypeContext>();
            ValueContexts = new List<SQLOutputRowValueContext>();
        }
    }
} // end of namespace