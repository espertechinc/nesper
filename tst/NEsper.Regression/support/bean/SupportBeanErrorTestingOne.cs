///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportBeanErrorTestingOne
    {
        public SupportBeanErrorTestingOne()
        {
            throw new EPException("Default ctor manufactured test exception");
        }

        public string Value {
            set => throw new EPException("Setter manufactured test exception");
            get => throw new EPException("Getter manufactured test exception");
        }
    }
} // end of namespace