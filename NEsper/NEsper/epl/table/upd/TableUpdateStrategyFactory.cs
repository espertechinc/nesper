///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.updatehelper;

namespace com.espertech.esper.epl.table.upd
{
    public class TableUpdateStrategyFactory
    {
        public static TableUpdateStrategy ValidateGetTableUpdateStrategy(TableMetadata tableMetadata, EventBeanUpdateHelper updateHelper, bool isOnMerge)
        {
            // determine affected indexes
            ISet<string> affectedIndexNames = null;
            bool uniqueIndexUpdated = false;
    
            foreach (var index in tableMetadata.EventTableIndexMetadataRepo.Indexes)
            {
                foreach (EventBeanUpdateItem updateItem in updateHelper.UpdateItems)
                {
                    if (updateItem.OptionalPropertyName != null)
                    {
                        bool match = DetermineUpdatesIndex(updateItem, index.Key);
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
            }
    
            // with affected indexes and with uniqueness : careful updates, may need to rollback
            if (affectedIndexNames != null && uniqueIndexUpdated)
            {
                if (isOnMerge)
                {
                    throw new ExprValidationException("On-merge statements may not update unique keys of tables");
                }
                return new TableUpdateStrategyWUniqueConstraint(updateHelper, affectedIndexNames);
            }
            // with affected indexes and without uniqueness : update indexes without unique key violation and rollback
            if (affectedIndexNames != null)
            {
                return new TableUpdateStrategyIndexNonUnique(updateHelper, affectedIndexNames);
            }
            // no affected indexes, the fasted means of updating
            return new TableUpdateStrategyNonIndex(updateHelper);
        }
    
        private static bool DetermineUpdatesIndex(EventBeanUpdateItem updateItem, IndexMultiKey key)
        {
            foreach (IndexedPropDesc prop in key.HashIndexedProps)
            {
                if (prop.IndexPropName.Equals(updateItem.OptionalPropertyName)) 
                {
                    return true;
                }
            }
            foreach (IndexedPropDesc prop in key.RangeIndexedProps)
            {
                if (prop.IndexPropName.Equals(updateItem.OptionalPropertyName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
