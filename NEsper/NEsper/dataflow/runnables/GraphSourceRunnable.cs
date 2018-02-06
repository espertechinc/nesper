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
using System.Threading;

using com.espertech.esper.client.annotation;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.util;

namespace com.espertech.esper.dataflow.runnables
{
    public class GraphSourceRunnable : BaseRunnable
        , DataFlowSignalListener
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _engineUri;
        private readonly String _statementName;
        private readonly DataFlowSourceOperator _graphSource;
        private readonly String _dataFlowName;
        private readonly String _operatorName;
        private readonly int _operatorNumber;
        private readonly String _operatorPrettyPrint;
        private readonly EPDataFlowExceptionHandler _optionalExceptionHandler;
        private readonly bool _audit;

        private bool _shutdown;
        private IList<CompletionListener> _completionListeners;

        public GraphSourceRunnable(
            String engineURI,
            String statementName,
            DataFlowSourceOperator graphSource,
            String dataFlowName,
            String operatorName,
            int operatorNumber,
            String operatorPrettyPrint,
            EPDataFlowExceptionHandler optionalExceptionHandler,
            bool audit)
        {
            _engineUri = engineURI;
            _statementName = statementName;
            _graphSource = graphSource;
            _dataFlowName = dataFlowName;
            _operatorName = operatorName;
            _operatorNumber = operatorNumber;
            _operatorPrettyPrint = operatorPrettyPrint;
            _optionalExceptionHandler = optionalExceptionHandler;
            _audit = audit;
        }

        public void ProcessSignal(EPDataFlowSignal signal)
        {
            if (signal is EPDataFlowSignalFinalMarker)
            {
                _shutdown = true;
            }
        }

        public void Run()
        {
            try
            {
                RunLoop();
            }
            catch (ThreadInterruptedException ex)
            {
                Log.Debug("Interruped runnable: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                Log.Error("Exception encountered: " + ex.Message, ex);
                HandleException(ex);
            }

            InvokeCompletionListeners();
        }

        public void RunSync()
        {
            try
            {
                RunLoop();
            }
            catch (ThreadInterruptedException ex)
            {
                Log.Debug("Interruped runnable: " + ex.Message, ex);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error("Exception encountered: " + ex.Message, ex);
                HandleException(ex);
                throw;
            }
        }

        private void HandleException(Exception ex)
        {
            if (_optionalExceptionHandler == null)
            {
                return;
            }

            _optionalExceptionHandler.Handle(
                new EPDataFlowExceptionContext(_dataFlowName, _operatorName, _operatorNumber, _operatorPrettyPrint, ex));
        }

        private void RunLoop()
        {
            while (true)
            {
                if (_audit)
                {
                    AuditPath.AuditLog(
                        _engineUri, _statementName, AuditEnum.DATAFLOW_SOURCE,
                        "dataflow " + _dataFlowName + " operator " + _operatorName + "(" + _operatorNumber +
                        ") invoking source.Next()");
                }
                _graphSource.Next();

                if (_shutdown)
                {
                    break;
                }
            }
        }

        private void InvokeCompletionListeners()
        {
            lock (this)
            {
                if (_completionListeners != null)
                {
                    foreach (CompletionListener listener in _completionListeners)
                    {
                        listener.Invoke();
                    }
                }
            }
        }

        public void AddCompletionListener(CompletionListener completionListener)
        {
            lock (this)
            {
                if (_completionListeners == null)
                {
                    _completionListeners = new List<CompletionListener>();
                }
                _completionListeners.Add(completionListener);
            }
        }

        public void Next()
        {
            _graphSource.Next();
        }

        public void Shutdown()
        {
            _shutdown = true;
        }

        public bool IsShutdown()
        {
            return _shutdown;
        }
    }
}
