///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Factory for creating a dispatch strategy based on the subscriber object
    /// and the columns produced by a select-clause.
    /// </summary>
    public class ResultDeliveryStrategyFactory
    {
        /// <summary>
        /// Creates a strategy implementation that indicates to subscribers
        /// the statement results based on the select-clause columns.
        /// </summary>
        /// <param name="statement">The statement.</param>
        /// <param name="subscriber">to indicate to</param>
        /// <param name="selectClauseTypes">are the types of each column in the select clause</param>
        /// <param name="selectClauseColumns">the names of each column in the select clause</param>
        /// <param name="engineURI">The engine URI.</param>
        /// <param name="engineImportService">The engine import service.</param>
        /// <returns>
        /// strategy for dispatching naturals
        /// </returns>
        /// <exception cref="EPSubscriberException">
        /// </exception>
        /// <throws>EPSubscriberException if the subscriber is invalid</throws>
        public static ResultDeliveryStrategy Create(
            EPStatement statement,
            EPSubscriber subscriber,
            Type[] selectClauseTypes,
            string[] selectClauseColumns,
            string engineURI,
            EngineImportService engineImportService)
        {
            var subscriberObject = subscriber.Subscriber;
            var subscriberMethod = subscriber.SubscriberMethod;

            if (selectClauseTypes == null)
            {
                selectClauseTypes = new Type[0];
                selectClauseColumns = new string[0];
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

            var updateMethods = subscriberType
                .GetMethods()
                .Where(method => (method.Name == subscriberMethod) && (method.IsPublic))
                .OrderBy(method => IsFirstParameterEPStatement(method) ? 0 : 1)
                .ToDictionary(method => method, GetMethodParameterTypesWithoutEPStatement);

            // none found
            if (updateMethods.Count == 0)
            {
                var message = "Subscriber object does not provide a public method by name '" + subscriberMethod + "'";
                throw new EPSubscriberException(message);
            }

            // match to parameters
            var isMapArrayDelivery = false;
            var isObjectArrayDelivery = false;
            var isSingleRowMap = false;
            var isSingleRowObjectArr = false;
            var isTypeArrayDelivery = false;

            // find an exact-matching method: no conversions and not even unboxing/boxing
            foreach (var methodNormParameterEntry in updateMethods)
            {
                var normalized = methodNormParameterEntry.Value;
                if (normalized.Length == selectClauseTypes.Length)
                {
                    var fits = true;
                    for (var i = 0; i < normalized.Length; i++)
                    {
                        if ((selectClauseTypes[i] != null) && (selectClauseTypes[i] != normalized[i]))
                        {
                            fits = false;
                            break;
                        }
                    }
                    if (fits)
                    {
                        subscriptionMethod = methodNormParameterEntry.Key;
                        break;
                    }
                }
            }

            // when not yet resolved, find an exact-matching method with boxing/unboxing
            if (subscriptionMethod == null)
            {
                foreach (var methodNormParameterEntry in updateMethods)
                {
                    var normalized = methodNormParameterEntry.Value;
                    if (normalized.Length == selectClauseTypes.Length)
                    {
                        var fits = true;
                        for (var i = 0; i < normalized.Length; i++)
                        {
                            var boxedExpressionType = selectClauseTypes[i].GetBoxedType();
                            var boxedParameterType = normalized[i].GetBoxedType();
                            if ((boxedExpressionType != null) && (boxedExpressionType != boxedParameterType))
                            {
                                fits = false;
                                break;
                            }
                        }
                        if (fits)
                        {
                            subscriptionMethod = methodNormParameterEntry.Key;
                            break;
                        }
                    }
                }
            }

            // when not yet resolved, find assignment-compatible methods that may require widening (including Integer to Long etc.)
            var checkWidening = false;
            if (subscriptionMethod == null)
            {
                foreach (var methodNormParameterEntry in updateMethods)
                {
                    var normalized = methodNormParameterEntry.Value;
                    if (normalized.Length == selectClauseTypes.Length)
                    {
                        var fits = true;
                        for (var i = 0; i < normalized.Length; i++) {
                            var parameterType = normalized[i];
                            var selectClauseType = selectClauseTypes[i];
                            var boxedExpressionType = selectClauseType.GetBoxedType();
                            var boxedParameterType = parameterType.GetBoxedType();

                            if (((selectClauseType == null) || (!selectClauseType.IsAssignmentCompatible(parameterType))) &&
                                ((boxedExpressionType == null) || (!boxedExpressionType.IsAssignmentCompatible(boxedParameterType)))) {
                                fits = false;
                                break;
                            }
                        }
                        if (fits)
                        {
                            subscriptionMethod = methodNormParameterEntry.Key;
                            checkWidening = true;
                            break;
                        }
                    }
                }
            }

            // when not yet resolved, find first-fit wildcard method
            if (subscriptionMethod == null)
            {
                foreach (var methodNormParameterEntry in updateMethods)
                {
                    var normalized = methodNormParameterEntry.Value;
                    if ((normalized.Length == 1) && (normalized[0] == typeof(DataMap)))
                    {
                        isSingleRowMap = true;
                        subscriptionMethod = methodNormParameterEntry.Key;
                        break;
                    }
                    if ((normalized.Length == 1) && (normalized[0] == typeof(object[])))
                    {
                        isSingleRowObjectArr = true;
                        subscriptionMethod = methodNormParameterEntry.Key;
                        break;
                    }

                    if ((normalized.Length == 2) && (normalized[0] == typeof(DataMap[])) &&
                        (normalized[1] == typeof(DataMap[])))
                    {
                        subscriptionMethod = methodNormParameterEntry.Key;
                        isMapArrayDelivery = true;
                        break;
                    }
                    if ((normalized.Length == 2) && (normalized[0] == typeof(object[][])) &&
                        (normalized[1] == typeof(object[][])))
                    {
                        subscriptionMethod = methodNormParameterEntry.Key;
                        isObjectArrayDelivery = true;
                        break;
                    }
                    // Handle uniform underlying or column type array dispatch
                    if ((normalized.Length == 2) && (normalized[0].Equals(normalized[1])) && (normalized[0].IsArray)
                        && (selectClauseTypes.Length == 1))
                    {
                        var componentType = normalized[0].GetElementType();
                        if (selectClauseTypes[0].IsAssignmentCompatible(componentType))
                        {
                            subscriptionMethod = methodNormParameterEntry.Key;
                            isTypeArrayDelivery = true;
                            break;
                        }
                    }

                    if ((normalized.Length == 0) && (selectClauseTypes.Length == 1) && (selectClauseTypes[0] == null))
                    {
                        subscriptionMethod = methodNormParameterEntry.Key;
                    }
                }
            }

            if (subscriptionMethod == null)
            {
                if (updateMethods.Count > 1)
                {
                    var parametersDesc = TypeHelper.GetParameterAsString(selectClauseTypes);
                    var message =
                        "No suitable subscriber method named 'Update' found, expecting a method that takes " +
                        selectClauseTypes.Length + " parameter of type " + parametersDesc;
                    throw new EPSubscriberException(message);
                }
                else
                {
                    var firstUpdateMethod = updateMethods.First();
                    var parametersNormalized = firstUpdateMethod.Value;
                    var parametersDescNormalized = TypeHelper.GetParameterAsString(selectClauseTypes);
                    if (parametersNormalized.Length != selectClauseTypes.Length)
                    {
                        if (selectClauseTypes.Length > 0)
                        {
                            var message =
                                "No suitable subscriber method named 'Update' found, expecting a method that takes " +
                                selectClauseTypes.Length + " parameter of type " + parametersDescNormalized;
                            throw new EPSubscriberException(message);
                        }
                        else
                        {
                            var message =
                                "No suitable subscriber method named 'Update' found, expecting a method that takes no parameters";
                            throw new EPSubscriberException(message);
                        }
                    }
                    for (var i = 0; i < parametersNormalized.Length; i++)
                    {
                        var boxedExpressionType = selectClauseTypes[i].GetBoxedType();
                        var boxedParameterType = parametersNormalized[i].GetBoxedType();
                        if ((boxedExpressionType != null) &&
                            (!boxedExpressionType.IsAssignmentCompatible(boxedParameterType)))
                        {
                            var message = "Subscriber method named 'Update' for parameter number " + (i + 1) +
                                             " is not assignable, " +
                                             "expecting type '" + selectClauseTypes[i].GetParameterAsString() +
                                             "' but found type '"
                                             + parametersNormalized[i].GetParameterAsString() + "'";
                            throw new EPSubscriberException(message);
                        }
                    }
                }
            }

            var parameterTypes = subscriptionMethod.GetParameterTypes();

            // Invalid if there is a another footprint for the subscription method that does not include EPStatement if present
            var firstParameterIsEPStatement = IsFirstParameterEPStatement(subscriptionMethod);
            if (isMapArrayDelivery)
            {
                return firstParameterIsEPStatement
                    ? new ResultDeliveryStrategyMapWStmt(statement, subscriberObject, subscriptionMethod, selectClauseColumns, engineImportService)
                    : new ResultDeliveryStrategyMap(statement, subscriberObject, subscriptionMethod, selectClauseColumns, engineImportService);
            }
            else if (isObjectArrayDelivery)
            {
                return firstParameterIsEPStatement
                    ? new ResultDeliveryStrategyObjectArrWStmt(statement, subscriberObject, subscriptionMethod, engineImportService)
                    : new ResultDeliveryStrategyObjectArr(statement, subscriberObject, subscriptionMethod, engineImportService);
            }
            else if (isTypeArrayDelivery)
            {
                return firstParameterIsEPStatement
                    ? new ResultDeliveryStrategyTypeArrWStmt(statement, subscriberObject, subscriptionMethod, parameterTypes[1].GetElementType(), engineImportService)
                    : new ResultDeliveryStrategyTypeArr(statement, subscriberObject, subscriptionMethod, parameterTypes[0].GetElementType(), engineImportService);
            }

            // Try to find the "start", "end" and "updateRStream" methods
            MethodInfo startMethod = null;
            MethodInfo endMethod = null;
            MethodInfo rStreamMethod = null;

            startMethod = subscriberObject.GetType().GetMethod("UpdateStart", new Type[] { typeof(EPStatement), typeof(int), typeof(int) });
            if (startMethod == null)
            {
                startMethod = subscriberObject.GetType().GetMethod("UpdateStart", new Type[] { typeof(int), typeof(int) });
            }

            endMethod = subscriberObject.GetType().GetMethod("UpdateEnd", new Type[] { typeof(EPStatement) });
            if (endMethod == null)
            {
                endMethod = subscriberObject.GetType().GetMethod("UpdateEnd");
            }

            // must be exactly the same footprint (may include EPStatement), since delivery convertor used for both
            rStreamMethod = subscriberObject.GetType().GetMethod("UpdateRStream", parameterTypes);
            if (rStreamMethod == null)
            {
                // we don't have an "updateRStream" expected, make sure there isn't one with/without EPStatement
                if (IsFirstParameterEPStatement(subscriptionMethod))
                {
                    var classes = updateMethods.Get(subscriptionMethod);
                    ValidateNonMatchUpdateRStream(subscriberObject, classes);
                }
                else
                {
                    var classes = new Type[parameterTypes.Length + 1];
                    classes[0] = typeof(EPStatement);
                    Array.Copy(parameterTypes, 0, classes, 1, parameterTypes.Length);
                    ValidateNonMatchUpdateRStream(subscriberObject, classes);
                }
            }

            DeliveryConvertor convertor;
            if (isSingleRowMap)
            {
                convertor = firstParameterIsEPStatement
                    ? (DeliveryConvertor)new DeliveryConvertorMapWStatement(selectClauseColumns, statement)
                    : (DeliveryConvertor)new DeliveryConvertorMap(selectClauseColumns);
            }
            else if (isSingleRowObjectArr)
            {
                convertor = firstParameterIsEPStatement
                    ? (DeliveryConvertor)new DeliveryConvertorObjectArrWStatement(statement)
                    : (DeliveryConvertor)DeliveryConvertorObjectArr.INSTANCE;
            }
            else
            {
                if (checkWidening)
                {
                    var normalizedParameters = updateMethods.Get(subscriptionMethod);
                    convertor = DetermineWideningDeliveryConvertor(
                        firstParameterIsEPStatement, statement, selectClauseTypes, normalizedParameters,
                        subscriptionMethod, engineURI);
                }
                else
                {
                    convertor = firstParameterIsEPStatement
                        ? (DeliveryConvertor)new DeliveryConvertorNullWStatement(statement)
                        : (DeliveryConvertor)DeliveryConvertorNull.INSTANCE;
                }
            }

            return new ResultDeliveryStrategyImpl(
                statement, subscriberObject, convertor, subscriptionMethod, startMethod, endMethod, rStreamMethod, engineImportService);
        }

        private static DeliveryConvertor DetermineWideningDeliveryConvertor(
            bool firstParameterIsEPStatement,
            EPStatement statement,
            Type[] selectClauseTypes,
            Type[] parameterTypes,
            MethodInfo method,
            string engineURI)
        {
            var needWidener = false;
            for (var i = 0; i < selectClauseTypes.Length; i++)
            {
                var optionalWidener = GetWidener(i, selectClauseTypes[i], parameterTypes[i], method, statement.Name, engineURI);
                if (optionalWidener != null)
                {
                    needWidener = true;
                    break;
                }
            }
            if (!needWidener)
            {
                return firstParameterIsEPStatement
                    ? (DeliveryConvertor)new DeliveryConvertorNullWStatement(statement)
                    : (DeliveryConvertor)DeliveryConvertorNull.INSTANCE;
            }
            var wideners = new TypeWidener[selectClauseTypes.Length];
            for (var i = 0; i < selectClauseTypes.Length; i++)
            {
                wideners[i] = GetWidener(i, selectClauseTypes[i], parameterTypes[i], method, statement.Name, engineURI);
            }
            return firstParameterIsEPStatement
                ? (DeliveryConvertor)new DeliveryConvertorWidenerWStatement(wideners, statement)
                : (DeliveryConvertor)new DeliveryConvertorWidener(wideners);
        }

        private static TypeWidener GetWidener(
            int columnNum,
            Type selectClauseType,
            Type parameterType,
            MethodInfo method,
            string statementName,
            string engineURI)
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
                    "Method Parameter " + columnNum, false, null, 
                    statementName, engineURI);
            }
            catch (ExprValidationException e)
            {
                throw new EPException(
                    "Unexpected exception assigning select clause columns to subscriber method " + method + ": " +
                    e.Message, e);
            }
        }

        private static void ValidateNonMatchUpdateRStream(object subscriber, Type[] classes)
        {
            var m = subscriber.GetType().GetMethod("UpdateRStream", classes);
            if (m != null)
            {
                throw new EPSubscriberException(
                    "Subscriber 'UpdateRStream' method footprint must match 'Update' method footprint");
            }
        }

        private static Type[] GetMethodParameterTypesWithoutEPStatement(MethodInfo method)
        {
            var parameterTypes = method.GetParameterTypes();
            if (parameterTypes.Length == 0 || parameterTypes[0] != typeof(EPStatement))
            {
                return parameterTypes;
            }
            var normalized = new Type[parameterTypes.Length - 1];
            Array.Copy(parameterTypes, 1, normalized, 0, parameterTypes.Length - 1);
            return normalized;
        }

        private static bool IsFirstParameterEPStatement(MethodInfo method)
        {
            var parameterTypes = method.GetParameterTypes();
            return parameterTypes.Length > 0 && parameterTypes[0] == typeof(EPStatement);
        }
    }
} // end of namespace
