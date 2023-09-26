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
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.derived;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.extend.view
{
    public class MyTrendSpotterViewForge : ViewFactoryForge
    {
        private ExprNode parameter;
        private IList<ExprNode> viewParameters;

        public void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            viewParameters = parameters;
        }
        
        public IList<ViewFactoryForge> InnerForges =>
            EmptyList<ViewFactoryForge>.Instance;

        public IList<StmtClassForgeableFactory> InitAdditionalForgeables(ViewForgeEnv viewForgeEnv) =>
            EmptyList<StmtClassForgeableFactory>.Instance;

        public void Attach(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            var validated = ViewForgeSupport.Validate(
                "Trend spotter view",
                parentEventType,
                viewParameters,
                false,
                viewForgeEnv,
                streamNumber);
            var message = "Trend spotter view accepts a single integer or double value";
            if (validated.Length != 1) {
                throw new ViewParameterException(message);
            }

            var resultType = validated[0].Forge.EvaluationType;
            if (resultType != typeof(int?) &&
                resultType != typeof(int) &&
                resultType != typeof(double?) &&
                resultType != typeof(double)) {
                throw new ViewParameterException(message);
            }

            parameter = validated[0];

            var eventTypeMap = new LinkedHashMap<string, object>();
            eventTypeMap.Put("trendcount", typeof(long?));

            EventType = DerivedViewTypeUtil.NewType("trendview", eventTypeMap, viewForgeEnv, streamNumber);
        }

        public EventType EventType { get; private set; }

        public string ViewName => "Trend-spotter";

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenSymbolProvider symbols,
            CodegenClassScope classScope)
        {
            return new SAIFFInitializeBuilder(
                    typeof(MyTrendSpotterViewFactory),
                    GetType(),
                    "factory",
                    parent,
                    (SAIFFInitializeSymbol) symbols,
                    classScope)
                .Eventtype("eventType", EventType)
                .Exprnode("parameter", parameter)
                .Build();
        }

        public void Accept(ViewForgeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
} // end of namespace