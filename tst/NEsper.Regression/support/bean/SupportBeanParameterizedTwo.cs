///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
	public class SupportBeanParameterizedTwo<A, B>
	{
		private readonly A one;
		private readonly B two;

		public SupportBeanParameterizedTwo(
			A one,
			B two)
		{
			this.one = one;
			this.two = two;
		}

		public A One => one;

		public B Two => two;
	}
} // end of namespace
