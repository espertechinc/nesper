///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.function;

namespace com.espertech.esper.regressionlib.support.expreval
{
	public class SupportEvalExpectedAssertion : SupportEvalExpected
	{
		private readonly Consumer<object> _verifier;

		public SupportEvalExpectedAssertion(Consumer<object> verifier)
		{
			_verifier = verifier;
		}

		public override void AssertValue(
			string message,
			object actual)
		{
			_verifier.Invoke(actual);
		}
	}
} // end of namespace
