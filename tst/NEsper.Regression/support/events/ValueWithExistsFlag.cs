///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.@event
{
    public class ValueWithExistsFlag
    {
        private ValueWithExistsFlag(
            bool exists,
            object value)
        {
            IsExists = exists;
            Value = value;
        }

        public bool IsExists { get; }

        public object Value { get; }

        public static ValueWithExistsFlag NotExists()
        {
            return new ValueWithExistsFlag(false, null);
        }

        public static ValueWithExistsFlag Exists(object value)
        {
            return new ValueWithExistsFlag(true, value);
        }

        public static ValueWithExistsFlag[] MultipleNotExists(int count)
        {
            var flagged = new ValueWithExistsFlag[count];
            for (var i = 0; i < flagged.Length; i++) {
                flagged[i] = NotExists();
            }

            return flagged;
        }

        public static ValueWithExistsFlag[] AllExist(params object[] values)
        {
            var flagged = new ValueWithExistsFlag[values.Length];
            for (var i = 0; i < values.Length; i++) {
                flagged[i] = Exists(values[i]);
            }

            return flagged;
        }
    }
} // end of namespace