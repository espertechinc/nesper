///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.epl.agg.service
{
    public class AggregationServiceMatchRecognizeFactoryDesc
    {
        public AggregationServiceMatchRecognizeFactoryDesc(AggregationServiceMatchRecognizeFactory aggregationServiceFactory, IList<AggregationServiceAggExpressionDesc> expressions)
        {
            AggregationServiceFactory = aggregationServiceFactory;
            Expressions = expressions;
        }

        public AggregationServiceMatchRecognizeFactory AggregationServiceFactory { get; private set; }

        public IList<AggregationServiceAggExpressionDesc> Expressions { get; private set; }
    }
}
