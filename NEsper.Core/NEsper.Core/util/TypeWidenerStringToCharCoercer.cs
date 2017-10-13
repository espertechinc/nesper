///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Type widener that coerces from String to char if required.
    /// </summary>
    public class TypeWidenerStringToCharCoercer
    {
        public static Object Widen(Object input)
        {
            string result = input.ToString();
            if ((result != null) && (result.Length > 0))
            {
                return result[0];
            }
            return null;
        }
    }
}
