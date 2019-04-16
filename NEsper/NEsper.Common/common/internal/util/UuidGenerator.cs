///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.util
{
    public class UuidGenerator
    {
        public static String Generate()
        {
            return Guid.NewGuid().ToString();
        }

        public static String GenerateNoDash()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
} // End of namespace