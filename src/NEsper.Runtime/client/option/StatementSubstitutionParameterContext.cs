///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.runtime.client.option
{
    /// <summary>
    /// Provides the environment to <seealso cref="StatementSubstitutionParameterOption" />.
    /// </summary>
    public interface StatementSubstitutionParameterContext
    {
        /// <summary>
        /// Returns the deployment id
        /// </summary>
        /// <value>deployment id</value>
        string DeploymentId { get; }

        /// <summary>
        /// Returns the statement name
        /// </summary>
        /// <value>statement name</value>
        string StatementName { get; }

        /// <summary>
        /// Returns the statement id
        /// </summary>
        /// <value>statement id</value>
        int StatementId { get; }

        /// <summary>
        /// Returns the EPL when provided or null when not provided
        /// </summary>
        /// <value>epl</value>
        string Epl { get; }

        /// <summary>
        /// Returns the annotations
        /// </summary>
        /// <value>annotations</value>
        Attribute[] Annotations { get; }

        /// <summary>
        /// Returns the parameter types
        /// </summary>
        /// <value>types</value>
        Type[] SubstitutionParameterTypes { get; }

        /// <summary>
        /// Returns the parameter names
        /// </summary>
        /// <value>names</value>
        IDictionary<string, int> SubstitutionParameterNames { get; }

        /// <summary>
        /// Sets the value of the designated parameter using the given object.
        /// </summary>
        /// <param name="parameterIndex">the first parameter is 1, the second is 2, ...</param>
        /// <param name="value">the object containing the input parameter value</param>
        /// <throws>EPException if the substitution parameter could not be set</throws>
        void SetObject(int parameterIndex, object value);

        /// <summary>
        /// Sets the value of the designated parameter using the given object.
        /// </summary>
        /// <param name="parameterName">the name of the parameter</param>
        /// <param name="value">the object containing the input parameter value</param>
        /// <throws>EPException if the substitution parameter could not be set</throws>
        void SetObject(string parameterName, object value);
    }
} // end of namespace