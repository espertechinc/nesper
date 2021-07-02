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
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.view.core.ViewFactoryForgeUtil;

namespace com.espertech.esper.common.@internal.view.groupwin
{
    /// <summary>
    ///     Factory for <seealso cref="GroupByView" /> instances.
    /// </summary>
    public class GroupByViewFactoryForge : ViewFactoryForgeBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _addingProperties; // when adding additional properties to output events
        private ExprNode[] _criteriaExpressions;
        private IList<ViewFactoryForge> _groupeds;
        private bool _isReclaimAged;
        private string[] _propertyNames;
        private long _reclaimFrequency;
        private long _reclaimMaxAge;
        private IList<ExprNode> _viewParameters;
        private MultiKeyClassRef _multiKeyClassNames;

        public IList<ViewFactoryForge> Groupeds {
            get => _groupeds;
            set => _groupeds = value;
        }

        public override string ViewName => "Group-By";

        public ExprNode[] CriteriaExpressions => _criteriaExpressions;

        public IList<ExprNode> ViewParameters => _viewParameters;

        public override EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public override IList<ViewFactoryForge> InnerForges => _groupeds;

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            _viewParameters = parameters;

            var timeAbacus = viewForgeEnv.ImportServiceCompileTime.TimeAbacus;
            var reclaimGroupAged = HintEnum.RECLAIM_GROUP_AGED.GetHint(viewForgeEnv.Annotations);

            if (reclaimGroupAged != null) {
                _isReclaimAged = true;
                var hintValueMaxAge = HintEnum.RECLAIM_GROUP_AGED.GetHintAssignedValue(reclaimGroupAged);
                if (hintValueMaxAge == null) {
                    throw new ViewParameterException(
                        "Required hint value for hint '" + HintEnum.RECLAIM_GROUP_AGED + "' has not been provided");
                }

                try {
                    _reclaimMaxAge = timeAbacus.DeltaForSecondsDouble(double.Parse(hintValueMaxAge));
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
                    _reclaimFrequency = _reclaimMaxAge;
                }
                else {
                    try {
                        _reclaimFrequency = timeAbacus.DeltaForSecondsDouble(
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

                if (_reclaimMaxAge < 1) {
                    Log.Warn("Reclaim max age parameter is less then 1, are your sure?");
                }

                if (Log.IsDebugEnabled) {
                    Log.Debug(
                        "Using reclaim-aged strategy for group-window age " +
                        _reclaimMaxAge +
                        " frequency " +
                        _reclaimFrequency);
                }
            }
        }

        public override void AttachValidate(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv,
            bool grouped)
        {
            _criteriaExpressions = ViewForgeSupport.Validate(
                ViewName,
                parentEventType,
                _viewParameters,
                false,
                viewForgeEnv,
                streamNumber);

            if (_criteriaExpressions.Length == 0) {
                var errorMessage =
                    ViewName + " view requires a one or more expressions provinding unique values as parameters";
                throw new ViewParameterException(errorMessage);
            }

            _propertyNames = new string[_criteriaExpressions.Length];
            for (var i = 0; i < _criteriaExpressions.Length; i++) {
                _propertyNames[i] = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(_criteriaExpressions[i]);
            }

            var groupedEventType = _groupeds[_groupeds.Count - 1].EventType;
            eventType = DetermineEventType(groupedEventType, _criteriaExpressions, streamNumber, viewForgeEnv);
            if (!Equals(eventType, groupedEventType)) {
                _addingProperties = true;
            }
        }

        public override IList<StmtClassForgeableFactory> InitAdditionalForgeables(ViewForgeEnv viewForgeEnv)
        {
            MultiKeyPlan desc = MultiKeyPlanner.PlanMultiKey(_criteriaExpressions, false, viewForgeEnv.StatementRawInfo, viewForgeEnv.SerdeResolver);
            _multiKeyClassNames = desc.ClassRef;
            return desc.MultiKeyForgeables;
        }

        public override Type TypeOfFactory()
        {
            return typeof(GroupByViewFactory);
        }

        public override string FactoryMethod()
        {
            return "Group";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (_groupeds == null) {
                throw new IllegalStateException("Empty grouped forges");
            }

            method.Block
                .SetProperty(
                    factory,
                    "IsReclaimAged",
                    Constant(_isReclaimAged))
                .SetProperty(
                    factory,
                    "ReclaimMaxAge",
                    Constant(_reclaimMaxAge))
                .SetProperty(
                    factory,
                    "ReclaimFrequency",
                    Constant(_reclaimFrequency))
                .SetProperty(
                    factory,
                    "PropertyNames",
                    Constant(_propertyNames))
                .SetProperty(
                    factory,
                    "Groupeds",
                    LocalMethod(MakeViewFactories(_groupeds, GetType(), method, classScope, symbols)))
                .SetProperty(
                    factory,
                    "EventType",
                    EventTypeUtility.ResolveTypeCodegen(eventType, EPStatementInitServicesConstants.REF))
                .SetProperty(factory, "IsAddingProperties", Constant(_addingProperties));
            ViewMultiKeyHelper.Assign(_criteriaExpressions, _multiKeyClassNames, method, factory, symbols, classScope);
        }

        public override void Accept(ViewForgeVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var forge in _groupeds) {
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
                var type = criteriaExpressions[i].Forge.EvaluationType;
                if (type.IsNullTypeSafe()) {
                    throw new ViewParameterException("Group-window received a null-typed criteria expression");
                }

                fieldTypes[i] = type;
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
                        // for ident-nodes we also use the unresolved name as that has the unescaped property name
                        if (criteriaExpressions[i] is ExprIdentNode identNode) {
                            if (!(groupedEventType.IsProperty(identNode.UnresolvedPropertyName))) {
                                parentContainsMergeKeys = false;
                            }
                        }
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

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_GROUP;
        }
    }
} // end of namespace