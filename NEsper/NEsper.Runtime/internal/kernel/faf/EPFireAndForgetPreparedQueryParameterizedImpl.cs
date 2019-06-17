///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.@internal.epl.fafquery.querymethod;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.kernel.faf
{
    public class EPFireAndForgetPreparedQueryParameterizedImpl : EPFireAndForgetPreparedQueryParameterized
    {
        private readonly Type[] types;

        public EPFireAndForgetPreparedQueryParameterizedImpl(
            AtomicBoolean serviceProviderStatus,
            FAFQueryMethodAssignerSetter fields,
            FAFQueryMethod queryMethod,
            FAFQueryInformationals queryInformationals)
        {
            ServiceProviderStatus = serviceProviderStatus;
            Fields = fields;
            QueryMethod = queryMethod;
            types = queryInformationals.SubstitutionParamsTypes;
            Names = queryInformationals.SubstitutionParamsNames;
            if (types != null && types.Length > 0) {
                UnsatisfiedParamsOneOffset = new LinkedHashSet<int>();
                for (var i = 0; i < types.Length; i++) {
                    UnsatisfiedParamsOneOffset.Add(i + 1);
                }
            }
            else {
                UnsatisfiedParamsOneOffset = new EmptySet<int>();
            }
        }

        public FAFQueryMethodAssignerSetter Fields { get; }

        public FAFQueryMethod QueryMethod { get; }

        public AtomicBoolean ServiceProviderStatus { get; }

        public ISet<int> UnsatisfiedParamsOneOffset { get; private set; }

        public IDictionary<string, int> Names { get; }

        public void SetObject(
            int parameterIndex,
            object value)
        {
            if (types == null || types.Length == 0) {
                throw new EPException("The query has no substitution parameters");
            }

            if (Names != null && !Names.IsEmpty()) {
                throw new EPException("Substitution parameter names have been provided for this query, please set the value by name");
            }

            if (parameterIndex > types.Length || parameterIndex < 1) {
                throw new EPException("Invalid substitution parameter index, expected an index between 1 and " + types.Length);
            }

            try {
                Fields.SetValue(parameterIndex, value);
                UpdateUnsatisfied(parameterIndex);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                throw HandleSetterException(Convert.ToString(parameterIndex), parameterIndex, ex);
            }
        }

        public void SetObject(
            string parameterName,
            object value)
        {
            if (types == null || types.Length == 0) {
                throw new EPException("The query has no substitution parameters");
            }

            if (Names == null || Names.IsEmpty()) {
                throw new EPException("Substitution parameter names have not been provided for this query");
            }

            if (!Names.TryGetValue(parameterName, out var index)) {
                throw new EPException("Failed to find substitution parameter named '" + parameterName + "', available parameters are " + Names.Keys);
            }

            try {
                Fields.SetValue(index, value);
                UpdateUnsatisfied(index);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                throw HandleSetterException("'" + parameterName + "'", index, ex);
            }
        }

        private EPException HandleSetterException(
            string parameterName,
            int parameterIndex,
            Exception ex)
        {
            var message = ex.Message;
            if (ex is ArgumentNullException) {
                message = "Received a null-value for a primitive type";
            }

            return new EPException(
                "Failed to set substitution parameter " + parameterName + ", expected a value of type '" + types[parameterIndex - 1].Name + "': " +
                message, ex);
        }

        private void UpdateUnsatisfied(int index)
        {
            if (UnsatisfiedParamsOneOffset.IsEmpty()) {
                return;
            }

            UnsatisfiedParamsOneOffset.Remove(index);
            if (UnsatisfiedParamsOneOffset.IsEmpty()) {
                UnsatisfiedParamsOneOffset = new EmptySet<int>();
            }
        }
    }
} // end of namespace