///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
    public class TableCompileTimeResolverImpl : TableCompileTimeResolver
    {
        private readonly string moduleName;
        private readonly ICollection<string> moduleUses;
        private readonly TableCompileTimeRegistry compileTimeRegistry;
        private readonly PathRegistry<string, TableMetaData> pathTables;
        private readonly ModuleDependenciesCompileTime moduleDependencies;
        private readonly bool isFireAndForget;

        public TableCompileTimeResolverImpl(
            string moduleName,
            ICollection<string> moduleUses,
            TableCompileTimeRegistry compileTimeRegistry,
            PathRegistry<string, TableMetaData> pathTables,
            ModuleDependenciesCompileTime moduleDependencies,
            bool isFireAndForget)
        {
            this.moduleName = moduleName;
            this.moduleUses = moduleUses;
            this.compileTimeRegistry = compileTimeRegistry;
            this.pathTables = pathTables;
            this.moduleDependencies = moduleDependencies;
            this.isFireAndForget = isFireAndForget;
        }

        public TableMetaData ResolveTableFromEventType(EventType containedType)
        {
            if (containedType != null && containedType.Metadata.TypeClass == EventTypeTypeClass.TABLE_INTERNAL) {
                var tableName = EventTypeNameUtil.GetTableNameFromInternalTypeName(containedType.Name);
                return Resolve(tableName);
            }

            return null;
        }

        public TableMetaData Resolve(string tableName)
        {
            var metaData = compileTimeRegistry.GetTable(tableName);
            if (metaData != null) {
                return metaData;
            }

            try {
                var data = pathTables.GetAnyModuleExpectSingle(tableName, moduleUses);
                if (data != null) {
                    if (!isFireAndForget &&
                        !NameAccessModifierExtensions.Visible(
                            data.First.TableVisibility,
                            data.First.TableModuleName,
                            moduleName)) {
                        return null;
                    }

                    moduleDependencies.AddPathTable(tableName, data.Second);
                    return data.First;
                }
            }
            catch (PathException e) {
                throw CompileTimeResolverUtil.MakePathAmbiguous(PathRegistryObjectType.TABLE, tableName, e);
            }

            return null;
        }
    }
} // end of namespace