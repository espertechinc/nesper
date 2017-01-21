///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.agg.service
{
    public class AggregatorUtil
    {
        public static bool CheckFilter(Object[] @object)
        {
            if (@object[1] == null)
                return false;
            return true.Equals(@object[1]);
        }
    }
}
