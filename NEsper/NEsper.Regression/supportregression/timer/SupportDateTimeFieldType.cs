///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;

namespace com.espertech.esper.supportregression.timer
{
    public enum SupportDateTimeFieldType
    {
        MSEC,
        DATE,
        CAL
    }

    public static class SupportDateTimeFieldTypeExtensions
    {
	    public static string GetDateTimeFieldType(this SupportDateTimeFieldType enumValue)
        {
	        switch (enumValue)
	        {
	            case SupportDateTimeFieldType.CAL:
	                return typeof (DateTimeEx).FullName;
                case SupportDateTimeFieldType.DATE:
	                return typeof (DateTime).FullName;
                case SupportDateTimeFieldType.MSEC:
	                return "long";
                default:
                    throw new ArgumentException();
	        }
	    }

        public static Func<SupportTimeStartEndA, object> GetEndDateTimeProvider(this SupportDateTimeFieldType enumValue)
        {
	        switch (enumValue)
	        {
	            case SupportDateTimeFieldType.CAL:
                    return input => input.caldateEnd;
                case SupportDateTimeFieldType.DATE:
	                return input => input.utildateEnd;
                case SupportDateTimeFieldType.MSEC:
	                return input => input.longdateEnd;
                default:
                    throw new ArgumentException();
	        }
        } 

	    public static object MakeStart(this SupportDateTimeFieldType enumValue, string time) {
	        return FromEndDate(enumValue, SupportTimeStartEndA.Make(null, time, 0));
	    }

	    public static object MakeEnd(this SupportDateTimeFieldType enumValue, string time, long duration) {
	        return FromEndDate(enumValue, SupportTimeStartEndA.Make(null, time, duration));
	    }

	    private static object FromEndDate(this SupportDateTimeFieldType enumValue, SupportTimeStartEndA holder) {
	        return GetEndDateTimeProvider(enumValue).Invoke(holder);
	    }
	}
} // end of namespace
