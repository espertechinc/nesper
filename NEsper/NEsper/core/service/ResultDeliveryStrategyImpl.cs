///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
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
        private readonly DeliveryConvertor _deliveryConvertor;
        private readonly FastMethod _endFastMethod;
        private readonly FastMethod _startFastMethod;

        private readonly String _statementName;
        private readonly Object _subscriber;
        private readonly FastMethod _updateFastMethod;
        private readonly FastMethod _updateRStreamFastMethod;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="subscriber">is the subscriber receiving method invocations</param>
        /// <param name="deliveryConvertor">for converting individual rows</param>
        /// <param name="method">to deliver the insert stream to</param>
        /// <param name="startMethod">to call to indicate when delivery starts, or null if no such indication is required</param>
        /// <param name="endMethod">to call to indicate when delivery ends, or null if no such indication is required</param>
        /// <param name="rStreamMethod">to deliver the remove stream to, or null if no such indication is required</param>
        public ResultDeliveryStrategyImpl(String statementName,
                                          Object subscriber,
                                          DeliveryConvertor deliveryConvertor,
                                          MethodInfo method,
                                          MethodInfo startMethod,
                                          MethodInfo endMethod,
                                          MethodInfo rStreamMethod)
        {
            _statementName = statementName;
            _subscriber = subscriber;
            _deliveryConvertor = deliveryConvertor;
            FastClass fastClass = FastClass.Create(subscriber.GetType());
            _updateFastMethod = fastClass.GetMethod(method);

            _startFastMethod = startMethod != null ? fastClass.GetMethod(startMethod) : null;

            _endFastMethod = endMethod != null ? fastClass.GetMethod(endMethod) : null;

            _updateRStreamFastMethod = rStreamMethod != null ? fastClass.GetMethod(rStreamMethod) : null;
        }

        #region ResultDeliveryStrategy Members

        public void Execute(UniformPair<EventBean[]> result)
        {
            if (_startFastMethod != null)
            {
                int countNew = 0;
                int countOld = 0;
                if (result != null)
                {
                    countNew = Count(result.First);
                    countOld = Count(result.Second);
                }

                var paramList = new Object[] {countNew, countOld};
                try
                {
                    _startFastMethod.Invoke(_subscriber, paramList);
                }
                catch (TargetInvocationException e)
                {
                    Handle(_statementName, Log, e, paramList, _subscriber, _startFastMethod);
                }
                catch (Exception e)
                {
                    HandleThrowable(Log, e, null, _subscriber, _startFastMethod);
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
                for (int i = 0; i < newData.Length; i++)
                {
                    EventBean theEvent = newData[i];
                    if (theEvent is NaturalEventBean)
                    {
                        var natural = (NaturalEventBean) theEvent;
                        Object[] paramList = _deliveryConvertor.ConvertRow(natural.Natural);
                        try
                        {
                            _updateFastMethod.Invoke(_subscriber, paramList);
                        }
                        catch (TargetInvocationException e)
                        {
                            Handle(_statementName, Log, e, paramList, _subscriber, _updateFastMethod);
                        }
                        catch (Exception e)
                        {
                            HandleThrowable(Log, e, paramList, _subscriber, _updateFastMethod);
                        }
                    }
                }
            }

            if ((_updateRStreamFastMethod != null) && (oldData != null) && (oldData.Length > 0))
            {
                for (int i = 0; i < oldData.Length; i++)
                {
                    EventBean theEvent = oldData[i];
                    if (theEvent is NaturalEventBean)
                    {
                        var natural = (NaturalEventBean) theEvent;
                        Object[] paramList = _deliveryConvertor.ConvertRow(natural.Natural);
                        try
                        {
                            _updateRStreamFastMethod.Invoke(_subscriber, paramList);
                        }
                        catch (TargetInvocationException e)
                        {
                            Handle(_statementName, Log, e, paramList, _subscriber, _updateRStreamFastMethod);
                        }
                        catch (Exception e)
                        {
                            HandleThrowable(Log, e, paramList, _subscriber, _updateRStreamFastMethod);
                        }
                    }
                }
            }

            if (_endFastMethod != null)
            {
                try
                {
                    _endFastMethod.Invoke(_subscriber, null);
                }
                catch (TargetInvocationException e)
                {
                    Handle(_statementName, Log, e, null, _subscriber, _endFastMethod);
                }
                catch (Exception e)
                {
                    HandleThrowable(Log, e, null, _subscriber, _endFastMethod);
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
        protected internal static void Handle(String statementName,
                                              ILog logger,
                                              TargetInvocationException e,
                                              Object[] paramList,
                                              Object subscriber,
                                              FastMethod method)
        {
            String message = TypeHelper.GetMessageInvocationTarget(statementName, method.Target,
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
                                              Object[] paramList,
                                              Object subscriber,
                                              FastMethod method)
        {
            String message = "Unexpected exception when invoking method '" + method.Name +
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
            int count = 0;
            for (int i = 0; i < events.Length; i++)
            {
                EventBean theEvent = events[i];
                if (theEvent is NaturalEventBean)
                {
                    count++;
                }
            }
            return count;
        }
    }
}