///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.lookup;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class EventTableIndexMetadata
    {
        public IDictionary<IndexMultiKey, EventTableIndexMetadataEntry> Indexes { get; } =
            new Dictionary<IndexMultiKey, EventTableIndexMetadataEntry>();

        public void AddIndexExplicit(
            bool isPrimary,
            IndexMultiKey indexMultiKey,
            string explicitIndexName,
            string explicitIndexModuleName,
            QueryPlanIndexItem explicitIndexDesc,
            string deploymentId)
        {
            if (GetIndexByName(explicitIndexName) != null) {
                throw new ExprValidationException("An index by name '" + explicitIndexName + "' already exists");
            }

            if (Indexes.ContainsKey(indexMultiKey)) {
                throw new ExprValidationException("An index for the same columns already exists");
            }

            var entry = new EventTableIndexMetadataEntry(
                explicitIndexName, explicitIndexModuleName, isPrimary, explicitIndexDesc, explicitIndexName,
                explicitIndexModuleName, deploymentId);
            entry.AddReferringDeployment(deploymentId);
            Indexes.Put(indexMultiKey, entry);
        }

        public void AddIndexNonExplicit(
            IndexMultiKey indexMultiKey,
            string deploymentId,
            QueryPlanIndexItem queryPlanIndexItem)
        {
            if (indexMultiKey == null) {
                throw new ArgumentException("Null index multikey");
            }

            if (Indexes.ContainsKey(indexMultiKey)) {
                return;
            }

            var entry = new EventTableIndexMetadataEntry(
                null, null, false, queryPlanIndexItem, null, null, deploymentId);
            entry.AddReferringDeployment(deploymentId);
            Indexes.Put(indexMultiKey, entry);
        }

        public void RemoveIndex(IndexMultiKey imk)
        {
            Indexes.Remove(imk);
        }

        public bool RemoveIndexReference(
            IndexMultiKey index,
            string referringDeploymentId)
        {
            if (index == null) {
                throw new ArgumentException("Null index multikey");
            }

            var entry = Indexes.Get(index);
            if (entry == null) {
                return false;
            }

            return entry.RemoveReferringStatement(referringDeploymentId);
        }

        public void AddIndexReference(
            string indexName,
            string deploymentId)
        {
            var entry = FindIndex(indexName);
            entry?.Value.AddReferringDeployment(deploymentId);
        }

        public void RemoveIndexReference(
            string indexName,
            string deploymentId)
        {
            var entry = FindIndex(indexName);
            entry?.Value.RemoveReferringStatement(deploymentId);
        }

        public void AddIndexReference(
            IndexMultiKey indexMultiKey,
            string deploymentId)
        {
            var entry = Indexes.Get(indexMultiKey);
            entry?.AddReferringDeployment(deploymentId);
        }

        public IndexMultiKey GetIndexByName(string indexName)
        {
            var entry = FindIndex(indexName);
            return entry?.Key;
        }

        public string GetIndexDeploymentId(string indexName)
        {
            var entry = FindIndex(indexName);
            return entry?.Value.DeploymentId;
        }

        public ICollection<string> GetRemoveRefIndexesDereferenced(string deploymentId)
        {
            ICollection<string> indexNamesDerrefd = null;
            foreach (var entry in Indexes) {
                bool last = entry.Value.RemoveReferringStatement(deploymentId);
                if (last) {
                    if (indexNamesDerrefd == null) {
                        indexNamesDerrefd = new ArrayDeque<string>(2);
                    }

                    indexNamesDerrefd.Add(entry.Value.OptionalIndexName);
                }
            }

            if (indexNamesDerrefd == null) {
                return Collections.GetEmptyList<string>();
            }

            foreach (var name in indexNamesDerrefd) {
                RemoveIndex(GetIndexByName(name));
            }

            return indexNamesDerrefd;
        }

        private KeyValuePair<IndexMultiKey, EventTableIndexMetadataEntry>? FindIndex(string indexName)
        {
            foreach (var entry in Indexes) {
                if (entry.Value.OptionalIndexName != null && entry.Value.OptionalIndexName.Equals(indexName)) {
                    return entry;
                }
            }

            return null;
        }

        public string[][] UniqueIndexProps {
            get {
                var uniques = new ArrayDeque<string[]>(2);
                foreach (var entry in Indexes) {
                    if (entry.Key.IsUnique) {
                        var props = new string[entry.Key.HashIndexedProps.Length];
                        for (var i = 0; i < entry.Key.HashIndexedProps.Length; i++) {
                            props[i] = entry.Key.HashIndexedProps[i].IndexPropName;
                        }

                        uniques.Add(props);
                    }
                }

                return uniques.ToArray();
            }
        }
    }
} // end of namespace