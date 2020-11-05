///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.util
{
    public class Randomizer
    {
        private static readonly Random _random = new Random(); 
        
        public static double Random(double minValue, double maxValue)
        {
            var range = maxValue - minValue;
            return minValue + _random.NextDouble() * range;
        }

        public static double Random(double maxValue)
        {
            return _random.NextDouble() * maxValue;
        }
        
        public static double Random()
        {
            return _random.NextDouble();
        }
    }
}