///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.supportregression.bean;

namespace com.espertech.esper.supportregression.subscriber
{
	public class SupportSubscriberRowByRowSpecificNStmt : SupportSubscriberRowByRowSpecificBase
	{
	    public SupportSubscriberRowByRowSpecificNStmt() : base(false)
        {
	    }

	    public void Update(string theString, int intPrimitive)
	    {
	        AddIndication(new object[] {theString, intPrimitive});
	    }

	    public void Update(int wideByte, long wideInt, double wideLong, double wideFloat)
	    {
	        AddIndication(new object[] {wideByte, wideInt, wideLong, wideFloat});
	    }

	    public void Update(SupportBean supportBean)
	    {
	        AddIndication(new object[] {supportBean});
	    }

	    public void Update(SupportBean supportBean, int value1, string value2)
	    {
	        AddIndication(new object[] {supportBean, value1, value2});
	    }

	    public void Update(SupportBeanComplexProps.SupportBeanSpecialGetterNested n, SupportBeanComplexProps.SupportBeanSpecialGetterNestedNested nn)
	    {
	        AddIndication(new object[] {n, nn});
	    }

	    public void Update(string theString, SupportEnum supportEnum)
	    {
	        AddIndication(new object[] {theString, supportEnum});
	    }

	    public void Update(string nullableValue, long? longBoxed)
	    {
	        AddIndication(new object[] {nullableValue, longBoxed});
	    }

	    public void Update(string value, SupportMarketDataBean s1, SupportBean s0)
	    {
	        AddIndication(new object[] {value, s1, s0});
	    }

	    public void Update(SupportBean s0, SupportMarketDataBean s1)
	    {
	        AddIndication(new object[] {s0, s1});
	    }
	}
} // end of namespace
