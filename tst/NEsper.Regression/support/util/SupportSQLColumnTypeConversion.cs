///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.hook.type;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportSQLColumnTypeConversion : SQLColumnTypeConversion
    {
        static SupportSQLColumnTypeConversion()
        {
            Reset();
        }

        public static IList<SQLColumnTypeContext> TypeContexts { get; private set; }

        public static IList<SQLColumnValueContext> ValueContexts { get; private set; }

        public static IList<SQLInputParameterContext> ParamContexts { get; private set; }

        public Type GetColumnType(SQLColumnTypeContext sqlColumnTypeContext)
        {
            TypeContexts.Add(sqlColumnTypeContext);
            return typeof(bool?);
        }

        public object GetColumnValue(SQLColumnValueContext valueContext)
        {
            ValueContexts.Add(valueContext);
            return (int?) valueContext.ColumnValue >= 50;
        }

        public object GetParameterValue(SQLInputParameterContext inputParameterContext)
        {
            ParamContexts.Add(inputParameterContext);
            if (inputParameterContext.ParameterValue is string) {
                return int.Parse(inputParameterContext.ParameterValue.ToString().Substring(1));
            }

            return inputParameterContext.ParameterValue;
        }

        public static void Reset()
        {
            TypeContexts = new List<SQLColumnTypeContext>();
            ValueContexts = new List<SQLColumnValueContext>();
            ParamContexts = new List<SQLInputParameterContext>();
        }
    }
} // end of namespace