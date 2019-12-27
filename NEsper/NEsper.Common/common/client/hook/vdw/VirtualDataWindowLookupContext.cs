///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.client.hook.vdw
{
    /// <summary>
    /// Context passed to <see cref="VirtualDataWindow" /> upon obtaining a lookup strategy for use by 
    /// an EPL statement that queries the virtual data window. 
    /// <para/>
    /// Represents an analysis of correlation information provided in the where-clause of the querying 
    /// EPL statement (join, subquery etc.). Hash-fields are always operator-equals semantics. Btree fields 
    /// require sorted access as the operator is always a range or 
    /// Relational(&gt;, &lt;, &gt;=, &lt;=) operator. 
    /// <para/>
    /// For example, the query 
    ///     "select * from MyVirtualDataWindow, MyTrigger where prop = trigger and prop2 between trigger1 and trigger2" 
    /// indicates a single hash-field "prop" and a single btree field "prop2" with a range operator.
    ///  </summary>
    public class VirtualDataWindowLookupContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualDataWindowLookupContext"/> class.
        /// </summary>
        /// <param name="deploymentId">the deployment id.</param>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="statementId">The statement id.</param>
        /// <param name="statementAnnotations">The statement annotations.</param>
        /// <param name="fireAndForget">if set to <c>true</c> [fire and forget].</param>
        /// <param name="namedWindowName">Name of the named window.</param>
        /// <param name="hashFields">The hash fields.</param>
        /// <param name="btreeFields">The btree fields.</param>
        public VirtualDataWindowLookupContext(
            string deploymentId,
            string statementName,
            int statementId,
            Attribute[] statementAnnotations,
            bool fireAndForget,
            string namedWindowName,
            IList<VirtualDataWindowLookupFieldDesc> hashFields,
            IList<VirtualDataWindowLookupFieldDesc> btreeFields)
        {
            DeploymentId = deploymentId;
            StatementName = statementName;
            StatementId = statementId;
            StatementAnnotations = statementAnnotations;
            IsFireAndForget = fireAndForget;
            NamedWindowName = namedWindowName;
            HashFields = hashFields;
            BtreeFields = btreeFields;
        }

        /// <summary>
        /// Gets or sets the deployment id.
        /// </summary>
        /// <value>The name of the deployment id.</value>

        public string DeploymentId { get; private set; }

        /// <summary>
        /// Gets or sets the named window name.
        /// </summary>
        /// <value>The name of the named window.</value>
        public string NamedWindowName { get; private set; }

        /// <summary>Returns the list of hash field descriptors. </summary>
        /// <value>hash fields</value>
        public IList<VirtualDataWindowLookupFieldDesc> HashFields { get; private set; }

        /// <summary>Returns the list of btree field descriptors. </summary>
        /// <value>btree fields</value>
        public IList<VirtualDataWindowLookupFieldDesc> BtreeFields { get; private set; }

        /// <summary>
        /// Returns the statement name of the statement to be performing the lookup, or null for fire-and-forget statements.
        /// </summary>
        public string StatementName { get; private set; }

        /// <summary>
        /// Returns the statement id of the statement to be performing the lookup, or -1 for fire-and-forget statements.
        /// </summary>
        public int StatementId { get; private set; }

        /// <summary>
        /// Returns the statement annotations of the statement to be performing the lookup, or null for fire-and-forget statements.
        /// </summary>
        public Attribute[] StatementAnnotations { get; private set; }

        /// <summary>
        /// Returns true for fire-and-forget queries.
        /// </summary>
        public bool IsFireAndForget { get; private set; }
    }
}