///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.util;

namespace com.espertech.esper.compat
{
    public class Name
    {
        public static string Clean<T>(bool useBoxed = true)
        {
            return (useBoxed ? typeof (T).GetBoxedType() : typeof (T)).GetCleanName();
        }

        public static string Of(Type type, bool useBoxed = true)
        {
            if (useBoxed)
            {
                type = type.GetBoxedType();
            }

            return type.FullName;
            
        }

        public static string Of<T>(bool useBoxed = true)
        {
            if (useBoxed)
            {
                return typeof (T).GetBoxedType().FullName;
            }

            return typeof (T).FullName;
        }
    }
}