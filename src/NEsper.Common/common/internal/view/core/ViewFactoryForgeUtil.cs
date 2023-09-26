///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.serde.compiletime.eventtype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.groupwin;
using com.espertech.esper.common.@internal.view.intersect;
using com.espertech.esper.common.@internal.view.union;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.view.core
{
    public class ViewFactoryForgeUtil
    {
        public static ViewFactoryForgeDesc CreateForges(
            ViewSpec[] viewSpecDefinitions,
            ViewFactoryForgeArgs args,
            EventType parentEventType)
        {
            try {
                // Clone the view spec list to prevent parameter modification
                var viewSpecList = new List<ViewSpec>(viewSpecDefinitions);
                var additionalForgeables = new List<StmtClassForgeableFactory>(2);

                // Inspect views and add merge views if required
                // As users can specify merge views, if they are not provided they get added
                AddMergeViews(viewSpecList);

                // Instantiate factories, not making them aware of each other yet, we now have a chain
                var viewForgeEnv = new ViewForgeEnv(args);
                var forgesChain = InstantiateFactories(viewSpecList, args, viewForgeEnv);

                // Determine event type serdes that may be required
                foreach (var forge in forgesChain) {
                    if (forge is DataWindowViewForge) {
                        var serdeForgeables = SerdeEventTypeUtility.Plan(
                            parentEventType,
                            viewForgeEnv.StatementRawInfo,
                            viewForgeEnv.SerdeEventTypeRegistry,
                            viewForgeEnv.SerdeResolver,
                            viewForgeEnv.StateMgmtSettingsProvider);
                        additionalForgeables.AddAll(serdeForgeables);
                    }
                }

                // Build data window views that occur next to each other ("d d", "d d d") into a single intersection or union
                // Calls attach on the contained-views.
                var forgesChainWIntersections = BuildIntersectionsUnions(
                    forgesChain,
                    args,
                    viewForgeEnv,
                    parentEventType);

                // Verify group window use
                VerifyGroups(forgesChainWIntersections);

                // Build group window views that may contain data windows and also intersection and union
                // Calls attach on the contained-views.
                var forgesGrouped = BuildGrouped(
                    forgesChainWIntersections,
                    args,
                    viewForgeEnv,
                    parentEventType);

                var eventType = parentEventType;

                for (var i = 0; i < forgesGrouped.Count; i++) {
                    var factoryToAttach = forgesGrouped[i];
                    try {
                        factoryToAttach.Attach(eventType, viewForgeEnv);
                        eventType = factoryToAttach.EventType;
                    }
                    catch (ViewParameterException ex) {
                        throw new ViewProcessingException(ex.Message, ex);
                    }
                }

                // get multikey forges
                var multikeyForges = GetMultikeyForges(forgesGrouped, viewForgeEnv);
                additionalForgeables.AddAll(multikeyForges);

                // get state mgmt settings
                var states = GetStateMgmtSettings(
                    forgesGrouped,
                    viewForgeEnv);

                return new ViewFactoryForgeDesc(forgesGrouped, additionalForgeables, states.First, states.Second);
            }
            catch (ViewProcessingException ex) {
                throw new ExprValidationException("Failed to validate data window declaration: " + ex.Message, ex);
            }
        }

        private static IList<StmtClassForgeableFactory> GetMultikeyForges(
            IList<ViewFactoryForge> forges,
            ViewForgeEnv viewForgeEnv)
        {
            IList<StmtClassForgeableFactory> factories = new List<StmtClassForgeableFactory>(1);
            GetMultikeyForgesRecursive(forges, factories, viewForgeEnv);
            return factories;
        }

        private static void GetMultikeyForgesRecursive(
            IList<ViewFactoryForge> forges,
            IList<StmtClassForgeableFactory> multikeyForges,
            ViewForgeEnv viewForgeEnv)
        {
            foreach (var forge in forges) {
                var plan = forge.InitAdditionalForgeables(viewForgeEnv);
                multikeyForges.AddAll(plan);
                GetMultikeyForgesRecursive(forge.InnerForges, multikeyForges, viewForgeEnv);
            }
        }

        private static Pair<IList<ScheduleHandleTracked>, FabricCharge> GetStateMgmtSettings(
            IList<ViewFactoryForge> forges,
            ViewForgeEnv viewForgeEnv)
        {
            var fabricCharge = viewForgeEnv.StateMgmtSettingsProvider.NewCharge();
            IList<ScheduleHandleTracked> schedules = new List<ScheduleHandleTracked>(2);
            foreach (var forge in forges) {
                if (forge is ScheduleHandleCallbackProvider provider) {
                    schedules.Add(
                        new ScheduleHandleTracked(
                            viewForgeEnv.AttributionUngrouped,
                            provider));
                }

                try {
                    forge.AssignStateMgmtSettings(fabricCharge, viewForgeEnv, null);
                }
                catch (ViewParameterException e) {
                    throw new ViewProcessingException(e.Message, e);
                }

                GetStateMgmtSettingsGroupedRecursive(forge.InnerForges, fabricCharge, schedules, viewForgeEnv, null);
            }

            return new Pair<IList<ScheduleHandleTracked>, FabricCharge>(schedules, fabricCharge);
        }

        private static void GetStateMgmtSettingsGroupedRecursive(
            IList<ViewFactoryForge> forges,
            FabricCharge fabricCharge,
            IList<ScheduleHandleTracked> schedules,
            ViewForgeEnv viewForgeEnv,
            int[] grouping)
        {
            for (var i = 0; i < forges.Count; i++) {
                var groupingChild = grouping == null ? new int[] { i } : IntArrayUtil.Append(grouping, i);
                var child = forges[i];
                try {
                    if (child is ScheduleHandleCallbackProvider provider) {
                        schedules.Add(
                            new ScheduleHandleTracked(
                                viewForgeEnv.GetAttributionGrouped(groupingChild),
                                provider));
                    }

                    child.AssignStateMgmtSettings(fabricCharge, viewForgeEnv, groupingChild);
                }
                catch (ViewParameterException e) {
                    throw new ViewProcessingException(e.Message, e);
                }

                GetStateMgmtSettingsGroupedRecursive(
                    child.InnerForges,
                    fabricCharge,
                    schedules,
                    viewForgeEnv,
                    groupingChild);
            }
        }

        private static IList<ViewFactoryForge> BuildGrouped(
            IList<ViewFactoryForge> forgesChain,
            ViewFactoryForgeArgs args,
            ViewForgeEnv viewForgeEnv,
            EventType parentEventType)
        {
            if (forgesChain.IsEmpty()) {
                return forgesChain;
            }

            if (!(forgesChain[0] is GroupByViewFactoryForge)) { // group is always first
                return forgesChain;
            }

            var group = (GroupByViewFactoryForge)forgesChain[0];

            // find merge
            var indexMerge = -1;
            for (var i = 0; i < forgesChain.Count; i++) {
                if (forgesChain[i] is MergeViewFactoryForge) {
                    indexMerge = i;
                    break;
                }
            }

            if (indexMerge == -1 || indexMerge == 1) {
                throw new ArgumentException();
            }

            // obtain list of grouped forges
            IList<ViewFactoryForge> groupeds = new List<ViewFactoryForge>(indexMerge - 1);
            var eventType = parentEventType;

            for (var i = 1; i < indexMerge; i++) {
                var forge = forgesChain[i];
                groupeds.Add(forge);

                try {
                    forge.Attach(eventType, viewForgeEnv);
                }
                catch (ViewParameterException ex) {
                    throw new ViewProcessingException(ex.Message, ex);
                }
            }

            group.Groupeds = groupeds;

            // obtain list of remaining
            IList<ViewFactoryForge> remainder = new List<ViewFactoryForge>(1);
            remainder.Add(group);
            for (var i = indexMerge + 1; i < forgesChain.Count; i++) {
                remainder.Add(forgesChain[i]);
            }

            // the result is the remainder
            return remainder;
        }

        // Identify a sequence of data windows and replace with an intersection or union
        private static IList<ViewFactoryForge> BuildIntersectionsUnions(
            IList<ViewFactoryForge> forges,
            ViewFactoryForgeArgs args,
            ViewForgeEnv viewForgeEnv,
            EventType parentEventType)
        {
            IList<ViewFactoryForge> result = new List<ViewFactoryForge>(forges.Count);
            IList<ViewFactoryForge> dataWindows = new List<ViewFactoryForge>(2);

            foreach (var forge in forges) {
                if (forge is DataWindowViewForge) {
                    dataWindows.Add(forge);
                }
                else {
                    if (!dataWindows.IsEmpty()) {
                        if (dataWindows.Count == 1) {
                            result.AddAll(dataWindows);
                        }
                        else {
                            var intersectUnion = MakeIntersectOrUnion(
                                dataWindows,
                                args,
                                viewForgeEnv,
                                parentEventType);
                            result.Add(intersectUnion);
                        }

                        dataWindows.Clear();
                    }

                    result.Add(forge);
                }
            }

            if (!dataWindows.IsEmpty()) {
                if (dataWindows.Count == 1) {
                    result.AddAll(dataWindows);
                }
                else {
                    var intersectUnion = MakeIntersectOrUnion(
                        dataWindows,
                        args,
                        viewForgeEnv,
                        parentEventType);
                    result.Add(intersectUnion);
                }
            }

            return result;
        }

        private static ViewFactoryForge MakeIntersectOrUnion(
            IList<ViewFactoryForge> dataWindows,
            ViewFactoryForgeArgs args,
            ViewForgeEnv viewForgeEnv,
            EventType parentEventType)
        {
            foreach (var forge in dataWindows) {
                try {
                    forge.Attach(parentEventType, viewForgeEnv);
                }
                catch (ViewParameterException ex) {
                    throw new ViewProcessingException(ex.Message, ex);
                }
            }

            if (args.Options.IsRetainUnion) {
                return new UnionViewFactoryForge(new List<ViewFactoryForge>(dataWindows));
            }

            return new IntersectViewFactoryForge(new List<ViewFactoryForge>(dataWindows));
        }

        private static void VerifyGroups(IList<ViewFactoryForge> forges)
        {
            GroupByViewFactoryForge group = null;
            MergeViewFactoryForge merge = null;
            var numDataWindows = 0;
            foreach (var forge in forges) {
                if (forge is GroupByViewFactoryForge factoryForge) {
                    if (group == null) {
                        group = factoryForge;
                    }
                    else {
                        throw new ViewProcessingException("Multiple groupwin-declarations are not supported");
                    }
                }

                if (forge is MergeViewFactoryForge viewFactoryForge) {
                    if (merge == null) {
                        merge = viewFactoryForge;
                    }
                    else {
                        throw new ViewProcessingException("Multiple merge-declarations are not supported");
                    }
                }

                numDataWindows += forge is DataWindowViewForge ? 1 : 0;
            }

            if (group != null && group != forges[0]) {
                throw new ViewProcessingException("The 'groupwin' declaration must occur in the first position");
            }

            if (merge != null) {
                if (numDataWindows > 1) {
                    throw new ViewProcessingException(
                        "The 'merge' declaration cannot be used in conjunction with multiple data windows");
                }

                if (group == null) {
                    throw new ViewProcessingException(
                        "The 'merge' declaration cannot be used in without a 'group' declaration");
                }

                if (!ExprNodeUtilityCompare.DeepEquals(group.ViewParameters, merge.ViewParameters)) {
                    throw new ViewProcessingException("Mismatching parameters between 'group' and 'merge'");
                }
            }
        }

        private static IList<ViewFactoryForge> InstantiateFactories(
            IList<ViewSpec> viewSpecList,
            ViewFactoryForgeArgs args,
            ViewForgeEnv viewForgeEnv)
        {
            IList<ViewFactoryForge> forges = new List<ViewFactoryForge>();

            foreach (var spec in viewSpecList) {
                // Create the new view factory
                var viewFactoryForge = args.ViewResolutionService.Create(
                    spec.ObjectNamespace,
                    spec.ObjectName,
                    args.OptionalCreateNamedWindowName);
                forges.Add(viewFactoryForge);

                // Set view factory parameters
                try {
                    viewFactoryForge.SetViewParameters(spec.ObjectParameters, viewForgeEnv, args.StreamNum);
                }
                catch (ViewParameterException e) {
                    throw new ViewProcessingException(
                        "Error in view '" +
                        spec.ObjectName +
                        "', " +
                        e.Message,
                        e);
                }
            }

            return forges;
        }

        private static void AddMergeViews(IList<ViewSpec> specifications)
        {
            // A grouping view requires a merge view and cannot be last since it would not group sub-views
            if (specifications.Count > 0) {
                var lastView = specifications[^1];
                var viewEnum = ViewEnumExtensions.ForName(lastView.ObjectNamespace, lastView.ObjectName);
                if (viewEnum != null && viewEnum.Value.GetMergeView() != null) {
                    throw new ViewProcessingException(
                        "Invalid use of the '" +
                        lastView.ObjectName +
                        "' view, the view requires one or more child views to group, or consider using the group-by clause");
                }
            }

            var mergeViewSpecs = new LinkedList<ViewSpec>();
            var foundMerge = false;
            foreach (var spec in specifications) {
                var viewEnum = ViewEnumExtensions.ForName(spec.ObjectNamespace, spec.ObjectName);
                if (viewEnum == ViewEnum.GROUP_MERGE) {
                    foundMerge = true;
                    break;
                }
            }

            if (foundMerge) {
                return;
            }

            foreach (var spec in specifications) {
                var viewEnum = ViewEnumExtensions.ForName(spec.ObjectNamespace, spec.ObjectName);
                if (viewEnum == null) {
                    continue;
                }

                var mergeView = viewEnum.Value.GetMergeView();
                if (mergeView == null) {
                    continue;
                }

                // The merge view gets the same parameters as the view that requires the merge
                var mergeViewSpec = new ViewSpec(
                    mergeView.Value.GetNamespace(),
                    mergeView.Value.GetViewName(),
                    spec.ObjectParameters);

                // The merge views are added to the beginning of the list.
                // This enables group views to stagger ie. marketdata.group("symbol").group("feed").xxx.merge(...).merge(...)
                mergeViewSpecs.AddFirst(mergeViewSpec);
            }

            specifications.AddAll(mergeViewSpecs);
        }

        public static CodegenMethod MakeViewFactories(
            IList<ViewFactoryForge> forges,
            Type generator,
            CodegenMethodScope parent,
            CodegenClassScope classScope,
            SAIFFInitializeSymbol symbols)
        {
            var method = parent.MakeChild(typeof(ViewFactory[]), generator, classScope);
            method.Block.DeclareVar<ViewFactory[]>(
                "groupeds",
                NewArrayByLength(typeof(ViewFactory), Constant(forges.Count)));
            for (var i = 0; i < forges.Count; i++) {
                method.Block.AssignArrayElement(
                    "groupeds",
                    Constant(i),
                    forges[i].Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref("groupeds"));
            return method;
        }

        public static CodegenExpression CodegenForgesWInit(
            IList<ViewFactoryForge> forges,
            int streamNum,
            int? subqueryNum,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (forges.IsEmpty()) {
                return PublicConstValue(typeof(ViewFactory), "EMPTY_ARRAY");
            }

            var method = parent.MakeChild(typeof(ViewFactory[]), typeof(ViewFactoryForgeUtil), classScope);
            method.Block
                .DeclareVar<ViewFactory[]>("factories", NewArrayByLength(typeof(ViewFactory), Constant(forges.Count)));

            method.Block.DeclareVarNewInstance(typeof(ViewFactoryContext), "ctx")
                .SetProperty(Ref("ctx"), "StreamNum", Constant(streamNum))
                .SetProperty(Ref("ctx"), "SubqueryNumber", Constant(subqueryNum));
            for (var i = 0; i < forges.Count; i++) {
                var @ref = "factory_" + i;
                method.Block.DeclareVar<ViewFactory>(@ref, forges[i].Make(method, symbols, classScope))
                    .ExprDotMethod(Ref(@ref), "Init", Ref("ctx"), symbols.GetAddInitSvc(method))
                    .AssignArrayElement(Ref("factories"), Constant(i), Ref(@ref));
            }

            method.Block.MethodReturn(Ref("factories"));
            return LocalMethod(method);
        }

        public static bool HasDataWindows(IList<ViewFactoryForge> views)
        {
            foreach (var view in views) {
                if (view is DataWindowViewForge) {
                    return true;
                }

                if (view is GroupByViewFactoryForge grouped) {
                    return HasDataWindows(grouped.Groupeds);
                }
            }

            return false;
        }
    }
} // end of namespace