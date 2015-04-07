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
    public class MySubscriberRowByRowFull
    {
        private IList<UniformPair<int?>> indicateStart = new List<UniformPair<int?>>();
        private IList<Object> indicateEnd = new List<Object>();
        private IList<Object[]> indicateIStream = new List<Object[]>();
        private IList<Object[]> indicateRStream = new List<Object[]>();
    
        public void UpdateStart(int lengthIStream, int lengthRStream)
        {
            indicateStart.Add(new UniformPair<int?>(lengthIStream, lengthRStream));
        }
    
        public void Update(String stringValue, int IntPrimitive)
        {
            indicateIStream.Add(new Object[] {stringValue, IntPrimitive});
        }
    
        public void UpdateRStream(String stringValue, int IntPrimitive)
        {
            indicateRStream.Add(new Object[] {stringValue, IntPrimitive});
        }
    
        public void UpdateEnd()
        {
            indicateEnd.Add(this);
        }
    
        public IList<UniformPair<int?>> GetAndResetIndicateStart()
        {
            IList<UniformPair<int?>> result = indicateStart;
            indicateStart = new List<UniformPair<int?>>();
            return result;
        }
    
        public IList<Object[]> GetAndResetIndicateIStream()
        {
            IList<Object[]> result = indicateIStream;
            indicateIStream = new List<Object[]>();
            return result;
        }
    
        public IList<Object[]> GetAndResetIndicateRStream()
        {
            IList<Object[]> result = indicateRStream;
            indicateRStream = new List<Object[]>();
            return result;
        }
    
        public IList<Object> GetAndResetIndicateEnd()
        {
            IList<Object> result = indicateEnd;
            indicateEnd = new List<Object>();
            return result;
        }

        public IList<UniformPair<int?>> IndicateStart
        {
            get { return indicateStart; }
        }

        public IList<Object> GetIndicateEnd() {
            return indicateEnd;
        }
    
        public IList<Object[]> GetIndicateIStream() {
            return indicateIStream;
        }
    
        public IList<Object[]> GetIndicateRStream() {
            return indicateRStream;
        }
    }
}
