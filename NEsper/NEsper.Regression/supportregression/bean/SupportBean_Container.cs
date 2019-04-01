///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.supportregression.bean
{
    public class SupportBean_Container
    {
        public SupportBean_Container(IList<SupportBean> beans)
        {
            Beans = beans;
        }

        public IList<SupportBean> Beans { get; set; }
    }
}