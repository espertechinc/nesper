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
	public class ISupportAImplSuperGImplPlus : ISupportAImplSuperG, ISupportB, ISupportC
	{
		override public String G
		{
            get { return valueG; }
		}
		override public String A
		{
            get { return valueA; }
		}
		override public String BaseAB
		{
            get { return valueBaseAB; }
		}
		public virtual String B
		{
            get { return valueB; }
		}
		public virtual String C
		{
            get { return valueC; }
		}
		internal String valueG;
		internal String valueA;
		internal String valueBaseAB;
		internal String valueB;
		internal String valueC;
		
		public ISupportAImplSuperGImplPlus()
		{
		}
		
		public ISupportAImplSuperGImplPlus(String valueG, String valueA, String valueBaseAB, String valueB, String valueC)
		{
			this.valueG = valueG;
			this.valueA = valueA;
			this.valueBaseAB = valueBaseAB;
			this.valueB = valueB;
			this.valueC = valueC;
		}
	}
}
