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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client.option;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class DeployerSubstitutionParameterHandler : StatementSubstitutionParameterContext
    {
        private readonly StatementAIFactoryProvider aiFactoryProvider;
        private readonly StatementLightweight lightweight;
        private readonly IDictionary<int, IDictionary<int, object>> provided;

        public DeployerSubstitutionParameterHandler(
            string deploymentId,
            StatementLightweight lightweight,
            IDictionary<int, IDictionary<int, object>> provided,
            Type[] types,
            IDictionary<string, int> names)
        {
            DeploymentId = deploymentId;
            this.lightweight = lightweight;
            this.provided = provided;
            SubstitutionParameterTypes = types;
            SubstitutionParameterNames = names;
            aiFactoryProvider = lightweight.StatementProvider.StatementAIFactoryProvider;
        }

        public string DeploymentId { get; }

        public string StatementName => lightweight.StatementContext.StatementName;

        public int StatementId => lightweight.StatementContext.StatementId;

        public string Epl => (string) lightweight.StatementInformationals.Properties.Get(StatementProperty.EPL);

        public Attribute[] Annotations => lightweight.StatementContext.Annotations;

        public Type[] SubstitutionParameterTypes { get; }

        public IDictionary<string, int> SubstitutionParameterNames { get; }

        public void SetObject(
            int parameterIndex,
            object value)
        {
            if (SubstitutionParameterTypes == null || SubstitutionParameterTypes.Length == 0)
            {
                throw new EPException("The statement has no substitution parameters");
            }

            if (SubstitutionParameterNames != null && !SubstitutionParameterNames.IsEmpty())
            {
                throw new EPException("Substitution parameter names have been provided for this statement, please set the value by name");
            }

            if (parameterIndex > SubstitutionParameterTypes.Length || parameterIndex < 1)
            {
                throw new EPException("Invalid substitution parameter index, expected an index between 1 and " + SubstitutionParameterTypes.Length);
            }

            try
            {
                aiFactoryProvider.SetValue(parameterIndex, value);
                AddValue(parameterIndex, value);
            }
            catch (Exception ex)
            {
                throw HandleSetterException(Convert.ToString(parameterIndex), parameterIndex, ex);
            }
        }

        public void SetObject(
            string parameterName,
            object value)
        {
            if (SubstitutionParameterTypes == null || SubstitutionParameterTypes.Length == 0)
            {
                throw new EPException("The statement has no substitution parameters");
            }

            if (SubstitutionParameterNames == null || SubstitutionParameterNames.IsEmpty())
            {
                throw new EPException("Substitution parameter names have not been provided for this statement");
            }

            if (!SubstitutionParameterNames.TryGetValue(parameterName, out var index)) {
                throw new EPException(
                    "Failed to find substitution parameter named '" +
                    parameterName +
                    "', available parameters are " +
                    SubstitutionParameterNames.Keys.RenderAny());
            }

            try
            {
                aiFactoryProvider.SetValue(index, value);
                AddValue(index, value);
            }
            catch (Exception ex)
            {
                throw HandleSetterException("'" + parameterName + "'", index, ex);
            }
        }

        private void AddValue(
            int index,
            object value)
        {
            var statementId = lightweight.StatementContext.StatementId;
            var existing = provided.Get(statementId);
            if (existing == null)
            {
                existing = new Dictionary<int, object>(8);
                provided.Put(statementId, existing);
            }

            existing.Put(index, value);
        }

        private EPException HandleSetterException(
            string parameterName,
            int parameterIndex,
            Exception ex)
        {
            var message = ex.Message;
            if (ex is ArgumentNullException)
            {
                message = "Received a null-value for a primitive type";
            }

            return new EPException(
                "Failed to set substitution parameter " + parameterName + ", expected a value of type '" +
                SubstitutionParameterTypes[parameterIndex - 1].CleanName() + "': " + message, ex);
        }
    }
} // end of namespace