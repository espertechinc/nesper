///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.lookup;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.updatehelper;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.update
{
    public class TableUpdateStrategyFactory
    {
        public static void ValidateNewUniqueIndex(string[] tableUpdatedProperties, IndexedPropDesc[] hashIndexedProps)
        {
            foreach (var prop in hashIndexedProps)
            {
                foreach (var col in tableUpdatedProperties)
                {
                    if (prop.IndexPropName.Equals(col))
                    {
                        throw new EPException(
                            "Create-index adds a unique key on columns that are updated by one or more on-merge statements");
                    }
                }
            }
        }

        public static void ValidateTableUpdateOnMerge(TableMetaData tableMetadata, string[] updatedProperties)
        {
            var desc = GetAffectedIndexes(tableMetadata, updatedProperties);
            if (desc.AffectedIndexNames != null && desc.IsUniqueIndexUpdated)
            {
                throw new ExprValidationException("On-merge statements may not update unique keys of tables");
            }
        }

        public static TableUpdateStrategy ValidateGetTableUpdateStrategy(
            TableMetaData tableMetadata, EventBeanUpdateHelperNoCopy updateHelper, bool isOnMerge)
        {
            var desc = GetAffectedIndexes(tableMetadata, updateHelper.UpdatedProperties);

            // with affected indexes and with uniqueness : careful updates, may need to rollback
            if (desc.AffectedIndexNames != null && desc.IsUniqueIndexUpdated)
            {
                if (isOnMerge)
                {
                    throw new ExprValidationException("On-merge statements may not update unique keys of tables");
                }

                return new TableUpdateStrategyWUniqueConstraint(updateHelper, desc.AffectedIndexNames);
            }

            // with affected indexes and without uniqueness : update indexes without unique key violation and rollback
            if (desc.AffectedIndexNames != null)
            {
                return new TableUpdateStrategyIndexNonUnique(updateHelper, desc.AffectedIndexNames);
            }

            // no affected indexes, the fasted means of updating
            return new TableUpdateStrategyNonIndex(updateHelper);
        }

        private static IndexUpdateDesc GetAffectedIndexes(TableMetaData tableMetadata, string[] updatedProperties)
        {
            ISet<string> affectedIndexNames = null;
            var uniqueIndexUpdated = false;

            foreach (KeyValuePair<IndexMultiKey, EventTableIndexMetadataEntry> index in tableMetadata.IndexMetadata.Indexes)
            {
                foreach (var updatedProperty in updatedProperties)
                {
                    var match = DetermineUpdatesIndex(updatedProperty, index.Key);
                    if (match)
                    {
                        if (affectedIndexNames == null)
                        {
                            affectedIndexNames = new LinkedHashSet<string>();
                        }

                        affectedIndexNames.Add(index.Value.OptionalIndexName);
                        uniqueIndexUpdated |= index.Key.IsUnique;
                    }
                }
            }

            return new IndexUpdateDesc(affectedIndexNames, uniqueIndexUpdated);
        }

        private static bool DetermineUpdatesIndex(string updatedProperty, IndexMultiKey key)
        {
            foreach (var prop in key.HashIndexedProps)
            {
                if (prop.IndexPropName.Equals(updatedProperty))
                {
                    return true;
                }
            }

            foreach (var prop in key.RangeIndexedProps)
            {
                if (prop.IndexPropName.Equals(updatedProperty))
                {
                    return true;
                }
            }

            return false;
        }

        private class IndexUpdateDesc
        {
            public IndexUpdateDesc(ISet<string> affectedIndexNames, bool uniqueIndexUpdated)
            {
                AffectedIndexNames = affectedIndexNames;
                IsUniqueIndexUpdated = uniqueIndexUpdated;
            }

            public ISet<string> AffectedIndexNames { get; }

            public bool IsUniqueIndexUpdated { get; }
        }
    }
} // end of namespace