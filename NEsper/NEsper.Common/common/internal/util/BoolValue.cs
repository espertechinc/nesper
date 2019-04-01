///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.util
{
    public class BoolValue
    {
        /// <summary>
        ///     Parse the boolean string.
        /// </summary>
        /// <param name="value">is a bool value</param>
        /// <returns>parsed boolean</returns>
        public static bool ParseString(string value)
        {
            bool rvalue;
            value = value.ToLower();
            if (!Boolean.TryParse(value, out rvalue))
            {
                throw new ArgumentException("Boolean value '" + value + "' cannot be converted to bool");
            }

            return rvalue;
        }
    }
} // end of namespace