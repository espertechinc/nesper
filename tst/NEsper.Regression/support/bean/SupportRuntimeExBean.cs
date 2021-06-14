///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportRuntimeExBean
    {
        public string Property2 => "2";

        public string GetProperty1()
        {
            throw new EPException("I should not have been called!");
        }
    }
} // end of namespace