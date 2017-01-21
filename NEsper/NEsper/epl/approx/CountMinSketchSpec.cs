///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client.util;

namespace com.espertech.esper.epl.approx
{
    public class CountMinSketchSpec
    {
        public CountMinSketchSpec(CountMinSketchSpecHashes hashesSpec, int? topkSpec, CountMinSketchAgent agent)
        {
            HashesSpec = hashesSpec;
            TopkSpec = topkSpec;
            Agent = agent;
        }

        public CountMinSketchSpecHashes HashesSpec { get; private set; }

        public int? TopkSpec { get; set; }

        public CountMinSketchAgent Agent { get; set; }
    }
}