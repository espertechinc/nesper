///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.approx.countminsketch
{
    public enum CountMinSketchAggType
    {
        STATE,
        ADD,
        FREQ,
        TOPK
    }

    public static class CountMinSketchAggTypeExtensions
    {
        public static string GetFuncName(this CountMinSketchAggType enumValue)
        {
            switch (enumValue)
            {
                case CountMinSketchAggType.ADD:
                    return "countMinSketchAdd";
                case CountMinSketchAggType.FREQ:
                    return "countMinSketchFrequency";
                case CountMinSketchAggType.STATE:
                    return "countMinSketch";
                case CountMinSketchAggType.TOPK:
                    return "countMinSketchTopk";
            }

            throw new ArgumentException("invalid value for enum value", "enumValue");
        }

        public static CountMinSketchAggType? FromNameMayMatch(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "countminsketchadd":
                    return CountMinSketchAggType.ADD;
                case "countminsketchfrequency":
                    return CountMinSketchAggType.FREQ;
                case "countminsketch":
                    return CountMinSketchAggType.STATE;
                case "countminsketchtopk":
                    return CountMinSketchAggType.TOPK;
            }

            return null;
        }
    }
}