///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace NEsper.Examples.VirtualDW
{
    public class SampleMergeEvent
    {
        public SampleMergeEvent(String propOne, String propTwo)
        {
            PropOne = propOne;
            PropTwo = propTwo;
        }

        public string PropOne { get; private set; }

        public string PropTwo { get; private set; }
    }
}