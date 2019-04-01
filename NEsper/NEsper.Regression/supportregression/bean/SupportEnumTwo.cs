///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.supportregression.bean
{
    public enum SupportEnumTwo
    {
        ENUM_VALUE_1,
        ENUM_VALUE_2,
        ENUM_VALUE_3
    }

    public static class SupportEnumTwoExtensions
    {
        public static string[] GetMystrings(this SupportEnumTwo @enum) {
            switch (@enum)
            {
                case SupportEnumTwo.ENUM_VALUE_1:
                    return new string[] {"1", "0", "0"};
                case SupportEnumTwo.ENUM_VALUE_2:
                    return new string[] {"2", "0", "0"};
                case SupportEnumTwo.ENUM_VALUE_3:
                    return new string[] {"3", "0", "0"};
            }

            throw new ArgumentException();
        }
    
        public static int GetAssociatedValue(this SupportEnumTwo @enum)
        {
            switch (@enum)
            {
                case SupportEnumTwo.ENUM_VALUE_1:
                    return 100;
                case SupportEnumTwo.ENUM_VALUE_2:
                    return 200;
                case SupportEnumTwo.ENUM_VALUE_3:
                    return 300;
            }

            throw new ArgumentException();
        }

        public static bool CheckAssociatedValue(this SupportEnumTwo @enum, int value)
        {
            return GetAssociatedValue(@enum) == value;
        }

        public static bool CheckEventBeanPropInt(this SupportEnumTwo @enum, EventBean @event, String propertyName)
        {
            var value = @event.Get(propertyName);
            if (value == null && (!value.IsInt())) {
                return false;
            }
            return GetAssociatedValue(@enum) == (int?)value;
        }

        public static Nested GetNested(this SupportEnumTwo @enum)
        {
            return new Nested(GetAssociatedValue(@enum));
        }
    
        public class Nested
        {
            public Nested(int value)
            {
                Value = value;
            }

            public int Value { get; private set; }
        }
    }
}
