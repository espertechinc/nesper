///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class EventTableIndexMetadataEntry : EventTableIndexEntryBase
    {
        private readonly ISet<string> referencedByDeployment;

        public EventTableIndexMetadataEntry(
            string optionalIndexName,
            string optionalIndexModuleName,
            bool primary,
            QueryPlanIndexItem optionalQueryPlanIndexItem,
            string explicitIndexNameIfExplicit,
            string explicitIndexModuleNameIfExplicit,
            string deploymentId)
            : base(optionalIndexName, optionalIndexModuleName)
        {
            IsPrimary = primary;
            OptionalQueryPlanIndexItem = optionalQueryPlanIndexItem;
            referencedByDeployment = primary ? null : new HashSet<string>();
            ExplicitIndexNameIfExplicit = explicitIndexNameIfExplicit;
            ExplicitIndexModuleNameIfExplicit = explicitIndexModuleNameIfExplicit;
            DeploymentId = deploymentId;
        }

        public bool IsPrimary { get; }

        public string[] ReferringDeployments => referencedByDeployment.ToArray();

        public QueryPlanIndexItem OptionalQueryPlanIndexItem { get; }

        public string ExplicitIndexNameIfExplicit { get; }

        public string ExplicitIndexModuleNameIfExplicit { get; }

        public string DeploymentId { get; }

        public void AddReferringDeployment(string deploymentId)
        {
            if (!IsPrimary) {
                referencedByDeployment.Add(deploymentId);
            }
        }

        public bool RemoveReferringStatement(string deploymentId)
        {
            if (!IsPrimary) {
                referencedByDeployment.Remove(deploymentId);
                if (referencedByDeployment.IsEmpty()) {
                    return true;
                }
            }

            return false;
        }
    }
} // end of namespace