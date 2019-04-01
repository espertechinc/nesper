///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.db
{
    public class SupportSQLOutputRowConversion : SQLOutputRowConversion
    {
        private static List<SQLOutputRowTypeContext> typeContexts;
        private static List<SQLOutputRowValueContext> valueContexts;
    
        static SupportSQLOutputRowConversion()
        {
            Reset();
        }

        public static void Reset()
        {
            typeContexts = new List<SQLOutputRowTypeContext>();
            valueContexts = new List<SQLOutputRowValueContext>();
        }

        public static IList<SQLOutputRowTypeContext> TypeContexts => typeContexts;

        public static IList<SQLOutputRowValueContext> ValueContexts => valueContexts;

        public Type GetOutputRowType(SQLOutputRowTypeContext sqlOutputRowTypeContext)
        {
            typeContexts.Add(sqlOutputRowTypeContext);
            return typeof(SupportBean);
        }

        public Object GetOutputRow(SQLOutputRowValueContext rowContext)
        {
            int myint = rowContext.Values.Get("myint").AsInt();
            if (myint == 90)
            {
                return null;
            }

            valueContexts.Add(rowContext);
            return new SupportBean(">" + myint + "<", 99000 + myint);
        }
    }
} // end of namespace
