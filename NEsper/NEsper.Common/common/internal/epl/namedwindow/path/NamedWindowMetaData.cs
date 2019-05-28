///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.@join.lookup;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.namedwindow.path
{
    public class NamedWindowMetaData
    {
        public NamedWindowMetaData(
            EventType eventType,
            string namedWindowModuleName,
            string contextName,
            string[] uniqueness,
            bool isChildBatching,
            bool isEnableIndexShare,
            EventType optionalEventTypeAs,
            bool virtualDataWindow)
        {
            EventType = eventType;
            NamedWindowModuleName = namedWindowModuleName;
            ContextName = contextName;
            Uniqueness = uniqueness;
            IsChildBatching = isChildBatching;
            IsEnableIndexShare = isEnableIndexShare;
            OptionalEventTypeAs = optionalEventTypeAs;
            IndexMetadata = new EventTableIndexMetadata();
            IsVirtualDataWindow = virtualDataWindow;
        }

        public NamedWindowMetaData(
            EventType eventType,
            string namedWindowModuleName,
            string contextName,
            string[] uniqueness,
            bool isChildBatching,
            bool isEnableIndexShare,
            EventType optionalEventTypeAs,
            bool virtualDataWindow,
            EventTableIndexMetadata indexMetadata)
        {
            EventType = eventType;
            NamedWindowModuleName = namedWindowModuleName;
            ContextName = contextName;
            Uniqueness = uniqueness;
            IsChildBatching = isChildBatching;
            IsEnableIndexShare = isEnableIndexShare;
            OptionalEventTypeAs = optionalEventTypeAs;
            IsVirtualDataWindow = virtualDataWindow;
            IndexMetadata = indexMetadata;
        }

        public EventType EventType { get; }

        public string[] Uniqueness { get; }

        public EventTableIndexMetadata IndexMetadata { get; }

        public ISet<string> UniquenessAsSet {
            get {
                if (Uniqueness == null || Uniqueness.Length == 0) {
                    return Collections.GetEmptySet<string>();
                }

                return new HashSet<string>(Uniqueness);
            }
        }

        public string ContextName { get; }

        public bool IsChildBatching { get; }

        public bool IsEnableIndexShare { get; }

        public EventType OptionalEventTypeAs { get; }

        public bool IsVirtualDataWindow { get; }

        public string NamedWindowModuleName { get; }

        public CodegenExpression Make(CodegenExpressionRef addInitSvc)
        {
            return NewInstance<NamedWindowMetaData>(
                EventTypeUtility.ResolveTypeCodegen(EventType, addInitSvc),
                Constant(NamedWindowModuleName), Constant(ContextName), Constant(Uniqueness),
                Constant(IsChildBatching), Constant(IsEnableIndexShare),
                OptionalEventTypeAs == null
                    ? ConstantNull()
                    : EventTypeUtility.ResolveTypeCodegen(OptionalEventTypeAs, addInitSvc),
                Constant(IsVirtualDataWindow));
        }

        public void AddIndex(
            string indexName,
            string indexModuleName,
            IndexMultiKey imk,
            QueryPlanIndexItem optionalQueryPlanIndexItem)
        {
            IndexMetadata.AddIndexExplicit(false, imk, indexName, indexModuleName, optionalQueryPlanIndexItem, "");
        }
    }
} // end of namespace