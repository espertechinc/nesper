///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;
using static com.espertech.esper.common.@internal.view.core.ViewFactoryForgeUtil;

namespace com.espertech.esper.common.@internal.view.groupwin
{
    /// <summary>
    ///     Factory for <seealso cref="GroupByView" /> instances.
    /// </summary>
    public class GroupByViewFactoryForge : ViewFactoryForgeBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal bool addingProperties; // when adding additional properties to output events
        internal ExprNode[] criteriaExpressions;
        internal IList<ViewFactoryForge> groupeds;
        internal bool isReclaimAged;
        internal string[] propertyNames;
        internal long reclaimFrequency;
        internal long reclaimMaxAge;
        internal IList<ExprNode> viewParameters;

        public IList<ViewFactoryForge> Groupeds {
            get => groupeds;
            set => groupeds = value;
        }

        public override string ViewName => "Group-By";

        public ExprNode[] CriteriaExpressions => criteriaExpressions;

        public IList<ExprNode> ViewParameters => viewParameters;

        public override EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            viewParameters = parameters;

            var timeAbacus = viewForgeEnv.ImportServiceCompileTime.TimeAbacus;
            var reclaimGroupAged = HintEnum.RECLAIM_GROUP_AGED.GetHint(viewForgeEnv.Annotations);

