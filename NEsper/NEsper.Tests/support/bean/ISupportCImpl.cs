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
	public class ISupportCImpl : ISupportC
	{
		virtual public String C
		{
            get { return valueC; }
		}
		private String valueC;
		
		public ISupportCImpl(String valueC)
		{
			this.valueC = valueC;
		}
	}
}
