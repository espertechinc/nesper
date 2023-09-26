///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.@event.core
{
    public class EventTypeNameUtil
    {
        private const string TABLE_INTERNAL_PREFIX = "table_internal_";

        public static string GetWrapperInnerTypeName(string name)
        {
            return name + "_in";
        }

        public static string GetTableInternalTypeName(string tableName)
        {
            return TABLE_INTERNAL_PREFIX + tableName;
        }

        public static string GetTableNameFromInternalTypeName(string typeName)
        {
            return typeName.Substring(TABLE_INTERNAL_PREFIX.Length);
        }

        public static bool IsTableNamePrefix(string typeName)
        {
            return typeName.StartsWith(TABLE_INTERNAL_PREFIX);
        }

        public static string GetTablePublicTypeName(string tableName)
        {
            return "table_" + tableName;
        }

        public static string GetAnonymousTypeNameExcludePlanHint()
        {
            return "exclude_plan_hint";
        }
    }
} // end of namespace