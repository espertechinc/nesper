///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.support.bean;

namespace com.espertech.esper.multithread
{
    public class EnumerationGenerator
    {
        public static IEnumerator<Object> Create(int maxNumEvents)
        {
            for( int ii = 0 ; ii < maxNumEvents ; ii++ ) {
                yield return new SupportBean(Convert.ToString(ii), ii);
            }
        }
    }
}
