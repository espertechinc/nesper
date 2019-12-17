///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public enum HashFunctionEnum
    {
        CONSISTENT_HASH_CRC32,
        HASH_CODE
    }

    public static class HashFunctionEnumExtensions
    {
        public static HashFunctionEnum? Determine(
            string contextName,
            string name)
        {
            string nameTrim = name.ToLowerInvariant().Trim();
            foreach (HashFunctionEnum val in EnumHelper.GetValues<HashFunctionEnum>())
            {
                if (val.GetName().ToLowerInvariant().Trim() == nameTrim)
                {
                    return val;
                }
            }

            return null;
        }

        public static string GetStringList()
        {
            StringWriter message = new StringWriter();
            string delimiter = "";
            foreach (HashFunctionEnum val in EnumHelper.GetValues<HashFunctionEnum>())
            {
                message.Write(delimiter);
                message.Write(val.GetName().ToLowerInvariant().Trim());
                delimiter = ", ";
            }

            return message.ToString();
        }
    }
} // end of namespace