///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.compile
{
    public class IndexCollectorCompileTime : IndexCollector
    {
        private readonly IDictionary<string, NamedWindowMetaData> moduleNamedWindows;
        private readonly IDictionary<string, TableMetaData> moduleTables;
        private readonly PathRegistry<string, NamedWindowMetaData> pathNamedWindows;
        private readonly PathRegistry<string, TableMetaData> pathTables;

        public IndexCollectorCompileTime(
            IDictionary<string, NamedWindowMetaData> moduleNamedWindows,
            IDictionary<string, TableMetaData> moduleTables,
            PathRegistry<string, NamedWindowMetaData> pathNamedWindows,
            PathRegistry<string, TableMetaData> pathTables)
        {
            this.moduleNamedWindows = moduleNamedWindows;
            this.moduleTables = moduleTables;
            this.pathNamedWindows = pathNamedWindows;
            this.pathTables = pathTables;
        }

        public void RegisterIndex(
            IndexCompileTimeKey indexKey,
            IndexDetail indexDetail)
        {
            EventTableIndexMetadata indexMetadata = null;
            if (indexKey.IsNamedWindow) {
                NamedWindowMetaData localNamedWindow = moduleNamedWindows.Get(indexKey.InfraName);
                if (localNamedWindow != null) {
                    indexMetadata = localNamedWindow.IndexMetadata;
                }
                else {
                    if (indexKey.Visibility == NameAccessModifier.PUBLIC) {
                        NamedWindowMetaData pathNamedWindow = pathNamedWindows.GetWithModule(
                            indexKey.InfraName,
                            indexKey.InfraModuleName);
                        if (pathNamedWindow != null) {
                            indexMetadata = pathNamedWindow.IndexMetadata;
                        }
                    }
                }

                if (indexMetadata == null) {
                    throw new EPException("Failed to find named window '" + indexKey.InfraName + "'");
                }
            }
            else {
                TableMetaData localTable = moduleTables.Get(indexKey.InfraName);
                if (localTable != null) {
                    indexMetadata = localTable.IndexMetadata;
                }
                else {
                    if (indexKey.Visibility == NameAccessModifier.PUBLIC) {
                        TableMetaData pathTable = pathTables.GetWithModule(
                            indexKey.InfraName,
                            indexKey.InfraModuleName);
                        if (pathTable != null) {
                            indexMetadata = pathTable.IndexMetadata;
                        }
                    }
                }

                if (indexMetadata == null) {
                    throw new EPException("Failed to find table '" + indexKey.InfraName + "'");
                }
            }

            try {
                indexMetadata.AddIndexExplicit(
                    false,
                    indexDetail.IndexMultiKey,
                    indexKey.IndexName,
                    indexKey.InfraModuleName,
                    indexDetail.QueryPlanIndexItem,
                    "");
            }
            catch (ExprValidationException ex) {
                throw new EPException(ex.Message, ex);
            }
        }
    }
} // end of namespace