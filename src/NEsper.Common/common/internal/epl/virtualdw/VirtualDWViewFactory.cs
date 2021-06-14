///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.virtualdw
{
    public class VirtualDWViewFactory : DataWindowViewFactory
    {
        private EventBeanFactory eventBeanFactory;

        public VirtualDataWindowFactory Factory { get; set; }

        public object[] Parameters { get; set; }

        public ExprEvaluator[] ParameterExpressions { get; set; }

        public string NamedWindowName { get; set; }

        public object CompileTimeConfiguration { get; set; }

        public EventType EventType { get; set; }

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
            try {
                eventBeanFactory = EventTypeUtility.GetFactoryForType(
                    EventType,
                    services.EventBeanTypedEventFactory,
                    services.EventTypeAvroHandler);
                var factoryContext = new VirtualDataWindowFactoryContext(
                    EventType,
                    Parameters,
                    ParameterExpressions,
                    NamedWindowName,
                    CompileTimeConfiguration,
                    viewFactoryContext,
                    services);
                Factory.Initialize(factoryContext);
            }
            catch (Exception ex) {
                throw new EPException(
                    "Validation exception initializing virtual data window '" + NamedWindowName + "': " + ex.Message,
                    ex);
            }
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var outputStream = new VirtualDataWindowOutStreamImpl();
            VirtualDataWindow window;
            try {
                var context = new VirtualDataWindowContext(
                    this,
                    agentInstanceViewFactoryContext,
                    eventBeanFactory,
                    outputStream);
                window = Factory.Create(context);
            }
            catch (Exception ex) {
                throw new EPException(
                    "Exception returned by virtual data window factory upon creation: " + ex.Message,
                    ex);
            }

            var view = new VirtualDWViewImpl(this, agentInstanceViewFactoryContext.AgentInstanceContext, window);
            outputStream.View = view;
            return view;
        }

        public string ViewName => "virtual-data-window";

        public void Destroy()
        {
            Factory.Destroy();
        }
    }
} // end of namespace