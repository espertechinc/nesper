///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportBean_Container
    {
        public SupportBean_Container(IList<SupportBean> beans)
        {
            Beans = beans;
        }

        public IList<SupportBean> Beans { get; private set; }

        public void SetBeans(IList<SupportBean> beans)
        {
            Beans = beans;
        }

        public override string ToString()
        {
            return nameof(Beans) + " : " + Beans.RenderAny();
        }
    }
} // end of namespace