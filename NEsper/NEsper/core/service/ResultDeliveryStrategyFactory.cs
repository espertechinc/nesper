///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Factory for creating a dispatch strategy based on the subscriber object and the columns produced by a select-clause.
    /// </summary>
    public class ResultDeliveryStrategyFactory
    {
        /// <summary>
        /// Creates a strategy implementation that indicates to subscribers the statement results based on the select-clause columns.
        /// </summary>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="subscriber">to indicate to</param>
        /// <param name="selectClauseTypes">are the types of each column in the select clause</param>
        /// <param name="selectClauseColumns">the names of each column in the select clause</param>
        /// <returns>strategy for dispatching naturals</returns>
        /// <throws>EPSubscriberException if the subscriber is invalid</throws>
        public static ResultDeliveryStrategy Create(
            string statementName,
            EPSubscriber subscriber,
            Type[] selectClauseTypes,
            string[] selectClauseColumns)
        {
            var subscriberObject = subscriber.Subscriber;
            var subscriberMethod = subscriber.SubscriberMethod;

            if (selectClauseTypes == null)
            {
                selectClauseTypes = new Type[0];
                selectClauseColumns = new String[0];
            }

            var subscriberType = subscriberObject.GetType();
            if (subscriberMethod == null)
            {
                if (subscriberType.IsDelegate())
                {
                    subscriberMethod = "Invoke";
                }
                else
                {
                    subscriberMethod = "Update";
                }
            }

            // Locate Update methods
            MethodInfo subscriptionMethod = null;
            List<MethodInfo> updateMethods;
            
            updateMethods = subscriberType
                .GetMethods()
                .Where(method => (method.Name == subscriberMethod) && (method.IsPublic))
                .ToList();

            // none found
            if (updateMethods.Count == 0)
            {
                String message = "EPSubscriber object does not provide a public method by name '" + subscriberMethod + "'";
                throw new EPSubscriberException(message);
            }

            // match to parameters
            bool isMapArrayDelivery = false;
            bool isObjectArrayDelivery = false;
            bool isSingleRowMap = false;
            bool isSingleRowObjectArr = false;
            bool isTypeArrayDelivery = false;

            // find an exact-matching method: no conversions and not even unboxing/boxing
            foreach (MethodInfo method in updateMethods)
            {
                Type[] parameters = method.GetParameterTypes();
                if (parameters.Length == selectClauseTypes.Length)
                {
                    bool fits = true;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if ((selectClauseTypes[i] != null) && (selectClauseTypes[i] != parameters[i]))
                        {
                            fits = false;
                            break;
                        }
                    }
                    if (fits)
                    {
                        subscriptionMethod = method;
                        break;
                    }
                }
            }

            // when not yet resolved, find an exact-matching method with boxing/unboxing
            if (subscriptionMethod == null)
            {
                foreach (MethodInfo method in updateMethods)
                {
                    Type[] parameters = method.GetParameterTypes();
                    if (parameters.Length == selectClauseTypes.Length)
                    {
                        bool fits = true;
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            Type boxedExpressionType = selectClauseTypes[i].GetBoxedType();
                            Type boxedParameterType = parameters[i].GetBoxedType();
                            if ((boxedExpressionType != null) && (boxedExpressionType != boxedParameterType))
                            {
                                fits = false;
                                break;
                            }
                        }
                        if (fits)
                        {
                            subscriptionMethod = method;
                            break;
                        }
                    }
                }
            }

            // when not yet resolved, find assignment-compatible methods that may require widening (including Integer to Long etc.)
            bool checkWidening = false;
            if (subscriptionMethod == null)
            {
                foreach (MethodInfo method in updateMethods)
                {
                    Type[] parameters = method.GetParameterTypes();
                    if (parameters.Length == selectClauseTypes.Length)
                    {
                        bool fits = true;
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            Type boxedExpressionType = selectClauseTypes[i].GetBoxedType();
                            Type boxedParameterType = parameters[i].GetBoxedType();
                            if ((boxedExpressionType != null) &&
                                (!boxedExpressionType.IsAssignmentCompatible(boxedParameterType)))
                            {
                                fits = false;
                                break;
                            }
                        }
                        if (fits)
                        {
                            subscriptionMethod = method;
                            checkWidening = true;
                            break;
                        }
                    }
                }
            }

            // when not yet resolved, find first-fit wildcard method
            if (subscriptionMethod == null)
            {
                foreach (MethodInfo method in updateMethods)
                {
                    Type[] parameters = method.GetParameterTypes();
                    if ((parameters.Length == 1) && (parameters[0] == typeof (DataMap)))
                    {
                        isSingleRowMap = true;
                        subscriptionMethod = method;
                        break;
                    }
                    if ((parameters.Length == 1) && (parameters[0] == typeof (Object[])))
                    {
                        isSingleRowObjectArr = true;
                        subscriptionMethod = method;
                        break;
                    }

                    if ((parameters.Length == 2) && (parameters[0] == typeof (DataMap[])) &&
                        (parameters[1] == typeof (DataMap[])))
                    {
                        subscriptionMethod = method;
                        isMapArrayDelivery = true;
                        break;
                    }
                    if ((parameters.Length == 2) && (parameters[0] == typeof (Object[][])) &&
                        (parameters[1] == typeof (Object[][])))
                    {
                        subscriptionMethod = method;
                        isObjectArrayDelivery = true;
                        break;
                    }
                    // Handle uniform underlying or column type array dispatch
                    if ((parameters.Length == 2) && (parameters[0].Equals(parameters[1])) && (parameters[0].IsArray)
                        && (selectClauseTypes.Length == 1))
                    {
                        Type componentType = parameters[0].GetElementType();
                        if (selectClauseTypes[0].IsAssignmentCompatible(componentType))
                        {
                            subscriptionMethod = method;
                            isTypeArrayDelivery = true;
                            break;
                        }
                    }

                    if ((parameters.Length == 0) && (selectClauseTypes.Length == 1) && (selectClauseTypes[0] == null))
                    {
                        subscriptionMethod = method;
                    }
                }
            }

            if (subscriptionMethod == null)
            {
                if (updateMethods.Count > 1)
                {
                    String parametersDesc = TypeHelper.GetParameterAsString(selectClauseTypes);
                    String message =
                        "No suitable subscriber method named 'Update' found, expecting a method that takes " +
                        selectClauseTypes.Length + " parameter of type " + parametersDesc;
                    throw new EPSubscriberException(message);
                }
                else
                {
                    Type[] parameters = updateMethods[0].GetParameterTypes();
                    String parametersDesc = TypeHelper.GetParameterAsString(selectClauseTypes);
                    if (parameters.Length != selectClauseTypes.Length)
                    {
                        if (selectClauseTypes.Length > 0)
                        {
                            String message =
                                "No suitable subscriber method named 'Update' found, expecting a method that takes " +
                                selectClauseTypes.Length + " parameter of type " + parametersDesc;
                            throw new EPSubscriberException(message);
                        }
                        else
                        {
                            String message =
                                "No suitable subscriber method named 'Update' found, expecting a method that takes no parameters";
                            throw new EPSubscriberException(message);
                        }
                    }
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        Type boxedExpressionType = selectClauseTypes[i].GetBoxedType();
                        Type boxedParameterType = parameters[i].GetBoxedType();
                        if ((boxedExpressionType != null) &&
                            (!boxedExpressionType.IsAssignmentCompatible(boxedParameterType)))
                        {
                            String message = "EPSubscriber method named 'Update' for parameter number " + (i + 1) +
                                             " is not assignable, " +
                                             "expecting type '" + selectClauseTypes[i].GetParameterAsString() +
                                             "' but found type '"
                                             + parameters[i].GetParameterAsString() + "'";
                            throw new EPSubscriberException(message);
                        }
                    }
                }
            }

            if (isMapArrayDelivery)
            {
                return new ResultDeliveryStrategyMap(statementName, subscriberObject, subscriptionMethod, selectClauseColumns);
            }
            else if (isObjectArrayDelivery)
            {
                return new ResultDeliveryStrategyObjectArr(statementName, subscriberObject, subscriptionMethod);
            }
            else if (isTypeArrayDelivery)
            {
                return new ResultDeliveryStrategyTypeArr(statementName, subscriberObject, subscriptionMethod);
            }

            // Try to find the "start", "end" and "updateRStream" methods
            MethodInfo startMethod = subscriberObject.GetType().GetMethod(
                "UpdateStart", new Type[]
                {
                    typeof (int), typeof (int)
                });
            MethodInfo endMethod = subscriberObject.GetType().GetMethod(
                "UpdateEnd");
            MethodInfo rStreamMethod = subscriberObject.GetType().GetMethod(
                "UpdateRStream", subscriptionMethod.GetParameterTypes());

            DeliveryConvertor convertor;
            if (isSingleRowMap)
            {
                convertor = new DeliveryConvertorMap(selectClauseColumns);
            }
            else if (isSingleRowObjectArr)
            {
                convertor = new DeliveryConvertorObjectArr();
            }
            else
            {
                if (checkWidening)
                {
                    convertor = DetermineWideningDeliveryConvertor(
                        selectClauseTypes, subscriptionMethod.GetParameterTypes(), subscriptionMethod);
                }
                else
                {
                    convertor = DeliveryConvertorNull.INSTANCE;
                }
            }

            return new ResultDeliveryStrategyImpl(
                statementName, subscriberObject, convertor, subscriptionMethod, startMethod, endMethod, rStreamMethod);
        }

        private static DeliveryConvertor DetermineWideningDeliveryConvertor(Type[] selectClauseTypes,
                                                                            Type[] parameterTypes,
                                                                            MethodInfo method)
        {
            bool needWidener = false;
            for (int i = 0; i < selectClauseTypes.Length; i++)
            {
                TypeWidener optionalWidener = GetWidener(i, selectClauseTypes[i], parameterTypes[i], method);
                if (optionalWidener != null)
                {
                    needWidener = true;
                    break;
                }
            }
            if (!needWidener)
            {
                return DeliveryConvertorNull.INSTANCE;
            }
            var wideners = new TypeWidener[selectClauseTypes.Length];
            for (int i = 0; i < selectClauseTypes.Length; i++)
            {
                wideners[i] = GetWidener(i, selectClauseTypes[i], parameterTypes[i], method);
            }
            return new DeliveryConvertorWidener(wideners);
        }

        private static TypeWidener GetWidener(int columnNum,
                                              Type selectClauseType,
                                              Type parameterType,
                                              MethodInfo method)
        {
            if (selectClauseType == null || parameterType == null)
            {
                return null;
            }
            if (selectClauseType == parameterType)
            {
                return null;
            }
            try
            {
                return TypeWidenerFactory.GetCheckPropertyAssignType(
                    "Select-Clause Column " + columnNum, selectClauseType, parameterType,
                    "Method Parameter " + columnNum);
            }
            catch (ExprValidationException e)
            {
                throw new EPException(
                    "Unexpected exception assigning select clause columns to subscriber method " + method + ": " +
                    e.Message, e);
            }
        }
    }
}