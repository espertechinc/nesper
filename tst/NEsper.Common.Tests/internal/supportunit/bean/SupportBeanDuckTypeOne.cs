///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class SupportBeanDuckTypeOne : SupportBeanDuckType
    {
        private readonly string stringValue;

        public SupportBeanDuckTypeOne(string stringValue)
        {
            this.stringValue = stringValue;
        }

        public double ReturnDouble()
        {
            return 12.9876d;
        }

        public string MakeString()
        {
            return stringValue;
        }

        public object MakeCommon()
        {
            return new SupportBeanDuckTypeTwo(-1);
        }
    }
} // end of namespace
