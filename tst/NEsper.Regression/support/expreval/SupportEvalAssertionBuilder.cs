///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.regressionlib.support.expreval
{
	public class SupportEvalAssertionBuilder
	{
		private readonly SupportEvalBuilder _builder;
		private readonly IDictionary<string, SupportEvalExpected> _results = new Dictionary<string, SupportEvalExpected>();

		public SupportEvalAssertionBuilder(SupportEvalBuilder builder)
		{
			_builder = builder;
		}

		public SupportEvalAssertionBuilder Expect(
			string name,
			object result)
		{
			VerifyExpect(name);
			_results.Put(name, new SupportEvalExpectedObject(result));
			return this;
		}

		public SupportEvalAssertionBuilder Verify(
			string name,
			Consumer<object> verifier)
		{
			VerifyExpect(name);
			_results.Put(name, new SupportEvalExpectedAssertion(verifier));
			return this;
		}

		public SupportEvalAssertionBuilder Expect(
			string[] names,
			params object[] results)
		{
			if (results == null) {
				throw new ArgumentException("Expected result array, for 'null' use 'new Object[] {null}'");
			}

			if (names.Length != results.Length) {
				throw new ArgumentException("Names length and results length differ");
			}

			for (int i = 0; i < names.Length; i++) {
				Expect(names[i], results[i]);
			}

			return this;
		}

		public IDictionary<string, SupportEvalExpected> Results {
			get { return _results; }
		}

		private void VerifyExpect(string name)
		{
			if (!_builder.Expressions.ContainsKey(name)) {
				throw new ArgumentException("No expression for name '" + name + "'");
			}

			if (_results.ContainsKey(name)) {
				throw new ArgumentException("Already have result for name '" + name + "'");
			}
		}
	}
} // end of namespace
