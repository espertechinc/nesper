///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.expreval
{
	public class SupportEvalBuilder
	{
		public SupportEvalBuilder(string eventType) : this(eventType, null)
		{
		}

		public SupportEvalBuilder(
			string eventType,
			string streamAlias)
		{
			EventType = eventType;
			StreamAlias = streamAlias;
		}

		public SupportEvalBuilder WithStatementConsumer(Consumer<EPStatement> statementConsumer)
		{
			StatementConsumer = statementConsumer;
			return this;
		}

		public SupportEvalBuilder WithExpression(
			string name,
			string expression)
		{
			if (Expressions.ContainsKey(name)) {
				throw new ArgumentException("Expression '" + name + "' already provided");
			}

			Expressions.Put(name, expression);
			return this;
		}

		public SupportEvalBuilder WithExpressions(
			string[] names,
			params string[] expressions)
		{
			if (names.Length != expressions.Length) {
				throw new ArgumentException("Names length and expressions length differ");
			}

			for (var i = 0; i < names.Length; i++) {
				WithExpression(names[i], expressions[i]);
			}

			return this;
		}

		public SupportEvalAssertionBuilder WithAssertion(object underlying)
		{
			var builder = new SupportEvalAssertionBuilder(this);
			Assertions.Add(new SupportEvalAssertionPair(underlying, builder));
			return builder;
		}

		public SupportEvalBuilder WithPath(RegressionPath path)
		{
			Path = path;
			return this;
		}

		public SupportEvalBuilder WithExcludeAssertionsExcept(int included)
		{
			ExcludeAssertionsExcept = included;
			return this;
		}

		public SupportEvalBuilder WithExcludeNamesExcept(string name)
		{
			ExcludeNamesExcept = name;
			return this;
		}

		public SupportEvalBuilder WithLogging(bool flag)
		{
			IsLogging = flag;
			return this;
		}

		public SupportEvalBuilder WithExcludeEPLAssertion(bool flag)
		{
			IsExcludeEPLAssertion = flag;
			return this;
		}

		public IDictionary<string, string> Expressions { get; } = new LinkedHashMap<string, string>();

		public string EventType { get; }

		public IList<SupportEvalAssertionPair> Assertions { get; } = new List<SupportEvalAssertionPair>();

		public Consumer<EPStatement> StatementConsumer { get; private set; }

		public string StreamAlias { get; }

		public RegressionPath Path { get; set; }

		public int? ExcludeAssertionsExcept { get; private set; }

		public string ExcludeNamesExcept { get; private set; }

		public bool IsLogging { get; set; }

		public bool IsExcludeEPLAssertion { get; set; }

		public SupportEvalBuilderResult Run(RegressionEnvironment environment)
		{
			return Run(environment, false);
		}

		public SupportEvalBuilderResult Run(
			RegressionEnvironment environment,
			bool soda)
		{
			return SupportEvalRunner.Run(environment, soda, this);
		}
	}
} // end of namespace
