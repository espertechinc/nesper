///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.strategy;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public class TableImpl : TableBase
    {
        public TableImpl(TableMetaData metaData)
            : base(metaData)
        {
        }

        public override TableInstance TableInstanceNoContext => TableInstanceNoContextNoRemake;

        protected override PropertyHashedEventTableFactory SetupPrimaryKeyIndexFactory()
        {
            return new PropertyHashedEventTableFactory(
                0,
                metaData.KeyColumns,
                true,
                metaData.TableName,
                primaryKeyGetter,
                PrimaryKeyObjectArrayTransform);
        }

        public override TableInstance GetTableInstance(int agentInstanceId)
        {
            return GetTableInstanceNoRemake(agentInstanceId);
        }

        public override TableAndLockProvider GetStateProvider(
            int agentInstanceId,
            bool writesToTables)
        {
            var instance = GetTableInstance(agentInstanceId);
            var @lock = writesToTables ? instance.TableLevelRWLock.WriteLock : instance.TableLevelRWLock.ReadLock;
            if (instance is TableInstanceGrouped grouped) {
                return new TableAndLockProviderGroupedImpl(
                    new TableAndLockGrouped(@lock, grouped));
            }

            return new TableAndLockProviderUngroupedImpl(
                new TableAndLockUngrouped(@lock, (TableInstanceUngrouped)instance));
        }
    }
} // end of namespace