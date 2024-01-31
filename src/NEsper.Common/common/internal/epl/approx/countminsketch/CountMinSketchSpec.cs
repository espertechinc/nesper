///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.@internal.epl.approx.countminsketch
{
    public class CountMinSketchSpec
    {
        public CountMinSketchSpecHashes HashesSpec { get; set; }

        public int? TopkSpec { get; set; }

        public CountMinSketchAgent Agent { get; set; }

        public CountMinSketchAggState MakeAggState()
        {
            return new CountMinSketchAggState(CountMinSketchState.MakeState(this), Agent);
        }
    }
} // end of namespace