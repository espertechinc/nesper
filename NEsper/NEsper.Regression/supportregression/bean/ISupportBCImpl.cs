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
	public class ISupportBCImpl : ISupportB, ISupportC
	{
		public virtual String B
		{
            get { return valueB; }
		}
		public virtual String BaseAB
		{
            get { return valueBaseAB; }
		}
		public virtual String C
		{
            get { return valueC; }
		}
		private String valueB;
		private String valueBaseAB;
		private String valueC;
		
		public ISupportBCImpl(String valueB, String valueBaseAB, String valueC)
		{
			this.valueB = valueB;
			this.valueBaseAB = valueBaseAB;
			this.valueC = valueC;
		}
	}
}
