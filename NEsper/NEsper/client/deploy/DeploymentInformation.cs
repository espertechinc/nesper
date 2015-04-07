///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Available information about deployment made.
    /// </summary>
    [Serializable]
    public class DeploymentInformation
    {
        /// <summary>Ctor. </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="addedDate">date the deployment was added</param>
        /// <param name="lastUpdateDate">date of last Update to state</param>
        /// <param name="items">module statement-level details</param>
        /// <param name="state">current state</param>
        /// <param name="module">the module</param>
        public DeploymentInformation(String deploymentId, Module module, DateTime addedDate, DateTime lastUpdateDate, DeploymentInformationItem[] items, DeploymentState state)
        {
            DeploymentId = deploymentId;
            Module = module;
            LastUpdateDate = lastUpdateDate;
            AddedDate = addedDate;
            Items = items;
            State = state;
        }

        /// <summary>Returns the deployment id. </summary>
        /// <value>deployment id</value>
        public string DeploymentId { get; private set; }

        /// <summary>Returns the last Update date, i.e. date the information was last updated with new state. </summary>
        /// <value>last Update date</value>
        public DateTime LastUpdateDate { get; private set; }

        /// <summary>Returns deployment statement-level details: Note that for an newly-added undeployed modules not all statement-level information is available and therefore returns an empty array. </summary>
        /// <value>statement details or empty array for newly added deployments</value>
        public DeploymentInformationItem[] Items { get; private set; }

        /// <summary>Returns current deployment state. </summary>
        /// <value>state</value>
        public DeploymentState State { get; private set; }

        /// <summary>Returns date the deployment was added. </summary>
        /// <value>added-date</value>
        public DateTime AddedDate { get; private set; }

        /// <summary>Returns the module. </summary>
        /// <value>module</value>
        public Module Module { get; private set; }

        public override String ToString() {
            return "id '" + DeploymentId + "' " +
                   " added on " + AddedDate;
        }
    }
}
