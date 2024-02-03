///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.subscriber
{
    /// <summary>
    ///     Factory for creating a dispatch strategy based on the subscriber object
    ///     and the columns produced by a select-clause.
    /// </summary>
    public class ResultDeliveryStrategyFactory
    {
        private static readonly IComparer<MethodInfo> METHOD_PREFERENCE_COMPARATOR = new ProxyComparer<MethodInfo>(
            (
                o1,
                o2) => {
                var v1 = IsFirstParameterEPStatement(o1) ? 0 : 1;
                var v2 = IsFirstParameterEPStatement(o2) ? 0 : 1;
                return v1 > v2 ? 1 : v1 == v2 ? 0 : -1;
            });

        /// <summary>
        ///     Creates a strategy implementation that indicates to subscribers
        ///     the statement results based on the select-clause columns.
        /// </summary>
        /// <param name="subscriber">to indicate to</param>
        /// <param name="selectClauseTypes">are the types of each column in the select clause</param>
        /// <param name="selectClauseColumns">the names of each column in the select clause</param>
        /// <param name="statement">statement</param>
        /// <param name="methodName">method name</param>
        /// <param name="runtimeURI">runtime URI</param>
        /// <param name="importService">runtime imports</param>
        /// <returns>strategy for dispatching naturals</returns>
        /// <throws>ResultDeliveryStrategyInvalidException if the subscriber is invalid</throws>
        public static ResultDeliveryStrategy Create(
            EPStatement statement,
            object subscriber,
            string methodName,
            Type[] selectClauseTypes,
            string[] selectClauseColumns,
            string runtimeURI,
            ImportService importService)
        {
            if (selectClauseTypes == null) {
                selectClauseTypes = new Type[0];
                selectClauseColumns = new string[0];
            }

            if (methodName == null) {
                methodName = "Update";
            }

            // sort by presence of EPStatement as the first parameter
            var subscriberType = subscriber.GetType();
            var sorted = new List<MethodInfo>(subscriberType.GetMethods());
            sorted.Sort(METHOD_PREFERENCE_COMPARATOR);

            // Locate update methods
            MethodInfo subscriptionMethod = null;
            IDictionary<MethodInfo, Type[]> updateMethods = new LinkedHashMap<MethodInfo, Type[]>();

            foreach (var method in sorted) {
                if ((method.Name == methodName) &&
                    (method.IsPublic))
                { 
                    // Determine parameter types without EPStatement (the normalized parameters)
                    var normalizedParameters = GetMethodParameterTypesWithoutEPStatement(method);
                    updateMethods.Put(method, normalizedParameters);
                }
            }

            // none found
            if (updateMethods.Count == 0) {
                var message = "Subscriber object does not provide a public method by name 'Update'";
                throw new ResultDeliveryStrategyInvalidException(message);
            }

            // match to parameters
            var isMapArrayDelivery = false;
            var isObjectArrayDelivery = false;
            var isSingleRowMap = false;
            var isSingleRowObjectArr = false;
            var isTypeArrayDelivery = false;

            // find an exact-matching method: no conversions and not even unboxing/boxing
            foreach (var methodNormParameterEntry in updateMethods) {
                var normalized = methodNormParameterEntry.Value;
                if (normalized.Length == selectClauseTypes.Length) {
                    var fits = true;
                    for (var i = 0; i < normalized.Length; i++) {
                        if (selectClauseTypes[i] != null && selectClauseTypes[i] != normalized[i]) {
                            fits = false;
                            break;
                        }
                    }

                    if (fits) {
                        subscriptionMethod = methodNormParameterEntry.Key;
                        break;
                    }
                }
            }

            // when not yet resolved, find an exact-matching method with boxing/unboxing
            if (subscriptionMethod == null) {
                foreach (var methodNormParameterEntry in updateMethods) {
                    var normalized = methodNormParameterEntry.Value;
                    if (normalized.Length == selectClauseTypes.Length) {
                        var fits = true;
                        for (var i = 0; i < normalized.Length; i++) {
                            var boxedExpressionType = selectClauseTypes[i].GetBoxedType();
                            var boxedParameterType = normalized[i].GetBoxedType();
                            if (boxedExpressionType != null && boxedExpressionType != boxedParameterType) {
                                fits = false;
                                break;
                            }
                        }

                        if (fits) {
                            subscriptionMethod = methodNormParameterEntry.Key;
                            break;
                        }
                    }
                }
            }

            // when not yet resolved, find assignment-compatible methods that may require widening (including Integer to Long etc.)
            var checkWidening = false;
            if (subscriptionMethod == null) {
                foreach (var methodNormParameterEntry in updateMethods) {
                    var normalized = methodNormParameterEntry.Value;
                    if (normalized.Length == selectClauseTypes.Length) {
                        var fits = true;
                        for (var i = 0; i < normalized.Length; i++) {
                            var expressionType = selectClauseTypes[i];
                            var parameterType = normalized[i];

                            //var boxedExpressionType = expressionType.GetBoxedType();
                            //var boxedParameterType = parameterType.GetBoxedType();

                            if (expressionType != null && !expressionType.IsAssignmentCompatible(parameterType)) {
                                fits = false;
                                break;
                            }
                        }

                        if (fits) {
                            subscriptionMethod = methodNormParameterEntry.Key;
                            checkWidening = true;
                            break;
                        }
                    }
                }
            }

            // when not yet resolved, find first-fit wildcard method
            if (subscriptionMethod == null) {
                foreach (var methodNormParameterEntry in updateMethods) {
                    var normalized = methodNormParameterEntry.Value;
                    if (normalized.Length == 1 && normalized[0] == typeof(IDictionary<string, object>)) {
                        isSingleRowMap = true;
                        subscriptionMethod = methodNormParameterEntry.Key;
                        break;
                    }

                    if (normalized.Length == 1 && normalized[0] == typeof(object[])) {
                        isSingleRowObjectArr = true;
                        subscriptionMethod = methodNormParameterEntry.Key;
                        break;
                    }

                    if (normalized.Length == 2 && normalized[0] == typeof(IDictionary<string, object>[]) && normalized[1] == typeof(IDictionary<string, object>[])) {
                        subscriptionMethod = methodNormParameterEntry.Key;
                        isMapArrayDelivery = true;
                        break;
                    }

                    if (normalized.Length == 2 && normalized[0] == typeof(object[][]) && normalized[1] == typeof(object[][])) {
                        subscriptionMethod = methodNormParameterEntry.Key;
                        isObjectArrayDelivery = true;
                        break;
                    }

                    // Handle uniform underlying or column type array dispatch
                    if (normalized.Length == 2 && normalized[0].Equals(normalized[1]) && normalized[0].IsArray
                        && selectClauseTypes.Length == 1) {
                        Type componentType = normalized[0].GetElementType();
                        if (selectClauseTypes[0].IsAssignmentCompatible(componentType)) {
                            subscriptionMethod = methodNormParameterEntry.Key;
                            isTypeArrayDelivery = true;
                            break;
                        }
                    }

                    if (normalized.Length == 0 && selectClauseTypes.Length == 1 && selectClauseTypes[0] == null) {
                        subscriptionMethod = methodNormParameterEntry.Key;
                    }
                }
            }

            if (subscriptionMethod == null) {
                if (updateMethods.Count > 1) {
                    var parametersDesc = TypeHelper.GetParameterAsString(selectClauseTypes);
                    var message = "No suitable subscriber method named 'Update' found, expecting a method that takes " +
                                  selectClauseTypes.Length + " parameter of type " + parametersDesc;
                    throw new ResultDeliveryStrategyInvalidException(message);
                }

                KeyValuePair<MethodInfo, Type[]> firstUpdateMethod = updateMethods.First();
                var parametersNormalized = firstUpdateMethod.Value;
                var parametersDescNormalized = TypeHelper.GetParameterAsString(selectClauseTypes);
                if (parametersNormalized.Length != selectClauseTypes.Length) {
                    if (selectClauseTypes.Length > 0) {
                        var message = "No suitable subscriber method named 'Update' found, expecting a method that takes " +
                                      selectClauseTypes.Length + " parameter of type " + parametersDescNormalized;
                        throw new ResultDeliveryStrategyInvalidException(message);
                    }
                    else {
                        var message = "No suitable subscriber method named 'Update' found, expecting a method that takes no parameters";
                        throw new ResultDeliveryStrategyInvalidException(message);
                    }
                }

                for (var i = 0; i < parametersNormalized.Length; i++) {
                    var boxedExpressionType = selectClauseTypes[i].GetBoxedType();
                    var boxedParameterType = parametersNormalized[i].GetBoxedType();
                    if (boxedExpressionType != null && !boxedExpressionType.IsAssignmentCompatible(boxedParameterType)) {
                        var message = "Subscriber method named 'Update' for parameter number " + (i + 1) + " is not assignable, " +
                                      "expecting type '" + selectClauseTypes[i].GetParameterAsString() + "' but found type '"
                                      + parametersNormalized[i].GetParameterAsString() + "'";
                        throw new ResultDeliveryStrategyInvalidException(message);
                    }
                }

                throw new ResultDeliveryStrategyInvalidException("No suitable subscriber method named 'Update' found");
            }

            var parameterTypes = subscriptionMethod.GetParameterTypes();
            // Invalid if there is a another footprint for the subscription method that does not include EPStatement if present
            var firstParameterIsEPStatement = IsFirstParameterEPStatement(subscriptionMethod);
            if (isMapArrayDelivery) {
                return firstParameterIsEPStatement
                    ? new ResultDeliveryStrategyMapWStmt(statement, subscriber, subscriptionMethod, selectClauseColumns, importService)
                    : new ResultDeliveryStrategyMap(statement, subscriber, subscriptionMethod, selectClauseColumns, importService);
            }

            if (isObjectArrayDelivery) {
                return firstParameterIsEPStatement
                    ? new ResultDeliveryStrategyObjectArrWStmt(statement, subscriber, subscriptionMethod, importService)
                    : new ResultDeliveryStrategyObjectArr(statement, subscriber, subscriptionMethod, importService);
            }

            if (isTypeArrayDelivery) {
                return firstParameterIsEPStatement
                    ? new ResultDeliveryStrategyTypeArrWStmt(
                        statement, subscriber, subscriptionMethod, parameterTypes[1].GetElementType(), importService)
                    : new ResultDeliveryStrategyTypeArr(
                        statement, subscriber, subscriptionMethod, parameterTypes[0].GetElementType(), importService);
            }

            // Try to find the "Start", "End" and "UpdateRStream" methods
            MethodInfo startMethod = null;
            MethodInfo endMethod = null;
            MethodInfo rStreamMethod = null;

            startMethod = subscriberType.GetMethod("UpdateStart", new Type[] { typeof(EPStatement), typeof(int), typeof(int) });
            if (startMethod == null) {
                startMethod = subscriberType.GetMethod("UpdateStart", new Type[] {typeof(int), typeof(int)});
            }

            endMethod = subscriberType.GetMethod("UpdateEnd", new Type[] { typeof(EPStatement) });
            if (endMethod == null) {
                endMethod = subscriberType.GetMethod("UpdateEnd");
            }

            // must be exactly the same footprint (may include EPStatement), since delivery convertor used for both
            rStreamMethod = subscriberType.GetMethod("UpdateRStream", subscriptionMethod.GetParameterTypes());
            if (rStreamMethod == null)
            {
                // we don't have an "UpdateRStream" expected, make sure there isn't one with/without EPStatement
                if (IsFirstParameterEPStatement(subscriptionMethod)) {
                    var classes = updateMethods.Get(subscriptionMethod);
                    ValidateNonMatchUpdateRStream(subscriber, classes);
                }
                else {
                    var classes = new Type[parameterTypes.Length + 1];
                    classes[0] = typeof(EPStatement);
                    Array.Copy(parameterTypes, 0, classes, 1, parameterTypes.Length);
                    ValidateNonMatchUpdateRStream(subscriber, classes);
                }
            }

            DeliveryConvertor convertor;
            if (parameterTypes.Length == 0) {
                convertor = DeliveryConvertorZeroLengthParam.INSTANCE;
            }
            else if (parameterTypes.Length == 1 && parameterTypes[0] == typeof(EPStatement)) {
                convertor = new DeliveryConvertorStatementOnly(statement);
            }
            else if (isSingleRowMap) {
                convertor = firstParameterIsEPStatement
                    ? new DeliveryConvertorMapWStatement(selectClauseColumns, statement)
                    : (DeliveryConvertor) new DeliveryConvertorMap(selectClauseColumns);
            }
            else if (isSingleRowObjectArr) {
                convertor = firstParameterIsEPStatement
                    ? new DeliveryConvertorObjectArrWStatement(statement)
                    : (DeliveryConvertor) DeliveryConvertorObjectArr.INSTANCE;
            }
            else {
                if (checkWidening) {
                    var normalizedParameters = updateMethods.Get(subscriptionMethod);
                    convertor = DetermineWideningDeliveryConvertor(
                        firstParameterIsEPStatement, statement, selectClauseTypes, normalizedParameters, subscriptionMethod, runtimeURI);
                }
                else {
                    convertor = firstParameterIsEPStatement
                        ? new DeliveryConvertorNullWStatement(statement)
                        : (DeliveryConvertor) DeliveryConvertorNull.INSTANCE;
                }
            }

            return new ResultDeliveryStrategyImpl(
                statement, subscriber, convertor, subscriptionMethod, startMethod, endMethod, rStreamMethod, importService);
        }

        private static DeliveryConvertor DetermineWideningDeliveryConvertor(
            bool firstParameterIsEPStatement,
            EPStatement statement,
            Type[] selectClauseTypes,
            Type[] parameterTypes,
            MethodInfo method,
            string runtimeURI)
        {
            var needWidener = false;
            for (var i = 0; i < selectClauseTypes.Length; i++) {
                var optionalWidener = GetWidener(i, selectClauseTypes[i], parameterTypes[i], method, statement.Name);
                if (optionalWidener != null) {
                    needWidener = true;
                    break;
                }
            }

            if (!needWidener) {
                return firstParameterIsEPStatement
                    ? new DeliveryConvertorNullWStatement(statement)
                    : (DeliveryConvertor) DeliveryConvertorNull.INSTANCE;
            }

            var wideners = new TypeWidenerSPI[selectClauseTypes.Length];
            for (var i = 0; i < selectClauseTypes.Length; i++) {
                wideners[i] = GetWidener(i, selectClauseTypes[i], parameterTypes[i], method, statement.Name);
            }

            return firstParameterIsEPStatement
                ? new DeliveryConvertorWidenerWStatement(wideners, statement)
                : (DeliveryConvertor) new DeliveryConvertorWidener(wideners);
        }

        private static TypeWidenerSPI GetWidener(
            int columnNum,
            Type selectClauseType,
            Type parameterType,
            MethodInfo method,
            string statementName)
        {
            if (selectClauseType == null || parameterType == null) {
                return null;
            }

            if (selectClauseType == parameterType) {
                return null;
            }

            try {
                return TypeWidenerFactory.GetCheckPropertyAssignType(
                    "Select-Clause Column " + columnNum, selectClauseType, parameterType, "Method Parameter " + columnNum, false, null,
                    statementName);
            }
            catch (TypeWidenerException e) {
                throw new EPException("Unexpected exception assigning select clause columns to subscriber method " + method + ": " + e.Message, e);
            }
        }

        private static void ValidateNonMatchUpdateRStream(
            object subscriber,
            Type[] classes)
        {
            var m = subscriber.GetType().GetMethod("UpdateRStream", classes);
            if (m != null) {
                throw new ResultDeliveryStrategyInvalidException(
                    "Subscriber 'UpdateRStream' method footprint must match 'Update' method footprint");
            }
        }

        private static Type[] GetMethodParameterTypesWithoutEPStatement(MethodInfo method)
        {
            var parameterTypes = method.GetParameterTypes();
            if (parameterTypes.Length == 0 || parameterTypes[0] != typeof(EPStatement)) {
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