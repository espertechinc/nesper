///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.epl.core;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// A result delivery strategy that uses a matching "Update" method and optional start, 
    /// end, and updateRStream methods, to deliver column-wise to parameters of the Update method.
    /// </summary>
    public class ResultDeliveryStrategyImpl : ResultDeliveryStrategy
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPStatement _statement;
        private readonly object _subscriber;
        private readonly FastMethod _updateMethodFast;
        private readonly FastMethod _startMethodFast;
        private readonly bool _startMethodHasEPStatement;
        private readonly FastMethod _endMethodFast;
        private readonly bool _endMethodHasEPStatement;
        private readonly FastMethod _updateRStreamMethodFast;
        private readonly DeliveryConvertor _deliveryConvertor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultDeliveryStrategyImpl" /> class.
        /// </summary>
        /// <param name="statement">The statement.</param>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="deliveryConvertor">The delivery convertor.</param>
        /// <param name="method">The method.</param>
        /// <param name="startMethod">The start method.</param>
        /// <param name="endMethod">The end method.</param>
        /// <param name="rStreamMethod">The r stream method.</param>
        /// <param name="engineImportService">The engine import service.</param>
        public ResultDeliveryStrategyImpl(
            EPStatement statement,
            object subscriber,
            DeliveryConvertor deliveryConvertor,
            MethodInfo method,
            MethodInfo startMethod,
            MethodInfo endMethod,
            MethodInfo rStreamMethod,
            EngineImportService engineImportService)
        {
            _statement = statement;
            _subscriber = subscriber;
            _deliveryConvertor = deliveryConvertor;
            _updateMethodFast = FastClass.CreateMethod(method);

            if (startMethod != null)
            {
                _startMethodFast = FastClass.CreateMethod(startMethod);
                _startMethodHasEPStatement = IsMethodAcceptsStatement(startMethod);
            }
            else
            {
                _startMethodFast = null;
                _startMethodHasEPStatement = false;
            }

            if (endMethod != null)
            {
                _endMethodFast = FastClass.CreateMethod(endMethod);
                _endMethodHasEPStatement = IsMethodAcceptsStatement(endMethod);
            }
            else
            {
                _endMethodFast = null;
                _endMethodHasEPStatement = false;
            }

            _updateRStreamMethodFast = rStreamMethod != null ? FastClass.CreateMethod(rStreamMethod) : null;
        }

        #region ResultDeliveryStrategy Members

        public void Execute(UniformPair<EventBean[]> result)
        {
            if (_startMethodFast != null)
            {
                var countNew = 0;
                var countOld = 0;
                if (result != null)
                {
                    countNew = Count(result.First);
                    countOld = Count(result.Second);
                }

                object[] paramList;
                if (!_startMethodHasEPStatement)
                {
                    paramList = new object[] { countNew, countOld };
                }
                else
                {
                    paramList = new object[] { _statement, countNew, countOld };
                }

                try
                {
                    _startMethodFast.Invoke(_subscriber, paramList);
                }
                catch (TargetInvocationException e)
                {
                    Handle(_statement.Name, Log, e, paramList, _subscriber, _startMethodFast);
                }
                catch (Exception e)
                {
                    HandleThrowable(Log, e, null, _subscriber, _startMethodFast);
                }
            }

            EventBean[] newData = null;
            EventBean[] oldData = null;
            if (result != null)
            {
                newData = result.First;
                oldData = result.Second;
            }

            if ((newData != null) && (newData.Length > 0))
            {
                for (var i = 0; i < newData.Length; i++)
                {
                    var theEvent = newData[i];
                    if (theEvent is NaturalEventBean)
                    {
                        var natural = (NaturalEventBean)theEvent;
                        var paramList = _deliveryConvertor.ConvertRow(natural.Natural);
                        try
                        {
                            _updateMethodFast.Invoke(_subscriber, paramList);
                        }
                        catch (TargetInvocationException e)
                        {
                            Handle(_statement.Name, Log, e, paramList, _subscriber, _updateMethodFast);
                        }
                        catch (Exception e)
                        {
                            HandleThrowable(Log, e, paramList, _subscriber, _updateMethodFast);
                        }
                    }
                }
            }

            if ((_updateRStreamMethodFast != null) && (oldData != null) && (oldData.Length > 0))
            {
                for (var i = 0; i < oldData.Length; i++)
                {
                    var theEvent = oldData[i];
                    if (theEvent is NaturalEventBean)
                    {
                        var natural = (NaturalEventBean)theEvent;
                        var paramList = _deliveryConvertor.ConvertRow(natural.Natural);
                        try
                        {
                            _updateRStreamMethodFast.Invoke(_subscriber, paramList);
                        }
                        catch (TargetInvocationException e)
                        {
                            Handle(_statement.Name, Log, e, paramList, _subscriber, _updateRStreamMethodFast);
                        }
                        catch (Exception e)
                        {
                            HandleThrowable(Log, e, paramList, _subscriber, _updateRStreamMethodFast);
                        }
                    }
                }
            }

            if (_endMethodFast != null)
            {
                var paramList = _endMethodHasEPStatement ? new object[] { _statement } : new object[] { };
                try
                {
                    _endMethodFast.Invoke(_subscriber, paramList);
                }
                catch (TargetInvocationException e)
                {
                    Handle(_statement.Name, Log, e, null, _subscriber, _endMethodFast);
                }
                catch (Exception e)
                {
                    HandleThrowable(Log, e, null, _subscriber, _endMethodFast);
                }
            }
        }

        #endregion

        /// <summary>
        /// Handle the exception, displaying a nice message and converting to <seealso cref="EPException"/>.
        /// </summary>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="logger">is the logger to use for error logging</param>
        /// <param name="e">is the exception</param>
        /// <param name="paramList">the method parameters</param>
        /// <param name="subscriber">the object to deliver to</param>
        /// <param name="method">the method to call</param>
        /// <throws>EPException converted from the passed invocation exception</throws>
        protected internal static void Handle(string statementName,
                                              ILog logger,
                                              TargetInvocationException e,
                                              object[] paramList,
                                              object subscriber,
                                              FastMethod method)
        {
            var message = TypeHelper.GetMessageInvocationTarget(statementName, method.Target,
                                                                   subscriber.GetType().FullName,
                                                                   paramList, e);
            logger.Error(message, e.InnerException);
        }

        /// <summary>Handle the exception, displaying a nice message and converting to <seealso cref="EPException" />. </summary>
        /// <param name="logger">is the logger to use for error logging</param>
        /// <param name="e">is the throwable</param>
        /// <param name="paramList">the method parameters</param>
        /// <param name="subscriber">the object to deliver to</param>
        /// <param name="method">the method to call</param>
        /// <throws>EPException converted from the passed invocation exception</throws>
        internal static void HandleThrowable(ILog logger,
                                              Exception e,
                                              object[] paramList,
                                              object subscriber,
                                              FastMethod method)
        {
            var message = "Unexpected exception when invoking method '" + method.Name +
                             "' on subscriber class '" + subscriber.GetType().Name +
                             "' for parameters " + ((paramList == null) ? "null" : paramList.Render()) +
                             " : " + e.GetType().Name + " : " + e.Message;
            logger.Error(message, e);
        }

        private static int Count(EventBean[] events)
        {
            if (events == null)
            {
                return 0;
            }
            var count = 0;
            for (var i = 0; i < events.Length; i++)
            {
                var theEvent = events[i];
                if (theEvent is NaturalEventBean)
                {
                    count++;
                }
            }
            return count;
        }

        private static bool IsMethodAcceptsStatement(MethodInfo method)
        {
            return method.GetParameterTypes().Length > 0 && method.GetParameterTypes()[0] == typeof(EPStatement);
        }
    }
}