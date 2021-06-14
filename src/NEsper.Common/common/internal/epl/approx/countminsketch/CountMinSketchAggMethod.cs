///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.approx.countminsketch
{
	public enum CountMinSketchAggMethod
	{
		FREQ,
		TOPK
	}

	public static class CountMinSketchAggMethodExtensions
	{
		public static string GetMethodName(this CountMinSketchAggMethod value)
		{
			return value switch {
				CountMinSketchAggMethod.FREQ => "countMinSketchFrequency",
				CountMinSketchAggMethod.TOPK => "countMinSketchTopk",
				_ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
			};
		}

		public static CountMinSketchAggMethod? FromNameMayMatch(string name)
		{
			var nameLower = name.ToLowerInvariant();
			
			foreach (CountMinSketchAggMethod value in EnumHelper.GetValues<CountMinSketchAggMethod>()) {
				var funcName = GetMethodName(value).ToLowerInvariant();
				if (funcName == nameLower) {
					return value;
				}
			}
			
			return null;
		}
	}
} // end of namespace
