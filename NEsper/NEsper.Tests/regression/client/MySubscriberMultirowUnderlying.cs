///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.support.bean;


namespace com.espertech.esper.regression.client
{
    public class MySubscriberMultirowUnderlying
    {
        private IList<UniformPair<SupportBean[]>> indicateArr = new List<UniformPair<SupportBean[]>>();
    
        public void Update(SupportBean[] newEvents, SupportBean[] oldEvents)
        {
            indicateArr.Add(new UniformPair<SupportBean[]>(newEvents, oldEvents));
        }

        public IList<UniformPair<SupportBean[]>> IndicateArr
        {
            get { return indicateArr; }
        }

        public IList<UniformPair<SupportBean[]>> GetAndResetIndicateArr()
        {
            IList<UniformPair<SupportBean[]>> result = indicateArr;
            indicateArr = new List<UniformPair<SupportBean[]>>();
            return result;
        }
    }
}
