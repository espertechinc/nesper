///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client.hook;

namespace com.espertech.esper.supportregression.client
{
    public class SupportConditionHandlerFactory : ConditionHandlerFactory
    {
        static SupportConditionHandlerFactory()
        {
            Handlers = new List<SupportConditionHandler>();
            FactoryContexts = new List<ConditionHandlerFactoryContext>();
        }

        public ConditionHandler GetHandler(ConditionHandlerFactoryContext context)
        {
            FactoryContexts.Add(context);
            var handler = new SupportConditionHandler();
            Handlers.Add(handler);
            return handler.Handle;
        }

        public static List<ConditionHandlerFactoryContext> FactoryContexts { get; private set; }

        public static List<SupportConditionHandler> Handlers { get; private set; }

        public static SupportConditionHandler LastHandler
        {
            get { return Handlers[Handlers.Count - 1]; }
        }

        public class SupportConditionHandler
        {
            private readonly List<ConditionHandlerContext> _contexts = new List<ConditionHandlerContext>();

            public SupportConditionHandler()
            {
            }

            public void Handle(ConditionHandlerContext context)
            {
                _contexts.Add(context);
            }

            public List<ConditionHandlerContext> Contexts
            {
                get { return _contexts; }
            }

            public List<ConditionHandlerContext> GetAndResetContexts()
            {
                var result = new List<ConditionHandlerContext>(_contexts);
                _contexts.Clear();
                return result;
            }
        }
    }
}
