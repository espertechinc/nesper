///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.approx.countminsketch
{
    public class CountMinSketchSpec
    {
        private CountMinSketchSpecHashes hashesSpec;
        private int? topkSpec;
        private CountMinSketchAgent agent;

        public void SetHashesSpec(CountMinSketchSpecHashes hashesSpec)
        {
            this.hashesSpec = hashesSpec;
        }

        public CountMinSketchSpecHashes HashesSpec {
            get => hashesSpec;
        }

        public int? TopkSpec {
            get => topkSpec;
        }

        public void SetTopkSpec(int? topkSpec)
        {
            this.topkSpec = topkSpec;
        }

        public CountMinSketchAgent Agent {
            get => agent;
        }

        public void SetAgent(CountMinSketchAgent agent)
        {
            this.agent = agent;
        }

        public CountMinSketchAggState MakeAggState()
        {
            return new CountMinSketchAggState(CountMinSketchState.MakeState(this), agent);
        }
    }
} // end of namespace