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
    public class SupportOverrideOne : SupportOverrideBase
    {
        public SupportOverrideOne(
            string valOne,
            string valBase)
            : base(valBase)
        {
            Val = valOne;
        }

        public override string Val { get; }
    }
} // end of namespace
