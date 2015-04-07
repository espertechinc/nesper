///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.schedule
{
	/// <summary>
    /// Calendar class for use in scheduling, specifically for use in computing the next invocation time.
    /// </summary>
	
    public class ScheduleCalendar
	{
	    public int Milliseconds { get; set; }

	    public int Second { get; set; }

	    public int Minute { get; set; }

	    public int Hour { get; set; }

	    public int DayOfMonth { get; set; }

	    public int Month { get; set; }

	    internal ScheduleCalendar(int milliseconds, int second, int minute, int hour, int dayOfMonth, int month)
		{
			Milliseconds = milliseconds;
			Second = second;
			Minute = minute;
			Hour = hour;
			DayOfMonth = dayOfMonth;
			Month = month;
		}
		
		internal ScheduleCalendar()
		{
		}
	}
}
