///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    ///     Options for use when instantiating a data flow in <seealso cref="EPDataFlowService" />.
    /// </summary>
    public class EPDataFlowInstantiationOptions
    {
        private bool _cpuStatistics;
        private string _dataFlowInstanceId;
        private object _dataFlowInstanceUserObject;
        [JsonIgnore]
        [NonSerialized]
        private EPDataFlowOperatorProvider _operatorProvider;
        [JsonIgnore]
        [NonSerialized]
        private EPDataFlowOperatorParameterProvider _parameterProvider;
        [JsonIgnore]
        [NonSerialized]
        private EPDataFlowExceptionHandler _exceptionHandler;
        [JsonIgnore]
        [NonSerialized]
        private EPRuntimeEventProcessWrapped _surrogateEventSender;
        private bool _operatorStatistics;
        private IDictionary<string, object> _parametersUrIs;

        /// <summary>
        ///     Returns the operator provider.
        /// </summary>
        /// <returns>operator provider</returns>
        public EPDataFlowOperatorProvider OperatorProvider {
            get => _operatorProvider;
            set => _operatorProvider = value;
        }

        /// <summary>
        ///     Returns the parameter provider.
        /// </summary>
        /// <returns>parameter provider</returns>
        public EPDataFlowOperatorParameterProvider ParameterProvider {
            get => _parameterProvider;
            set => _parameterProvider = value;
        }

        /// <summary>
        ///     Returns the exception handler.
        /// </summary>
        /// <returns>exception handler.</returns>
        public EPDataFlowExceptionHandler ExceptionHandler {
            get => _exceptionHandler;
            set => _exceptionHandler = value;
        }

        /// <summary>
        ///     Returns the instance id assigned.
        /// </summary>
        /// <returns>instance if</returns>
        public string DataFlowInstanceId {
            get => _dataFlowInstanceId;
            set => _dataFlowInstanceId = value;
        }

        /// <summary>
        ///     Returns the user object associated to the data flow instance.
        /// </summary>
        /// <returns>user object</returns>
        public object DataFlowInstanceUserObject {
            get => _dataFlowInstanceUserObject;
            set => _dataFlowInstanceUserObject = value;
        }

        /// <summary>
        ///     Returns indicator whether to collect operator statistics.
        /// </summary>
        /// <returns>operator stats indicator</returns>
        public bool IsOperatorStatistics {
            get => _operatorStatistics;
            set => _operatorStatistics = value;
        }

        /// <summary>
        ///     Returns indicator whether to collect CPU statistics.
        /// </summary>
        /// <returns>CPU stats</returns>
        public bool IsCpuStatistics {
            get => _cpuStatistics;
            set => _cpuStatistics = value;
        }

        /// <summary>
        ///     Returns the event sender /runtime to use
        /// </summary>
        /// <returns>runtime.</returns>
        public EPRuntimeEventProcessWrapped SurrogateEventSender {
            get => _surrogateEventSender;
            set => _surrogateEventSender = value;
        }

        /// <summary>
        ///     Returns parameters.
        /// </summary>
        /// <value>parameters</value>
        public IDictionary<string, object> ParametersURIs {
            get => _parametersUrIs;
            set => _parametersUrIs = value;
        }

        /// <summary>
        ///     Sets the the operator provider.
        /// </summary>
        /// <param name="operatorProvider">operator provider</param>
        /// <returns>this options object</returns>
        public EPDataFlowInstantiationOptions WithOperatorProvider(EPDataFlowOperatorProvider operatorProvider)
        {
            _operatorProvider = operatorProvider;
            return this;
        }


        /// <summary>
        ///     Sets the parameter provider.
        /// </summary>
        /// <param name="parameterProvider">parameter provider</param>
        /// <returns>this options object</returns>
        public EPDataFlowInstantiationOptions WithParameterProvider(
            EPDataFlowOperatorParameterProvider parameterProvider)
        {
            _parameterProvider = parameterProvider;
            return this;
        }

        /// <summary>
        ///     Sets the exception handler.
        /// </summary>
        /// <param name="exceptionHandler">exception handler.</param>
        /// <returns>this options object</returns>
        public EPDataFlowInstantiationOptions WithExceptionHandler(EPDataFlowExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
            return this;
        }

        /// <summary>
        ///     Sets the data flow instance id
        /// </summary>
        /// <param name="dataFlowInstanceId">instance id</param>
        /// <returns>this options object</returns>
        public EPDataFlowInstantiationOptions WithDataFlowInstanceId(string dataFlowInstanceId)
        {
            _dataFlowInstanceId = dataFlowInstanceId;
            return this;
        }

        /// <summary>
        ///     Sets the user object associated to the data flow instance.
        /// </summary>
        /// <param name="dataFlowInstanceUserObject">user object</param>
        /// <returns>this options object</returns>
        public EPDataFlowInstantiationOptions WithDataFlowInstanceUserObject(object dataFlowInstanceUserObject)
        {
            _dataFlowInstanceUserObject = dataFlowInstanceUserObject;
            return this;
        }

        /// <summary>
        ///     Sets indicator whether to collect operator statistics.
        /// </summary>
        /// <param name="statistics">operator stats indicator</param>
        /// <returns>this options object</returns>
        public EPDataFlowInstantiationOptions WithOperatorStatistics(bool statistics)
        {
            _operatorStatistics = statistics;
            return this;
        }

        /// <summary>
        ///     Sets indicator whether to collect CPU statistics.
        /// </summary>
        /// <param name="cpuStatistics">CPU stats</param>
        /// <returns>this options object</returns>
        public EPDataFlowInstantiationOptions WithCpuStatistics(bool cpuStatistics)
        {
            _cpuStatistics = cpuStatistics;
            return this;
        }

        /// <summary>
        ///     Add a parameter.
        /// </summary>
        /// <param name="name">is the uri</param>
        /// <param name="value">the value</param>
        public void AddParameterURI(
            string name,
            object value)
        {
            if (_parametersUrIs == null) {
                _parametersUrIs = new Dictionary<string, object>();
            }

            _parametersUrIs.Put(name, value);
        }
    }
} // end of namespace