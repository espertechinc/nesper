///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.approx
{
    public class CountMinSketchSpecHashes
    {
        public CountMinSketchSpecHashes(double epsOfTotalCount, double confidence, int seed)
        {
            EpsOfTotalCount = epsOfTotalCount;
            Confidence = confidence;
            Seed = seed;
        }

        public double EpsOfTotalCount { get; set; }

        public double Confidence { get; set; }

        public int Seed { get; set; }
    }
}