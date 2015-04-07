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


namespace com.espertech.esper.regression.client
{
    public class MySubscriberRowByRowSpecific
    {
        private IList<Object[]> indicate = new List<Object[]>();
    
        public void Update(String stringValue, int IntPrimitive)
        {
            indicate.Add(new Object[] {stringValue, IntPrimitive});
        }
    
        public void Update(int wideByte, long wideInt, double wideLong, double wideFloat)
        {
            indicate.Add(new Object[] {wideByte, wideInt, wideLong, wideFloat});
        }
    
        public void Update(SupportBean supportBean)
        {
            indicate.Add(new Object[] {supportBean});
        }
    
        public void Update(SupportBean supportBean, int value1, String value2)
        {
            indicate.Add(new Object[] {supportBean, value1, value2});
        }
    
        public void Update(SupportBeanComplexProps.SupportBeanSpecialGetterNested n,
                                   SupportBeanComplexProps.SupportBeanSpecialGetterNestedNested nn)
        {
            indicate.Add(new Object[] {n, nn});
        }
    
        public void Update(String stringValue, SupportEnum supportEnum)
        {
            indicate.Add(new Object[] {stringValue, supportEnum});
        }
    
        public void Update(String nullableValue, long? longBoxed)
        {
            indicate.Add(new Object[] {nullableValue, longBoxed});
        }
    
        public void Update(String value, SupportMarketDataBean s1, SupportBean s0)
        {
            indicate.Add(new Object[] {value, s1, s0});
        }
    
        public void Update(SupportBean s0, SupportMarketDataBean s1)
        {
            indicate.Add(new Object[] {s0, s1});
        }
    
        public IList<Object[]> GetAndResetIndicate()
        {
            IList<Object[]> result = indicate;
            indicate = new List<Object[]>();
            return result;
        }
    }
}
