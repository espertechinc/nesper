///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;


namespace com.espertech.esper.regressionlib.support.expreval
{
	public class SupportEvalExpectedObject : SupportEvalExpected
	{
		private readonly object _expected;

		public SupportEvalExpectedObject(object expected)
		{
			_expected = expected;
		}

		public override void AssertValue(
			string message,
			object actual)
		{
			EPAssertionUtil.AssertEqualsAllowArray(message, _expected, actual);
		}
	}
} // end of namespace
