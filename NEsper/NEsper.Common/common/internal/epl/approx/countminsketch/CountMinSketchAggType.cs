///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.approx.countminsketch
{
    public enum CountMinSketchAggType
    {
        STATE,
        ADD
    }

    public static class CountMinSketchAggTypeExtensions
    {
        public static string GetFuncName(this CountMinSketchAggType enumValue)
        {
            return enumValue switch {
                CountMinSketchAggType.ADD => "countMinSketchAdd",
                CountMinSketchAggType.STATE => "countMinSketch",
                _ => throw new ArgumentException("invalid value for enum value", nameof(enumValue))
            };
        }

        public static CountMinSketchAggType? FromNameMayMatch(string name)
        {
            return name.ToLowerInvariant() switch {
                "countminsketchadd" => CountMinSketchAggType.ADD,
                "countminsketch" => CountMinSketchAggType.STATE,
                _ => null
            };
        }
    }
}