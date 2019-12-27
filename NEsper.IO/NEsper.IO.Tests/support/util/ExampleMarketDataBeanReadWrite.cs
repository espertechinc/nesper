///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.magic;
using com.espertech.esperio.regression.adapter;

namespace com.espertech.esperio.support.util
{
    public class ExampleMarketDataBeanReadWrite : TestCSVAdapterUseCases.ExampleMarketDataBean
    {
        [PropertyName("value")]
        public double Value => Price*Volume.Value;
    }
}
