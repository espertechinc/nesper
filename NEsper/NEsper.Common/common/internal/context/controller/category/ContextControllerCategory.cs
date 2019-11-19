///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.category
{
    public abstract class ContextControllerCategory : ContextControllerBase
    {
        protected internal readonly ContextControllerCategoryFactory factory;
        protected internal ContextControllerCategorySvc categorySvc;

        public ContextControllerCategory(
            ContextManagerRealization realization,
            ContextControllerCategoryFactory factory)
            : base(realization)
        {
            this.factory = factory;
        }

        public override ContextControllerFactory Factory => factory;

        public ContextControllerCategoryFactory CategoryFactory => factory;

        public override void Activate(
            IntSeqKey path,
            object[] parentPartitionKeys,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern)
        {
            var count = 0;
            ContextControllerDetailCategoryItem[] categories = factory.CategorySpec.Items;
            var subpathOrCPIds = new int[categories.Length];

            for (var i = 0; i < categories.Length; i++) {
                var result = realization.ContextPartitionInstantiate(
                    path,
                    count,
                    this,
                    null,
                    null,
                    parentPartitionKeys,
                    count);
                subpathOrCPIds[i] = result.SubpathOrCPId;
                count++;
            }

            categorySvc.MgmtCreate(path, parentPartitionKeys, subpathOrCPIds);
        }

        public override void Deactivate(
            IntSeqKey path,
            bool terminateChildContexts)
        {
            var subpathIdorCPs = categorySvc.MgmtDelete(path);
            if (subpathIdorCPs != null && terminateChildContexts) {
                for (var i = 0; i < factory.CategorySpec.Items.Length; i++) {
                    realization.ContextPartitionTerminate(path, subpathIdorCPs[i], this, null, false, null);
                }
            }
        }

        public override void VisitSelectedPartitions(
            IntSeqKey path,
            ContextPartitionSelector contextPartitionSelector,
            ContextPartitionVisitor visitor,
            ContextPartitionSelector[] selectorPerLevel)
        {
            if (contextPartitionSelector is ContextPartitionSelectorCategory) {
                var category = (ContextPartitionSelectorCategory) contextPartitionSelector;
                if (category.Labels == null || category.Labels.IsEmpty()) {
                    return;
                }

                var ids = categorySvc.MgmtGetSubpathOrCPIds(path);
                if (ids != null) {
                    var count = -1;
                    foreach (ContextControllerDetailCategoryItem categoryItem in factory.CategorySpec.Items) {
                        count++;
                        var subpathOrCPID = ids[count];
                        if (category.Labels.Contains(categoryItem.Name)) {
                            realization.ContextPartitionRecursiveVisit(
                                path,
                                subpathOrCPID,
                                this,
                                visitor,
                                selectorPerLevel);
                        }
                    }
                }

                return;
            }

            if (contextPartitionSelector is ContextPartitionSelectorFiltered) {
                var filter = (ContextPartitionSelectorFiltered) contextPartitionSelector;
                var ids = categorySvc.MgmtGetSubpathOrCPIds(path);
                if (ids != null) {
                    var count = -1;
                    foreach (ContextControllerDetailCategoryItem categoryItem in factory.CategorySpec.Items) {
                        var identifierCategory = new ContextPartitionIdentifierCategory(categoryItem.Name);
                        count++;
                        if (factory.FactoryEnv.IsLeaf) {
                            identifierCategory.ContextPartitionId = ids[count];
                        }

                        if (filter.Filter(identifierCategory)) {
                            realization.ContextPartitionRecursiveVisit(
                                path,
                                ids[count],
                                this,
                                visitor,
                                selectorPerLevel);
                        }
                    }
                }

                return;
            }

            if (contextPartitionSelector is ContextPartitionSelectorAll) {
                var ids = categorySvc.MgmtGetSubpathOrCPIds(path);
                if (ids != null) {
                    foreach (var id in ids) {
                        realization.ContextPartitionRecursiveVisit(path, id, this, visitor, selectorPerLevel);
                    }
                }

                return;
            }

            if (contextPartitionSelector is ContextPartitionSelectorById) {
                var byId = (ContextPartitionSelectorById) contextPartitionSelector;
                var ids = categorySvc.MgmtGetSubpathOrCPIds(path);
                foreach (var id in ids) {
                    if (byId.ContextPartitionIds.Contains(id)) {
                        realization.ContextPartitionRecursiveVisit(path, id, this, visitor, selectorPerLevel);
                    }
                }
            }

            throw ContextControllerSelectorUtil.GetInvalidSelector(
                new[] {typeof(ContextPartitionSelectorCategory)},
                contextPartitionSelector);
        }

        public override void Destroy()
        {
            categorySvc.Destroy();
        }
    }
} // end of namespace