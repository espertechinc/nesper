///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.core.service;

namespace com.espertech.esper.client.dataflow
{
    /// <summary>
    /// Options for use when instantiating a data flow in <seealso cref="EPDataFlowRuntime" />
    /// </summary>
    [Serializable]
    public class EPDataFlowInstantiationOptions
    {
        private bool _cpuStatistics;
        private String _dataFlowInstanceId;
        private Object _dataFlowInstanceUserObject;
        private EPDataFlowExceptionHandler _exceptionHandler;
        private EPDataFlowOperatorProvider _operatorProvider;
        private bool _operatorStatistics;
        private EPDataFlowOperatorParameterProvider _parameterProvider;

        /// <summary>
        /// Gets or sets the event sender /runtime to use
        /// </summary>
        /// <value>The surrogate event sender.</value>
        public EPRuntimeEventSender SurrogateEventSender { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        public IDictionary<string, object> ParametersURIs { get; private set; }

        /// <summary>Returns the operator provider. </summary>
        /// <returns>operator provider</returns>
        public EPDataFlowOperatorProvider GetOperatorProvider()
        {
            return _operatorProvider;
        }

        /// <summary>Sets the the operator provider. </summary>
        /// <param name="operatorProvider">operator provider</param>
        /// <returns>this options object</returns>
        public EPDataFlowInstantiationOptions OperatorProvider(EPDataFlowOperatorProvider operatorProvider)
        {
            _operatorProvider = operatorProvider;
            return this;
        }

        /// <summary>Sets the the operator provider. </summary>
        /// <param name="operatorProvider">operator provider</param>
        public void SetOperatorProvider(EPDataFlowOperatorProvider operatorProvider)
        {
            _operatorProvider = operatorProvider;
        }

        /// <summary>Sets the parameter provider. </summary>
        /// <param name="parameterProvider">parameter provider</param>
        /// <returns>this options object</returns>
        public EPDataFlowInstantiationOptions ParameterProvider(EPDataFlowOperatorParameterProvider parameterProvider)
        {
            _parameterProvider = parameterProvider;
            return this;
        }

        /// <summary>Returns the parameter provider. </summary>
        /// <returns>parameter provider</returns>
        public EPDataFlowOperatorParameterProvider GetParameterProvider()
        {
            return _parameterProvider;
        }

        /// <summary>Sets the parameter provider. </summary>
        /// <param name="parameterProvider">parameter provider</param>
        public void SetParameterProvider(EPDataFlowOperatorParameterProvider parameterProvider)
        {
            _parameterProvider = parameterProvider;
        }

        /// <summary>Returns the exception handler. </summary>
        /// <returns>exception handler.</returns>
        public EPDataFlowExceptionHandler GetExceptionHandler()
        {
            return _exceptionHandler;
        }

        /// <summary>Sets the exception handler. </summary>
        /// <param name="exceptionHandler">exception handler.</param>
        /// <returns>this options object</returns>
        public EPDataFlowInstantiationOptions ExceptionHandler(EPDataFlowExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
            return this;
        }

        /// <summary>Sets the exception handler. </summary>
        /// <param name="exceptionHandler">exception handler.</param>
        public void SetExceptionHandler(EPDataFlowExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>Returns the instance id assigned. </summary>
        /// <returns>instance if</returns>
        public String GetDataFlowInstanceId()
        {
            return _dataFlowInstanceId;
        }

        /// <summary>Sets the data flow instance id </summary>
        /// <param name="dataFlowInstanceId">instance id</param>
        /// <returns>this options object</returns>
        public EPDataFlowInstantiationOptions DataFlowInstanceId(String dataFlowInstanceId)
        {
            _dataFlowInstanceId = dataFlowInstanceId;
            return this;
        }

        /// <summary>Sets the data flow instance id </summary>
        /// <param name="dataFlowInstanceId">instance id</param>
        public void SetDataFlowInstanceId(String dataFlowInstanceId)
        {
            _dataFlowInstanceId = dataFlowInstanceId;
        }

        /// <summary>Returns the user object associated to the data flow instance. </summary>
        /// <returns>user object</returns>
        public Object GetDataFlowInstanceUserObject()
        {
            return _dataFlowInstanceUserObject;
        }

        /// <summary>Sets the user object associated to the data flow instance. </summary>
        /// <param name="dataFlowInstanceUserObject">user object</param>
        /// <returns>this options object</returns>
        public EPDataFlowInstantiationOptions DataFlowInstanceUserObject(Object dataFlowInstanceUserObject)
        {
            _dataFlowInstanceUserObject = dataFlowInstanceUserObject;
            return this;
        }

        /// <summary>Sets the user object associated to the data flow instance. </summary>
        /// <param name="dataFlowInstanceUserObject">this options object</param>
        public void SetDataFlowInstanceUserObject(Object dataFlowInstanceUserObject)
        {
            _dataFlowInstanceUserObject = dataFlowInstanceUserObject;
        }

        /// <summary>Returns indicator whether to collect operator statistics. </summary>
        /// <returns>operator stats indicator</returns>
        public bool IsOperatorStatistics()
        {
            return _operatorStatistics;
        }

        /// <summary>Sets indicator whether to collect operator statistics. </summary>
        /// <param name="statistics">operator stats indicator</param>
        /// <returns>this options object</returns>
        public EPDataFlowInstantiationOptions OperatorStatistics(bool statistics)
        {
            _operatorStatistics = statistics;
            return this;
        }

        /// <summary>Sets indicator whether to collect operator statistics. </summary>
        /// <param name="operatorStatistics">operator stats indicator</param>
        public void SetOperatorStatistics(bool operatorStatistics)
        {
            _operatorStatistics = operatorStatistics;
        }

        /// <summary>Returns indicator whether to collect CPU statistics. </summary>
        /// <returns>CPU stats</returns>
        public bool IsCpuStatistics()
        {
            return _cpuStatistics;
        }

        /// <summary>Sets indicator whether to collect CPU statistics. </summary>
        /// <param name="cpuStatistics">CPU stats</param>
        public void SetCpuStatistics(bool cpuStatistics)
        {
            _cpuStatistics = cpuStatistics;
        }

        /// <summary>Sets indicator whether to collect CPU statistics. </summary>
        /// <param name="cpuStatistics">CPU stats</param>
        /// <returns>this options object</returns>
        public EPDataFlowInstantiationOptions CpuStatistics(bool cpuStatistics)
        {
            _cpuStatistics = cpuStatistics;
            return this;
        }

        /// <summary>
        /// Adds the parameter URI.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void AddParameterURI(string name, object value)
        {
            if (ParametersURIs == null)
            {
                ParametersURIs = new Dictionary<string, object>();
            }

            ParametersURIs[name] = value;
        }
    }
}