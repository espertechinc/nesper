///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.collection;


namespace com.espertech.esper.regression.client
{
    public class MySubscriberMultirowObjectArr
    {
        private IList<UniformPair<Object[][]>> indicateArr = new List<UniformPair<Object[][]>>();
    
        public void Update(Object[][] newEvents, Object[][] oldEvents)
        {
            indicateArr.Add(new UniformPair<Object[][]>(newEvents, oldEvents));
        }

        public IList<UniformPair<object[][]>> IndicateArr
        {
            get { return indicateArr; }
        }

        public IList<UniformPair<Object[][]>> GetAndResetIndicateArr()
        {
            IList<UniformPair<Object[][]>> result = indicateArr;
            indicateArr = new List<UniformPair<Object[][]>>();
            return result;
        }
    }
}
