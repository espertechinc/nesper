///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    [Serializable]
    public class SupportBeanIterablePropsContainer
    {
        public SupportBeanIterablePropsContainer(SupportBeanIterableProps inner)
        {
            Contained = inner;
        }

        public SupportBeanIterableProps Contained { get; }

        public static SupportBeanIterablePropsContainer MakeDefaultBean()
        {
            return new SupportBeanIterablePropsContainer(SupportBeanIterableProps.MakeDefaultBean());
        }
    }
} // end of namespace
