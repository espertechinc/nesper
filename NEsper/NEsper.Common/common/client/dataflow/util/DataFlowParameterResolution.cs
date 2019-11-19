///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.dataflow.util
{
    /// <summary>
    /// Utility for data flow parameter resolution that considers parameter providers that are passed in.
    /// </summary>
    public class DataFlowParameterResolution
    {
        /// <summary>
        /// Resolve a number value by first looking at the parameter value provider and by using the evaluator if one was provided,
        /// returning the default value if no value was found and no evaluator was provided.
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="optionalEvaluator">evaluator</param>
        /// <param name="defaultValue">default</param>
        /// <param name="context">initialization context</param>
        /// <returns>value</returns>
        public static object ResolveNumber(
            string name,
            ExprEvaluator optionalEvaluator,
            object defaultValue,
            DataFlowOpInitializeContext context)
        {
            object resolvedFromProvider = TryParameterProvider<object>(name, context);
            if (resolvedFromProvider != null) {
                return resolvedFromProvider;
            }

            if (optionalEvaluator == null) {
                return defaultValue;
            }

            object value = (object) optionalEvaluator.Evaluate(null, true, context.AgentInstanceContext);
            if (value == null) {
                throw new EPException("Parameter '" + name + "' is null and is expected to have a value");
            }

            return value;
        }

        /// <summary>
        /// Resolve a string value by first looking at the parameter value provider and by using the evaluator if one was provided,
        /// throwing an exception if no value was provided.
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="optionalEvaluator">evaluator</param>
        /// <param name="context">initialization context</param>
        /// <returns>value</returns>
        /// <throws>EPException if no value was found</throws>
        public static string ResolveStringRequired(
            string name,
            ExprEvaluator optionalEvaluator,
            DataFlowOpInitializeContext context)
        {
            string resolvedFromProvider = TryParameterProvider<string>(name, context);
            if (resolvedFromProvider != null) {
                return resolvedFromProvider;
            }

            if (optionalEvaluator == null) {
                throw new EPException("Parameter by name '" + name + "' has no value");
            }

            string value = (string) optionalEvaluator.Evaluate(null, true, context.AgentInstanceContext);
            if (value == null) {
                throw new EPException("Parameter by name '" + name + "' has a null value");
            }

            return value;
        }

        /// <summary>
        /// Resolve a string value by first looking at the parameter value provider and by using the evaluator if one was provided
        /// or returning null if no value was found.
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="optionalEvaluator">evaluator</param>
        /// <param name="context">initialization context</param>
        /// <returns>value</returns>
        /// <throws>EPException if no value was found</throws>
        public static string ResolveStringOptional(
            string name,
            ExprEvaluator optionalEvaluator,
            DataFlowOpInitializeContext context)
        {
            string resolvedFromProvider = TryParameterProvider<string>(name, context);
            if (resolvedFromProvider != null) {
                return resolvedFromProvider;
            }

            if (optionalEvaluator == null) {
                return null;
            }

            return (string) optionalEvaluator.Evaluate(null, true, context.AgentInstanceContext);
        }

        /// <summary>
        /// Resolve a typed value by first looking at the parameter value provider and by using the evaluator if one was provided
        /// or returning the provided default value if no value was found.
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="optionalEvaluator">evaluator</param>
        /// <param name="context">initialization context</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>value</returns>
        public static T ResolveWithDefault<T>(
            string name,
            ExprEvaluator optionalEvaluator,
            T defaultValue,
            DataFlowOpInitializeContext context)
        {
            T resolvedFromProvider = TryParameterProvider<T>(name, context);
            if (resolvedFromProvider != null) {
                return resolvedFromProvider;
            }

            if (optionalEvaluator == null) {
                return defaultValue;
            }

            T result = (T) optionalEvaluator.Evaluate(null, true, context.AgentInstanceContext);
            if (result == null) {
                return defaultValue;
            }

            var clazz = typeof(T);
            if (clazz.GetBoxedType() == result.GetType().GetBoxedType()) {
                return result;
            }

            if (TypeHelper.IsSubclassOrImplementsInterface(result.GetType(), clazz)) {
                return result;
            }

            //if (TypeHelper.IsSubclassOrImplementsInterface(result.GetType().GetBoxedType(), typeof(object))) {
            if (result.GetType().GetBoxedType().IsNumeric()) {
                return (T) SimpleNumberCoercerFactory.GetCoercer(result.GetType(), clazz.GetBoxedType())
                    .CoerceBoxed(result);
            }

            return (T) result;
        }

        /// <summary>
        /// Resolve an instance from a class-name map.
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="configuration">map with key 'class' for the class name</param>
        /// <param name="context">initialization context</param>
        /// <returns>instance</returns>
        public static T ResolveOptionalInstance<T>(
            string name,
            IDictionary<string, object> configuration,
            DataFlowOpInitializeContext context)
            where T : class
        {
            T resolvedFromProvider = TryParameterProvider<T>(name, context);
            if (resolvedFromProvider != null) {
                return resolvedFromProvider;
            }

            if (configuration == null) {
                return null;
            }

            string className = (string) configuration.Get("class");
            if (className == null) {
                throw new EPException("Failed to find 'class' parameter for parameter '" + name + "'");
            }

            Type theClass;
            try {
                theClass = context.AgentInstanceContext.ImportServiceRuntime.ResolveClass(className, false);
            }
            catch (ImportException e) {
                throw new EPException("Failed to find class for parameter '" + name + "': " + e.Message, e);
            }

            try {
                return TypeHelper.Instantiate<T>(theClass);
            }
            catch (ClassInstantiationException ex) {
                throw new EPException("Failed to instantiate class for parameter '" + name + "': " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Resolve all entries in the map by first looking at the parameter value provider and by using the evaluator if one was provided
        /// or returning the provided value if no evaluator was found.
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="evals">map of properties with either evaluator or constant type</param>
        /// <param name="context">initialization context</param>
        /// <returns>value</returns>
        public static IDictionary<string, object> ResolveMap(
            string name,
            IDictionary<string, object> evals,
            DataFlowOpInitializeContext context)
        {
            if (evals == null) {
                return null;
            }

            if (evals.IsEmpty()) {
                return Collections.GetEmptyMap<string, object>();
            }

            var map = new LinkedHashMap<string, object>();
            foreach (KeyValuePair<string, object> entry in evals) {
                if (entry.Value is ExprEvaluator) {
                    try {
                        map.Put(name, ((ExprEvaluator) entry.Value).Evaluate(null, true, context.AgentInstanceContext));
                    }
                    catch (EPException) {
                        throw;
                    }
                    catch (Exception ex) {
                        throw new EPException(
                            "Failed to evaluate value for parameter '" +
                            name +
                            "' for entry key '" +
                            entry.Key +
                            "': " +
                            ex.Message,
                            ex);
                    }
                }
                else {
                    map.Put(name, entry.Value);
                }
            }

            return map;
        }

        private static T TryParameterProvider<T>(
            string name,
            DataFlowOpInitializeContext context)
        {
            if (context.AdditionalParameters != null && context.AdditionalParameters.ContainsKey(name)) {
                return (T) context.AdditionalParameters.Get(name);
            }

            if (context.ParameterProvider == null) {
                return default(T);
            }

            EPDataFlowOperatorParameterProviderContext ctx =
                new EPDataFlowOperatorParameterProviderContext(context, name);
            object value = context.ParameterProvider.Provide(ctx);
            if (value == null) {
                return default(T);
            }

            var clazz = typeof(T);
            if (TypeHelper.IsAssignmentCompatible(value.GetType(), clazz)) {
                return (T) value;
            }

            throw new EPException(
                "Parameter provider provided an unexpected object for parameter '" +
                name +
                "' of type '" +
                value.GetType().Name +
                "', expected type '" +
                clazz.Name +
                "'");
        }
    }
} // end of namespace