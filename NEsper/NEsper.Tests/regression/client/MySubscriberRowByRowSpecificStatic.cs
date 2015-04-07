///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;


namespace com.espertech.esper.regression.client
{
    public class MySubscriberRowByRowSpecificStatic
    {
        private static IList<Object[]> indicate = new List<Object[]>();
    
        public void Update(String stringValue, int IntPrimitive)
        {
            indicate.Add(new Object[] {stringValue, IntPrimitive});
        }
    
        public IList<Object[]> GetAndResetIndicate()
        {
            IList<Object[]> result = indicate;
            indicate = new List<Object[]>();
            return result;
        }
    }
}
