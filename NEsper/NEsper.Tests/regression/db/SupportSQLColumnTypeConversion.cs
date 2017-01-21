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

namespace com.espertech.esper.regression.db
{
    public class SupportSQLColumnTypeConversion : SQLColumnTypeConversion
    {
        private static List<SQLColumnTypeContext> _typeContexts;
        private static List<SQLColumnValueContext> _valueContexts;
        private static List<SQLInputParameterContext> _paramContexts;
    
        static SupportSQLColumnTypeConversion()
        {
            Reset();
        }
    
        public static void Reset() {
            _typeContexts = new List<SQLColumnTypeContext>();
            _valueContexts = new List<SQLColumnValueContext>();
            _paramContexts = new List<SQLInputParameterContext>();
        }

        public static IList<SQLColumnTypeContext> TypeContexts
        {
            get { return _typeContexts; }
        }

        public static IList<SQLColumnValueContext> ValueContexts
        {
            get { return _valueContexts; }
        }

        public static IList<SQLInputParameterContext> ParamContexts
        {
            get { return _paramContexts; }
        }

        public Type GetColumnType(SQLColumnTypeContext sqlColumnTypeContext)
        {
            _typeContexts.Add(sqlColumnTypeContext);
            return typeof(bool?);
        }
    
        public Object GetColumnValue(SQLColumnValueContext valueContext)
        {
            _valueContexts.Add(valueContext);
            return ((int?) valueContext.ColumnValue) >= 50;
        }
    
        public Object GetParameterValue(SQLInputParameterContext inputParameterContext)
        {
            _paramContexts.Add(inputParameterContext);
            if (inputParameterContext.ParameterValue is String) {
                return int.Parse(inputParameterContext.ParameterValue.ToString().Substring(1));
            }
            return inputParameterContext.ParameterValue;
        }
    }
}
