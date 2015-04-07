///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.support.bean
{
    [Serializable]
	public class ISupportBImpl : ISupportB
	{
		virtual public String B
		{
            get { return valueB; }
		}
		virtual public String BaseAB
		{
            get { return valueBaseAB; }
		}
		private String valueB;
		private String valueBaseAB;
		
		public ISupportBImpl(String valueB, String valueBaseAB)
		{
			this.valueB = valueB;
			this.valueBaseAB = valueBaseAB;
		}
	}
}
