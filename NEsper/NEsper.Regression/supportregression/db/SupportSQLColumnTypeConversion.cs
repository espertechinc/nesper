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

using NUnit.Framework;

namespace com.espertech.esper.supportregression.db
{
    public class SupportSQLColumnTypeConversion : SQLColumnTypeConversion
    {
        private static List<SQLColumnTypeContext> typeContexts;
        private static List<SQLColumnValueContext> valueContexts;
        private static List<SQLInputParameterContext> paramContexts;

        static SupportSQLColumnTypeConversion()
        {
            Reset();
        }

        public static void Reset()
        {
            typeContexts = new List<SQLColumnTypeContext>();
            valueContexts = new List<SQLColumnValueContext>();
            paramContexts = new List<SQLInputParameterContext>();
        }

        public static List<SQLColumnTypeContext> TypeContexts => typeContexts;

        public static List<SQLColumnValueContext> ValueContexts => valueContexts;

        public static List<SQLInputParameterContext> ParamContexts => paramContexts;

        public Type GetColumnType(SQLColumnTypeContext sqlColumnTypeContext)
        {
            typeContexts.Add(sqlColumnTypeContext);
            return typeof(bool?);
        }

        public Object GetColumnValue(SQLColumnValueContext valueContext)
        {
            valueContexts.Add(valueContext);
            return ((int?) valueContext.ColumnValue) >= 50;
        }

        public Object GetParameterValue(SQLInputParameterContext inputParameterContext)
        {
            paramContexts.Add(inputParameterContext);
            if (inputParameterContext.ParameterValue is string) {
                return int.Parse(inputParameterContext.ParameterValue.ToString().Substring(1));
            }

            return inputParameterContext.ParameterValue;
        }
    }
} // end of namespace
