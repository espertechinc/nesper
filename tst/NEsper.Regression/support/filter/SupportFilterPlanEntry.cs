///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.filter
{
	public class SupportFilterPlanEntry
	{
		private readonly EventType _eventType;
		private readonly FilterSpecPlanForge _plan;
		private readonly IList<ExprNode> _planNodes;

		public SupportFilterPlanEntry(
			EventType eventType,
			FilterSpecPlanForge forges,
			IList<ExprNode> planNodes)
		{
			_eventType = eventType;
			_plan = forges;
			_planNodes = planNodes;
		}

		public EventType EventType => _eventType;

		public FilterSpecPlanForge Plan => _plan;

		public IList<ExprNode> PlanNodes => _planNodes;

		public FilterSpecParamForge GetAssertSingle(string typeName)
		{
			ClassicAssert.AreEqual(typeName, _eventType.Name);
			ClassicAssert.AreEqual(1, _plan.Paths.Length);
			var path = _plan.Paths[0];
			ClassicAssert.AreEqual(1, path.Triplets.Length);
			return path.Triplets[0].Param;
		}
	}
} // end of namespace
