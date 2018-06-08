///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.supportregression.bean;

namespace com.espertech.esper.supportregression.subscriber
{
	public class SupportSubscriberRowByRowSpecificWStmt : SupportSubscriberRowByRowSpecificBase
    {
	    public SupportSubscriberRowByRowSpecificWStmt() : base(true)
        {
	    }

	    public void Update(EPStatement statement, string theString, int intPrimitive)
	    {
	        AddIndication(statement, new object[] {theString, intPrimitive});
	    }

	    public void Update(EPStatement statement, int wideByte, long wideInt, double wideLong, double wideFloat)
	    {
	        AddIndication(statement, new object[] {wideByte, wideInt, wideLong, wideFloat});
	    }

	    public void Update(EPStatement statement, SupportBean supportBean)
	    {
	        AddIndication(statement, new object[] {supportBean});
	    }

	    public void Update(EPStatement statement, SupportBean supportBean, int value1, string value2)
	    {
	        AddIndication(statement, new object[] {supportBean, value1, value2});
	    }

	    public void Update(EPStatement statement, SupportBeanComplexProps.SupportBeanSpecialGetterNested n, SupportBeanComplexProps.SupportBeanSpecialGetterNestedNested nn)
	    {
	        AddIndication(statement, new object[] {n, nn});
	    }

	    public void Update(EPStatement statement, string theString, SupportEnum supportEnum)
	    {
	        AddIndication(statement, new object[] {theString, supportEnum});
	    }

	    public void Update(EPStatement statement, string nullableValue, long? longBoxed)
	    {
	        AddIndication(statement, new object[] {nullableValue, longBoxed});
	    }

	    public void Update(EPStatement statement, string value, SupportMarketDataBean s1, SupportBean s0)
	    {
	        AddIndication(statement, new object[] {value, s1, s0});
	    }

	    public void Update(EPStatement statement, SupportBean s0, SupportMarketDataBean s1)
	    {
	        AddIndication(statement, new object[] {s0, s1});
	    }
	}
} // end of namespace
