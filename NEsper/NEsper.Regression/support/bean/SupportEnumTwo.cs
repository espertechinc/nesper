///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;

namespace com.espertech.esper.regressionlib.support.bean
{
    public enum SupportEnumTwo
    {
        ENUM_VALUE_1,
        ENUM_VALUE_2,
        ENUM_VALUE_3
    }

    public static class SupportEnumTwoExtensions
    {
        public static int GetAssociatedValue(this SupportEnumTwo value)
        {
            switch (value) {
                case SupportEnumTwo.ENUM_VALUE_1:
                    return 100;

                case SupportEnumTwo.ENUM_VALUE_2:
                    return 200;

                case SupportEnumTwo.ENUM_VALUE_3:
                    return 300;

                default:
                    throw new ArgumentException(nameof(value));
            }
        }

        public static string[] GetMystrings(this SupportEnumTwo value)
        {
            switch (value) {
                case SupportEnumTwo.ENUM_VALUE_1:
                    return new[] {"1", "0", "0"};

                case SupportEnumTwo.ENUM_VALUE_2:
                    return new[] {"2", "0", "0"};

                case SupportEnumTwo.ENUM_VALUE_3:
                    return new[] {"3", "0", "0"};

                default:
                    throw new ArgumentException(nameof(value));
            }
        }

        public static bool CheckAssociatedValue(
            this SupportEnumTwo enumValue,
            int value)
        {
            return GetAssociatedValue(enumValue) == value;
        }

        public static bool CheckEventBeanPropInt(
            this SupportEnumTwo enumValue,
            EventBean @event,
            string propertyName)
        {
            var value = @event.Get(propertyName);
            if (value == null && !(value is int?)) {
                return false;
            }

            return GetAssociatedValue(enumValue) == (int?) value;
        }

        public static Nested GetNested(this SupportEnumTwo enumValue)
        {
            return new Nested(GetAssociatedValue(enumValue));
        }

        public class Nested
        {
            public Nested(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }
    }
} // end of namespace