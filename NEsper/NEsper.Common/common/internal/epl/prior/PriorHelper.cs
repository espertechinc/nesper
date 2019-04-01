///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.prior;

namespace com.espertech.esper.common.@internal.epl.prior
{
    public class PriorHelper
    {
        public static PriorEventViewFactory FindPriorViewFactory(ViewFactory[] factories)
        {
            ViewFactory factoryFound = null;
            foreach (ViewFactory factory in factories)
            {
                if (factory is PriorEventViewFactory)
                {
                    factoryFound = factory;
                    break;
                }
            }
            if (factoryFound == null)
            {
                throw new EPRuntimeException("Failed to find 'prior'-handling view factory");  // was verified earlier, should not occur
            }
            return (PriorEventViewFactory) factoryFound;
        }

        public static PriorEvalStrategy ToStrategy(AgentInstanceViewFactoryChainContext viewFactoryChainContext)
        {
            ViewUpdatedCollection priorViewUpdatedCollection = viewFactoryChainContext.PriorViewUpdatedCollection;
            if (priorViewUpdatedCollection is RandomAccessByIndex)
            {
                return new ExprPriorEvalStrategyRandomAccess((RandomAccessByIndex) priorViewUpdatedCollection);
            }
            if (priorViewUpdatedCollection is RelativeAccessByEventNIndex)
            {
                return new ExprPriorEvalStrategyRelativeAccess((RelativeAccessByEventNIndex) priorViewUpdatedCollection);
            }
            return null;
        }
    }
} // end of namespace