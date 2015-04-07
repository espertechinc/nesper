///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.util
{
    public class AttributeUtil
    {
        public static bool IsListed(String list, String lookedForValue)
        {
            if (list == null)
            {
                return false;
            }

            lookedForValue = lookedForValue.Trim().ToUpper();
            list = list.Trim().ToUpper();

            if (list == lookedForValue)
            {
                return true;
            }

            if (list.IndexOf('=') != -1)
            {
                var hintName = list.Substring(0, list.IndexOf('='));
                if (hintName.Trim().ToUpper() == lookedForValue)
                {
                    return true;
                }
            }

            var items = list.Split(',');
            foreach (var item in items)
            {
                var listItem = item.Trim().ToUpper();
                if (listItem == lookedForValue)
                {
                    return true;
                }

                if (listItem.IndexOf('=') != -1)
                {
                    var listItemName = listItem.Substring(0, listItem.IndexOf('='));
                    if (listItemName.Trim().ToUpper() == lookedForValue)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static String GetAssignedValue(String value, String enumValue)
        {

            var valMixed = value.Trim();
            var val = valMixed.ToUpper();

            if (val.IndexOf(",") == -1)
            {
                if (val.IndexOf('=') == -1)
                {
                    return null;
                }

                var hintName = val.Substring(0, val.IndexOf('='));
                if (!hintName.Equals(enumValue))
                {
                    return null;
                }
                return valMixed.Substring(val.IndexOf('=') + 1, val.Length);
            }

            var hints = valMixed.Split(',');
            foreach (var hint in hints)
            {
                var indexOfEquals = hint.IndexOf('=');
                if (indexOfEquals == -1)
                {
                    continue;
                }

                val = hint.Substring(0, indexOfEquals).Trim().ToUpper();
                if (!val.Equals(enumValue))
                {
                    continue;
                }

                var strValue = hint.Substring(indexOfEquals + 1).Trim();
                if (strValue.Length == 0)
                {
                    return null;
                }
                return strValue;
            }

            return null;
        }
    }
}
