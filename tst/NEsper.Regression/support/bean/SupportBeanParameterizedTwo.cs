///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
	public class SupportBeanParameterizedTwo<TA, TB>
	{
		public TA One { get; }
		public TB Two { get; }

		public SupportBeanParameterizedTwo(
			TA one,
			TB two)
		{
			this.One = one;
			this.Two = two;
		}
	}
} // end of namespace
