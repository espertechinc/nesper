///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.bean
{
    public enum SupportEnum
    {
        ENUM_VALUE_1,
        ENUM_VALUE_2,
        ENUM_VALUE_3
    }

    public class SupportEnumHelper
    {
        public static SupportEnum GetEnumFor(String value)
        {
            return (SupportEnum)Enum.Parse(typeof(SupportEnum), value, false);
        }

        public static SupportEnum GetValueForEnum(int value)
        {
            return (SupportEnum)Enum.ToObject(typeof(SupportEnum), value);
        }
    }
}
