///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;

using DataMap = System.Collections.Generic.IDictionary<string, object>;

namespace com.espertech.esper.regression.client
{
    public class MySubscriberMultirowMap
    {
        private IList<UniformPair<DataMap[]>> indicateMap = new List<UniformPair<DataMap[]>>();

        public void Update(DataMap[] newEvents, DataMap[] oldEvents)
        {
            indicateMap.Add(new UniformPair<DataMap[]>(newEvents, oldEvents));
        }

        public IList<UniformPair<DataMap[]>> IndicateMap
        {
            get { return indicateMap; }
        }

        public IList<UniformPair<DataMap[]>> GetAndResetIndicateMap()
        {
            IList<UniformPair<DataMap[]>> result = indicateMap;
            indicateMap = new List<UniformPair<DataMap[]>>();
            return result;
        }
    }
}
