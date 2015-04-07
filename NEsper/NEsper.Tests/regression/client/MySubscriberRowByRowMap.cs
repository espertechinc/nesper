///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using DataMap = System.Collections.Generic.IDictionary<string, object>;

namespace com.espertech.esper.regression.client
{
    public class MySubscriberRowByRowMap
    {
        private IList<DataMap> indicateIStream = new List<DataMap>();
        private IList<DataMap> indicateRStream = new List<DataMap>();

        public void Update(DataMap row)
        {
            indicateIStream.Add(row);
        }

        public void UpdateRStream(DataMap row)
        {
            indicateRStream.Add(row);
        }
    
        public IList<DataMap> GetAndResetIndicateIStream()
        {
            IList<DataMap> result = indicateIStream;
            indicateIStream = new List<DataMap>();
            return result;
        }
    
        public IList<DataMap> GetAndResetIndicateRStream()
        {
            IList<DataMap> result = indicateRStream;
            indicateRStream = new List<DataMap>();
            return result;
        }
    }
}
