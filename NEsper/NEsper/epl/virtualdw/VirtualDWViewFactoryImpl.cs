///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.virtualdw
{
    public class VirtualDWViewFactoryImpl 
        : ViewFactory
        , DataWindowViewFactory
        , VirtualDWViewFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly object _customConfiguration;
        private ViewFactoryContext _viewFactoryContext;
        private IList<ExprNode> _viewParameters;
        private readonly string _namedWindowName;
        private readonly VirtualDataWindowFactory _virtualDataWindowFactory;
        private EventType _parentEventType;
        private Object[] _viewParameterArr;
        private ExprNode[] _viewParameterExp;
        private EventBeanFactory _eventBeanFactory;

        public VirtualDWViewFactoryImpl(Type first, string namedWindowName, object customConfiguration)
        {
            if (!first.IsImplementsInterface(typeof(VirtualDataWindowFactory))) {
                throw new ViewProcessingException("Virtual data window factory class " + Name.Clean(first) + " does not implement the interface " + Name.Clean<VirtualDataWindowFactory>());
            }
            _customConfiguration = customConfiguration;
            _namedWindowName = namedWindowName;
            _virtualDataWindowFactory = TypeHelper.Instantiate<VirtualDataWindowFactory>(first);
        }

        public ICollection<string> UniqueKeys
        {
            get { return _virtualDataWindowFactory.UniqueKeyPropertyNames; }
        }

        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParameters)
        {
            _viewFactoryContext = viewFactoryContext;
            _viewParameters = viewParameters;
        }

        public void Attach(
            EventType parentEventType,
            StatementContext statementContext,
            ViewFactory optionalParentFactory,
            IList<ViewFactory> parentViewFactories)
        {
            _parentEventType = parentEventType;

            ExprNode[] validatedNodes = ViewFactorySupport.Validate(
                _viewFactoryContext.ViewName, parentEventType, _viewFactoryContext.StatementContext, _viewParameters, true);
            _viewParameterArr = new Object[validatedNodes.Length];
            var evaluatorContextStmt = new ExprEvaluatorContextStatement(_viewFactoryContext.StatementContext, false);
            for (int i = 0; i < validatedNodes.Length; i++)
            {
                try
                {
                    _viewParameterArr[i] = ViewFactorySupport.EvaluateAssertNoProperties(
                        _viewFactoryContext.ViewName, validatedNodes[i], i, evaluatorContextStmt);
                }
                catch (Exception)
                {
                    // expected
                }
            }

            _viewParameterExp = ViewFactorySupport.Validate(
                _viewFactoryContext.ViewName, parentEventType, _viewFactoryContext.StatementContext, _viewParameters, true);

            // initialize
            try
            {
                _eventBeanFactory = EventAdapterServiceHelper.GetFactoryForType(
                    parentEventType, statementContext.EventAdapterService);
                _virtualDataWindowFactory.Initialize(
                    new VirtualDataWindowFactoryContext(
                        parentEventType, _viewParameterArr, _viewParameterExp, _eventBeanFactory, _namedWindowName,
                        _viewFactoryContext, _customConfiguration));
            }
            catch (Exception ex)
            {
                throw new ViewParameterException(
                    "Validation exception initializing virtual data window '" + _namedWindowName + "': " + ex.Message, ex);
            }
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var outputStream = new VirtualDataWindowOutStreamImpl();
            var context = new VirtualDataWindowContext(
                agentInstanceViewFactoryContext.AgentInstanceContext, _parentEventType, _viewParameterArr,
                _viewParameterExp, _eventBeanFactory, outputStream, _namedWindowName, _viewFactoryContext,
                _customConfiguration);
            VirtualDataWindow window;
            try
            {
                window = _virtualDataWindowFactory.Create(context);
            }
            catch (Exception ex)
            {
                throw new ViewProcessingException(
                    "Exception returned by virtual data window factory upon creation: " + ex.Message, ex);
            }
            var view = new VirtualDWViewImpl(window, _parentEventType, _namedWindowName);
            outputStream.SetView(view);
            return view;
        }

        public EventType EventType
        {
            get { return _parentEventType; }
        }

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            return false;
        }
    
        public void DestroyNamedWindow()
        {
            if (_virtualDataWindowFactory != null) {
                _virtualDataWindowFactory.DestroyAllContextPartitions();
            }
        }

        public string ViewName
        {
            get { return "Virtual Data Window"; }
        }
    }
} // end of namespace
