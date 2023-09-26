///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.derived;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.extend.view
{
	public class MyTrendSpotterViewForge : ViewFactoryForge
	{
		private IList<ExprNode> viewParameters;

		private ExprNode parameter;
		private EventType eventType;

		public void SetViewParameters(
			IList<ExprNode> parameters,
			ViewForgeEnv viewForgeEnv,
			int streamNumber)
		{
			this.viewParameters = parameters;
		}

		public void Attach(
			EventType parentEventType,
			ViewForgeEnv viewForgeEnv)
		{
			var validated = ViewForgeSupport.Validate(
				"Trend spotter view",
				parentEventType,
				viewParameters,
				false,
				viewForgeEnv);
			var message = "Trend spotter view accepts a single integer or double value";
			if (validated.Length != 1) {
				throw new ViewParameterException(message);
			}

			var resultEPType = validated[0].Forge.EvaluationType;
			if (!TypeHelper.IsTypeInteger(resultEPType) && !TypeHelper.IsTypeDouble(resultEPType)) {
				throw new ViewParameterException(message);
			}

			parameter = validated[0];

			var eventTypeMap = new LinkedHashMap<string, object>();
			eventTypeMap.Put("trendcount", typeof(long?));

			eventType = DerivedViewTypeUtil.NewType("trendview", eventTypeMap, viewForgeEnv);
		}

		public EventType EventType => eventType;

		public string ViewName => "Trend-spotter";

		public CodegenExpression Make(
			CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			return new SAIFFInitializeBuilder(
					typeof(MyTrendSpotterViewFactory),
					this.GetType(),
					"factory",
					parent,
					symbols,
					classScope)
				.Eventtype("eventType", eventType)
				.Exprnode("parameter", parameter)
				.Build();
		}

		public T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
		{
			return visitor.VisitExtension(this);
		}
	}
} // end of namespace
