///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class EPRuntimeStatementSelectionSPI
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		// Predefined properties available:
		// - name (string)
		// - description (string)
		// - epl (string)
		// - each tag individually (string)
		// - priority
		// - drop (boolean)
		// - hint (string)
		private readonly EPRuntimeSPI _runtimeSPI;
		private readonly BeanEventType _statementRowType;

		public EPRuntimeStatementSelectionSPI(EPRuntimeSPI runtimeSPI)
		{
			_runtimeSPI = runtimeSPI;
			_statementRowType = new EPRuntimeBeanAnonymousTypeService(runtimeSPI.Container)
				.MakeBeanEventTypeAnonymous(typeof(StatementRow));
		}

		public ExprNode CompileFilterExpression(string filterExpression)
		{
			try {
				return _runtimeSPI.ReflectiveCompileSvc.ReflectiveCompileExpression(
					filterExpression,
					new EventType[] {_statementRowType},
					new string[] {_statementRowType.Name});
			}
			catch (Exception ex) {
				throw new EPException("Failed to compiler filter: " + ex.Message, ex);
			}
		}

		public void TraverseStatementsContains(
			BiConsumer<EPDeployment, EPStatement> consumer,
			string containsIgnoreCase)
		{
			_runtimeSPI.TraverseStatements(
				(
					deployment,
					stmt) => {
					var match = false;
					var searchString = containsIgnoreCase.ToLowerInvariant();
					if (stmt.Name.ToLowerInvariant().Contains(searchString)) {
						match = true;
					}

					if (!match) {
						var epl = (string) stmt.GetProperty(StatementProperty.EPL);
						if ((epl != null) && (epl.ToLowerInvariant().Contains(searchString))) {
							match = true;
						}
					}

					if (!match) {
						return;
					}

					consumer.Invoke(deployment, stmt);
				});
		}

		public void TraverseStatementsFilterExpr(
			BiConsumer<EPDeployment, EPStatement> consumer,
			ExprNode filterExpr)
		{
			_runtimeSPI.TraverseStatements(
				(
					deployment,
					stmt) => {
					if (EvaluateStatement(filterExpr, stmt)) {
						consumer.Invoke(deployment, stmt);
					}
				});
		}

		private static StatementRow GetRow(EPStatement statement)
		{
			string description = null;
			string hint = null;
			var hintDelimiter = "";
			var priority = 0;
			IDictionary<string, string> tags = null;
			var drop = false;

			var annotations = statement.Annotations;
			foreach (var anno in annotations) {
				if (anno is HintAttribute hintAttribute) {
					if (hint == null) {
						hint = "";
					}

					hint += hintDelimiter + hintAttribute.Value;
					hintDelimiter = ",";
				}
				else if (anno is TagAttribute tagAttribute) {
					if (tags == null) {
						tags = new Dictionary<string, string>();
					}

					tags.Put(tagAttribute.Name, tagAttribute.Value);
				}
				else if (anno is PriorityAttribute priorityAttribute) {
					priority = priorityAttribute.Value;
				}
				else if (anno is DropAttribute) {
					drop = true;
				}
				else if (anno is DescriptionAttribute descriptionAttribute) {
					description = descriptionAttribute.Value;
				}
			}

			return new StatementRow(
				statement.DeploymentId,
				statement.Name,
				(string) statement.GetProperty(StatementProperty.EPL),
				statement.UserObjectCompileTime,
				statement.UserObjectRuntime,
				description,
				hint,
				priority,
				drop,
				tags
			);
		}

		public bool EvaluateStatement(
			ExprNode expression,
			EPStatement stmt)
		{
			if (expression == null) {
				return true;
			}

			var returnType = expression.Forge.EvaluationType;
			if (!returnType.IsBoolean()) {
				throw new EPException(
					"Invalid expression, expected a boolean return type for expression and received '" +
					returnType.CleanName() +
					"' for expression '" +
					ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(expression) +
					"'");
			}

			var evaluator = expression.Forge.ExprEvaluator;

			try {
				var row = GetRow(stmt);
				EventBean rowBean = new BeanEventBean(row, _statementRowType);

				var pass = evaluator.Evaluate(new EventBean[] {rowBean}, true, null);
				return true.Equals(pass);
				//return !((pass == null) || (false.Equals(pass)));
			}
			catch (Exception ex) {
				log.Error("Unexpected exception filtering statements by expression, skipping statement: " + ex.Message, ex);
			}

			return false;
		}

		public class StatementRow
		{
			public StatementRow(
				string deploymentId,
				string name,
				string epl,
				object userObjectCompileTime,
				object userObjectRuntimeTime,
				string description,
				string hint,
				int priority,
				Boolean drop,
				IDictionary<string, string> tag)
			{
				DeploymentId = deploymentId;
				Name = name;
				Epl = epl;
				UserObjectCompileTime = userObjectCompileTime;
				UserObjectRuntimeTime = userObjectRuntimeTime;
				Description = description;
				Hint = hint;
				Priority = priority;
				IsDrop = drop;
				Tag = tag;
			}

			public string Name { get; set; }

			public string Epl { get; set; }

			public string Description { get; set; }

			public string Hint { get; set; }

			public int Priority { get; set; }

			public bool IsDrop { get; set; }

			public object UserObjectCompileTime { get; set; }

			public object UserObjectRuntimeTime { get; set; }

			public IDictionary<string, string> Tag { get; set; }

			public string DeploymentId { get; set; }
		}
	}
} // end of namespace
