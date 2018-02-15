///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
    public class SupportBean_S4
	{
        public int Id { get; set; }

        public String P40 { get; set; }

        public String P41 { get; set; }

        public String P42 { get; set; }

        public String P43 { get; set; }

        private static int idCounter;

        public static object[] MakeS4(String propOne, String[] propTwo)
		{
			idCounter++;
			
			object[] events = new Object[propTwo.Length];
			for (int i = 0; i < propTwo.Length; i++)
			{
				events[i] = new SupportBean_S4(idCounter, propOne, propTwo[i]);
			}
			return events;
		}
		
		public SupportBean_S4(int id)
		{
            this.Id = id;
		}
		
		public SupportBean_S4(int id, String p40)
		{
            this.Id = id;
            this.P40 = p40;
		}
		
		public SupportBean_S4(int id, String p40, String p41)
		{
            this.Id = id;
            this.P40 = p40;
            this.P41 = p41;
		}
	}
}