            if (reclaimGroupAged != null) {
                isReclaimAged = true;
                var hintValueMaxAge = HintEnum.RECLAIM_GROUP_AGED.GetHintAssignedValue(reclaimGroupAged);
                if (hintValueMaxAge == null) {
                    throw new ViewParameterException(
                        "Required hint value for hint '" + HintEnum.RECLAIM_GROUP_AGED + "' has not been provided");
                }

                try {
                    reclaimMaxAge = timeAbacus.DeltaForSecondsDouble(double.Parse(hintValueMaxAge));
                }
                catch (EPException) {
                    throw;
                }
                catch (Exception) {
                    throw new ViewParameterException(
                        "Required hint value for hint '" +
                        HintEnum.RECLAIM_GROUP_AGED +
                        "' value '" +
                        hintValueMaxAge +
                        "' could not be parsed as a double value");
                }

                var hintValueFrequency = HintEnum.RECLAIM_GROUP_FREQ.GetHintAssignedValue(reclaimGroupAged);
                if (hintValueFrequency == null) {
                    reclaimFrequency = reclaimMaxAge;
                }
                else {
                    try {
                        reclaimFrequency = timeAbacus.DeltaForSecondsDouble(
                            double.Parse(hintValueFrequency));
                    }
                    catch (EPException) {
                        throw;
                    }
                    catch (Exception) {
                        throw new ViewParameterException(
                            "Required hint value for hint '" +
                            HintEnum.RECLAIM_GROUP_FREQ +
                            "' value '" +
                            hintValueFrequency +
                            "' could not be parsed as a double value");
                    }
                }

                if (reclaimMaxAge < 1) {
                    Log.Warn("Reclaim max age parameter is less then 1, are your sure?");
                }

                if (Log.IsDebugEnabled) {
                    Log.Debug(
                        "Using reclaim-aged strategy for group-window age " +
                        reclaimMaxAge +
                        " frequency " +
                        reclaimFrequency);
                }
            }
        }

        public override void Attach(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv)
        {
            criteriaExpressions = ViewForgeSupport.Validate(
                ViewName,
                parentEventType,
                viewParameters,
                false,
                viewForgeEnv,
                streamNumber);

            if (criteriaExpressions.Length == 0) {
                var errorMessage =
                    ViewName + " view requires a one or more expressions provinding unique values as parameters";
                throw new ViewParameterException(errorMessage);
            }

            propertyNames = new string[criteriaExpressions.Length];
            for (var i = 0; i < criteriaExpressions.Length; i++) {
                propertyNames[i] = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(criteriaExpressions[i]);
            }

            var groupedEventType = groupeds[groupeds.Count - 1].EventType;
            eventType = DetermineEventType(groupedEventType, criteriaExpressions, streamNumber, viewForgeEnv);
            if (!Equals(eventType, groupedEventType)) {
                addingProperties = true;
            }
        }

        internal override Type TypeOfFactory()
        {
            return typeof(GroupByViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "Group";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (groupeds == null) {
                throw new IllegalStateException("Empty grouped forges");
            }

            method.Block
                .SetProperty(
                    factory,
                    "IsReclaimAged",
                    Constant(isReclaimAged))
                .SetProperty(
                    factory,
                    "ReclaimMaxAge",
                    Constant(reclaimMaxAge))
                .SetProperty(
                    factory,
                    "ReclaimFrequency",
                    Constant(reclaimFrequency))
                .SetProperty(
                    factory,
                    "PropertyNames",
                    Constant(propertyNames))
                .SetProperty(
                    factory,
                    "CriteriaEvals",
                    CodegenEvaluators(criteriaExpressions, method, GetType(), classScope))
                .SetProperty(
                    factory,
                    "CriteriaTypes",
                    Constant(ExprNodeUtilityQuery.GetExprResultTypes(criteriaExpressions)))
                .SetProperty(
                    factory,
                    "Groupeds",
                    LocalMethod(MakeViewFactories(groupeds, GetType(), method, classScope, symbols)))
                .SetProperty(
                    factory,
                    "EventType",
                    EventTypeUtility.ResolveTypeCodegen(eventType, EPStatementInitServicesConstants.REF))
                .SetProperty(factory, "IsAddingProperties", Constant(addingProperties));
        }

        public override void Accept(ViewForgeVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var forge in groupeds) {
                forge.Accept(visitor);
            }
        }

        private static EventType DetermineEventType(
            EventType groupedEventType,
            ExprNode[] criteriaExpressions,
            int streamNum,
            ViewForgeEnv viewForgeEnv)
        {
            // determine types of fields
            var fieldTypes = new Type[criteriaExpressions.Length];
            for (var i = 0; i < fieldTypes.Length; i++) {
                fieldTypes[i] = criteriaExpressions[i].Forge.EvaluationType;
            }

            // Determine the final event type that the merge view generates
            // This event type is ultimately generated by AddPropertyValueView which is added to each view branch for each
            // group key.

            // If the parent event type contains the merge fields, we use the same event type
            var parentContainsMergeKeys = true;
            var fieldNames = new string[criteriaExpressions.Length];
            for (var i = 0; i < criteriaExpressions.Length; i++) {
                var name = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(criteriaExpressions[i]);
                fieldNames[i] = name;
                try {
                    if (!groupedEventType.IsProperty(name)) {
                        parentContainsMergeKeys = false;
                    }
                }
                catch (PropertyAccessException) {
                    // expected
                    parentContainsMergeKeys = false;
                }
            }

            // If the parent view contains the fields to group by, the event type after merging stays the same
            if (parentContainsMergeKeys) {
                return groupedEventType;
            }

            // If the parent event type does not contain the fields, such as when a statistics views is
            // grouped which simply provides a map of calculated values,
            // then we need to add in the merge field as an event property thus changing event types.
            IDictionary<string, object> additionalProps = new Dictionary<string, object>();
            for (var i = 0; i < fieldNames.Length; i++) {
                additionalProps.Put(fieldNames[i], fieldTypes[i]);
            }

            var outputEventTypeName =
                viewForgeEnv.StatementCompileTimeServices.EventTypeNameGeneratorStatement.GetViewGroup(streamNum);
            var metadata = new EventTypeMetadata(
                outputEventTypeName,
                viewForgeEnv.ModuleName,
                EventTypeTypeClass.VIEWDERIVED,
                EventTypeApplicationType.WRAPPER,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            EventType eventType = WrapperEventTypeUtil.MakeWrapper(
                metadata,
                groupedEventType,
                additionalProps,
                EventBeanTypedEventFactoryCompileTime.INSTANCE,
                viewForgeEnv.BeanEventTypeFactoryProtected,
                viewForgeEnv.EventTypeCompileTimeResolver);
            viewForgeEnv.EventTypeModuleCompileTimeRegistry.NewType(eventType);
            return eventType;
        }
    }
} // end of namespace