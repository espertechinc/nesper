///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.hook;
using com.espertech.esper.compat.collections;
using com.espertech.esper.support.bean;

namespace com.espertech.esper.regression.db
{
    public class SupportSQLOutputRowConversion : SQLOutputRowConversion
    {
        private static List<SQLOutputRowTypeContext> _typeContexts;
        private static List<SQLOutputRowValueContext> _valueContexts;
    
        static SupportSQLOutputRowConversion() 
        {
            Reset();
        }
    
        public static void Reset()
        {
            _typeContexts = new List<SQLOutputRowTypeContext>();
            _valueContexts = new List<SQLOutputRowValueContext>();
        }

        public static IList<SQLOutputRowTypeContext> TypeContexts
        {
            get { return _typeContexts; }
        }

        public static IList<SQLOutputRowValueContext> ValueContexts
        {
            get { return _valueContexts; }
        }

        public Type GetOutputRowType(SQLOutputRowTypeContext sqlOutputRowTypeContext)
        {
            _typeContexts.Add(sqlOutputRowTypeContext);
            return typeof(SupportBean);
        }
    
        public Object GetOutputRow(SQLOutputRowValueContext rowContext)
        {
            var myint = (int?) rowContext.Values.Get("myint");
            if (myint == 90) {
                return null;
            }
            _valueContexts.Add(rowContext);
            return new SupportBean(">" + myint + "<", 99000 + myint.Value);
        }
    }
}
