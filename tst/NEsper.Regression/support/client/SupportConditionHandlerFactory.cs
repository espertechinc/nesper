///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.hook.condition;

namespace com.espertech.esper.regressionlib.support.client
{
    public class SupportConditionHandlerFactory : ConditionHandlerFactory
    {
        public static IList<ConditionHandlerFactoryContext> FactoryContexts { get; } =
            new List<ConditionHandlerFactoryContext>();

        public static IList<SupportConditionHandler> Handlers { get; } = new List<SupportConditionHandler>();

        public static SupportConditionHandler LastHandler => Handlers[Handlers.Count - 1];

        public ConditionHandler GetHandler(ConditionHandlerFactoryContext context)
        {
            FactoryContexts.Add(context);
            var handler = new SupportConditionHandler();
            Handlers.Add(handler);
            return handler.Handle;
        }

        public class SupportConditionHandler
        {
            public IList<ConditionHandlerContext> Contexts { get; } = new List<ConditionHandlerContext>();

            public void Handle(ConditionHandlerContext context)
            {
                Contexts.Add(context);
            }

            public IList<ConditionHandlerContext> GetAndResetContexts()
            {
                IList<ConditionHandlerContext> result = new List<ConditionHandlerContext>(Contexts);
                Contexts.Clear();
                return result;
            }
        }
    }
} // end of namespace