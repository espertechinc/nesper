///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.avro;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    ///     Processor for select-clause expressions that handles a list of selection items represented by
    ///     expression nodes. Computes results based on matching events.
    /// </summary>
    public class SelectExprProcessorHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Attribute[] _annotations;

        private readonly ICollection<int> _assignedTypeNumberStack;
        private readonly ConfigurationInformation _configuration;
        private readonly EngineImportService _engineImportService;
        private readonly EventAdapterService _eventAdapterService;
        private readonly GroupByRollupInfo _groupByRollupInfo;
        private readonly InsertIntoDesc _insertIntoDesc;
        private readonly bool _isUsingWildcard;
        private readonly NamedWindowMgmtService _namedWindowMgmtService;
        private readonly SelectExprEventTypeRegistry _selectExprEventTypeRegistry;
        private readonly IList<SelectExprStreamDesc> _selectedStreams;
        private readonly IList<SelectClauseExprCompiledSpec> _selectionList;
        private readonly int _statementId;
        private readonly string _statementName;
        private readonly TableService _tableService;
        private readonly StreamTypeService _typeService;
        private readonly ValueAddEventService _valueAddEventService;
        private EventType _optionalInsertIntoOverrideType;

        public SelectExprProcessorHelper(
            ICollection<int> assignedTypeNumberStack,
            IList<SelectClauseExprCompiledSpec> selectionList,
            IList<SelectExprStreamDesc> selectedStreams,
            InsertIntoDesc insertIntoDesc,
            EventType optionalInsertIntoOverrideType,
            bool isUsingWildcard,
            StreamTypeService typeService,
            EventAdapterService eventAdapterService,
            ValueAddEventService valueAddEventService,
            SelectExprEventTypeRegistry selectExprEventTypeRegistry,
            EngineImportService engineImportService,
            int statementId,
            string statementName,
            Attribute[] annotations,
            ConfigurationInformation configuration,
            NamedWindowMgmtService namedWindowMgmtService,
            TableService tableService,
            GroupByRollupInfo groupByRollupInfo)
        {
            _assignedTypeNumberStack = assignedTypeNumberStack;
            _selectionList = selectionList;
            _selectedStreams = selectedStreams;
            _insertIntoDesc = insertIntoDesc;
            _optionalInsertIntoOverrideType = optionalInsertIntoOverrideType;
            _eventAdapterService = eventAdapterService;
            _isUsingWildcard = isUsingWildcard;
            _typeService = typeService;
            _valueAddEventService = valueAddEventService;
            _selectExprEventTypeRegistry = selectExprEventTypeRegistry;
            _engineImportService = engineImportService;
            _statementId = statementId;
            _statementName = statementName;
            _annotations = annotations;
            _configuration = configuration;
            _namedWindowMgmtService = namedWindowMgmtService;
            _tableService = tableService;
            _groupByRollupInfo = groupByRollupInfo;
        }

        private static EPType[] DetermineInsertedEventTypeTargets(
            EventType targetType,
            IList<SelectClauseExprCompiledSpec> selectionList)
        {
            var targets = new EPType[selectionList.Count];
            if (targetType == null)
            {
                return targets;
            }

            for (int i = 0; i < selectionList.Count; i++)
            {
                SelectClauseExprCompiledSpec expr = selectionList[i];
                if (expr.ProvidedName == null)
                {
                    continue;
                }

                EventPropertyDescriptor desc = targetType.GetPropertyDescriptor(expr.ProvidedName);
                if (desc == null)
                {
                    continue;
                }

                if (!desc.IsFragment)
                {
                    continue;
                }

                FragmentEventType fragmentEventType = targetType.GetFragmentType(expr.ProvidedName);
                if (fragmentEventType == null)
                {
                    continue;
                }

                if (fragmentEventType.IsIndexed)
                {
                    targets[i] = EPTypeHelper.CollectionOfEvents(fragmentEventType.FragmentType);
                }
                else
                {
                    targets[i] = EPTypeHelper.SingleEvent(fragmentEventType.FragmentType);
                }
            }

            return targets;
        }

        // Determine which properties provided by the Map must be downcast from EventBean to Object
        private static ISet<string> GetEventBeanToObjectProps(
            IDictionary<string, Object> selPropertyTypes,
            EventType resultEventType)
        {
            if (!(resultEventType is BaseNestableEventType))
            {
                return Collections.GetEmptySet<string>();
            }
            var mapEventType = (BaseNestableEventType) resultEventType;
            ISet<string> props = null;
            foreach (var entry in selPropertyTypes)
            {
                if (entry.Value is BeanEventType && mapEventType.Types.Get(entry.Key) is Type)
                {
                    if (props == null)
                    {
                        props = new HashSet<string>();
                    }
                    props.Add(entry.Key);
                }
            }
            if (props == null)
            {
                return Collections.GetEmptySet<string>();
            }
            return props;
        }

        private static void VerifyInsertInto(
            InsertIntoDesc insertIntoDesc,
            IList<SelectClauseExprCompiledSpec> selectionList)
        {
            // Verify all column names are unique
            var names = new HashSet<string>();
            foreach (string element in insertIntoDesc.ColumnNames)
            {
                if (names.Contains(element))
                {
                    throw new ExprValidationException(
                        "Property name '" + element + "' appears more then once in insert-into clause");
                }
                names.Add(element);
            }

            // Verify number of columns matches the select clause
            if ((!insertIntoDesc.ColumnNames.IsEmpty()) &&
                (insertIntoDesc.ColumnNames.Count != selectionList.Count))
            {
                throw new ExprValidationException(
                    "Number of supplied values in the select or values clause does not match insert-into clause");
            }
        }

        public SelectExprProcessor GetEvaluator()
        {
            // Get the named and un-named stream selectors (i.e. select s0.* from S0 as s0), if any
            var namedStreams = new List<SelectClauseStreamCompiledSpec>();
            var unnamedStreams = new List<SelectExprStreamDesc>();
            foreach (SelectExprStreamDesc spec in _selectedStreams)
            {
                // handle special "Transpose(...)" function
                if ((spec.StreamSelected != null && spec.StreamSelected.OptionalName == null)
                    ||
                    (spec.ExpressionSelectedAsStream != null))
                {
                    unnamedStreams.Add(spec);
                }
                else
                {
                    namedStreams.Add(spec.StreamSelected);
                    if (spec.StreamSelected.IsProperty)
                    {
                        throw new ExprValidationException(
                            "The property wildcard syntax must be used without column name");
                    }
                }
            }

            // Error if there are more then one un-named streams (i.e. select s0.*, s1.* from S0 as s0, S1 as s1)
            // Thus there is only 1 unnamed stream selector maximum.
            if (unnamedStreams.Count > 1)
            {
                throw new ExprValidationException(
                    "A column name must be supplied for all but one stream if multiple streams are selected via the stream.* notation");
            }

            if (_selectedStreams.IsEmpty() && _selectionList.IsEmpty() && !_isUsingWildcard)
            {
                throw new ArgumentException("EmptyFalse selection list not supported");
            }

            foreach (SelectClauseExprCompiledSpec entry in _selectionList)
            {
                if (entry.AssignedName == null)
                {
                    throw new ArgumentException("Expected name for each expression has not been supplied");
                }
            }

            // Verify insert into clause
            if (_insertIntoDesc != null)
            {
                VerifyInsertInto(_insertIntoDesc, _selectionList);
            }

            // Build a subordinate wildcard processor for joins
            SelectExprProcessor joinWildcardProcessor = null;
            if (_typeService.StreamNames.Length > 1 && _isUsingWildcard)
            {
                joinWildcardProcessor = SelectExprJoinWildcardProcessorFactory.Create(
                    _assignedTypeNumberStack, _statementId, _statementName, _typeService.StreamNames,
                    _typeService.EventTypes, _eventAdapterService, null, _selectExprEventTypeRegistry,
                    _engineImportService, _annotations, _configuration, _tableService, _typeService.EngineURIQualifier);
            }

            // Resolve underlying event type in the case of wildcard select
            EventType eventType = null;
            bool singleStreamWrapper = false;
            if (_isUsingWildcard)
            {
                if (joinWildcardProcessor != null)
                {
                    eventType = joinWildcardProcessor.ResultEventType;
                }
                else
                {
                    eventType = _typeService.EventTypes[0];
                    if (eventType is WrapperEventType)
                    {
                        singleStreamWrapper = true;
                    }
                }
            }

            // Find if there is any fragments selected
            EventType insertIntoTargetType = null;
            if (_insertIntoDesc != null)
            {
                if (_optionalInsertIntoOverrideType != null)
                {
                    insertIntoTargetType = _optionalInsertIntoOverrideType;
                }
                else
                {
                    insertIntoTargetType = _eventAdapterService.GetEventTypeByName(_insertIntoDesc.EventTypeName);
                    TableMetadata tableMetadata = _tableService.GetTableMetadata(_insertIntoDesc.EventTypeName);
                    if (tableMetadata != null)
                    {
                        insertIntoTargetType = tableMetadata.InternalEventType;
                        _optionalInsertIntoOverrideType = insertIntoTargetType;
                    }
                }
            }

            // Obtain insert-into per-column type information, when available
            EPType[] insertIntoTargetsPerCol = DetermineInsertedEventTypeTargets(insertIntoTargetType, _selectionList);

            // Get expression nodes
            var exprEvaluators = new ExprEvaluator[_selectionList.Count];
            var exprNodes = new ExprNode[_selectionList.Count];
            var expressionReturnTypes = new Object[_selectionList.Count];
            for (int i = 0; i < _selectionList.Count; i++)
            {
                SelectClauseExprCompiledSpec spec = _selectionList[i];
                ExprNode expr = spec.SelectExpression;
                ExprEvaluator evaluator = expr.ExprEvaluator;
                exprNodes[i] = expr;

                // if there is insert-into specification, use that
                if (_insertIntoDesc != null)
                {
                    // handle insert-into, with well-defined target event-typed column, and enumeration
                    TypeAndFunctionPair pairX = HandleInsertIntoEnumeration(
                        spec.ProvidedName, insertIntoTargetsPerCol[i], evaluator, _engineImportService);
                    if (pairX != null)
                    {
                        expressionReturnTypes[i] = pairX.Type;
                        exprEvaluators[i] = pairX.Function;
                        continue;
                    }

                    // handle insert-into with well-defined target event-typed column, and typable expression
                    pairX = HandleInsertIntoTypableExpression(
                        insertIntoTargetsPerCol[i], evaluator, _engineImportService);
                    if (pairX != null)
                    {
                        expressionReturnTypes[i] = pairX.Type;
                        exprEvaluators[i] = pairX.Function;
                        continue;
                    }
                }

                // handle @eventbean annotation, i.e. well-defined type through enumeration
                TypeAndFunctionPair pair = HandleAtEventbeanEnumeration(spec.IsEvents, evaluator);
                if (pair != null)
                {
                    expressionReturnTypes[i] = pair.Type;
                    exprEvaluators[i] = pair.Function;
                    continue;
                }

                // handle typeable return, i.e. typable multi-column return without provided target type
                pair = HandleTypableExpression(evaluator, i);
                if (pair != null)
                {
                    expressionReturnTypes[i] = pair.Type;
                    exprEvaluators[i] = pair.Function;
                    continue;
                }

                // handle select-clause expressions that match group-by expressions with rollup and therefore should be boxed types as rollup can produce a null value
                if (_groupByRollupInfo != null && _groupByRollupInfo.RollupDesc != null)
                {
                    Type returnType = evaluator.ReturnType;
                    Type returnTypeBoxed = returnType.GetBoxedType();
                    if (returnType != returnTypeBoxed && IsGroupByRollupNullableExpression(expr, _groupByRollupInfo))
                    {
                        exprEvaluators[i] = evaluator;
                        expressionReturnTypes[i] = returnTypeBoxed;
                        continue;
                    }
                }

                // assign normal expected return type
                exprEvaluators[i] = evaluator;
                expressionReturnTypes[i] = exprEvaluators[i].ReturnType;
            }

            // Get column names
            string[] columnNames;
            string[] columnNamesAsProvided;
            if ((_insertIntoDesc != null) && (!_insertIntoDesc.ColumnNames.IsEmpty()))
            {
                columnNames = _insertIntoDesc.ColumnNames.ToArray();
                columnNamesAsProvided = columnNames;
            }
            else if (!_selectedStreams.IsEmpty())
            {
                // handle stream selection column names
                int numStreamColumnsJoin = 0;
                if (_isUsingWildcard && _typeService.EventTypes.Length > 1)
                {
                    numStreamColumnsJoin = _typeService.EventTypes.Length;
                }
                columnNames = new string[_selectionList.Count + namedStreams.Count + numStreamColumnsJoin];
                columnNamesAsProvided = new string[columnNames.Length];
                int countX = 0;
                foreach (SelectClauseExprCompiledSpec aSelectionList in _selectionList)
                {
                    columnNames[countX] = aSelectionList.AssignedName;
                    columnNamesAsProvided[countX] = aSelectionList.ProvidedName;
                    countX++;
                }
                foreach (SelectClauseStreamCompiledSpec aSelectionList in namedStreams)
                {
                    columnNames[countX] = aSelectionList.OptionalName;
                    columnNamesAsProvided[countX] = aSelectionList.OptionalName;
                    countX++;
                }
                // for wildcard joins, add the streams themselves
                if (_isUsingWildcard && _typeService.EventTypes.Length > 1)
                {
                    foreach (string streamName in _typeService.StreamNames)
                    {
                        columnNames[countX] = streamName;
                        columnNamesAsProvided[countX] = streamName;
                        countX++;
                    }
                }
            }
            else
            {
                // handle regular column names
                columnNames = new string[_selectionList.Count];
                columnNamesAsProvided = new string[_selectionList.Count];
                for (int i = 0; i < _selectionList.Count; i++)
                {
                    columnNames[i] = _selectionList[i].AssignedName;
                    columnNamesAsProvided[i] = _selectionList[i].ProvidedName;
                }
            }

            // Find if there is any fragment event types:
            // This is a special case for fragments: select a, b from pattern [a=A -> b=B]
            // We'd like to maintain 'A' and 'B' EventType in the Map type, and 'a' and 'b' EventBeans in the event bean
            for (int i = 0; i < _selectionList.Count; i++)
            {
                if (!(exprNodes[i] is ExprIdentNode))
                {
                    continue;
                }

                var identNode = (ExprIdentNode) exprNodes[i];
                string propertyName = identNode.ResolvedPropertyName;
                int streamNum = identNode.StreamId;

                EventType eventTypeStream = _typeService.EventTypes[streamNum];
                if (eventTypeStream is NativeEventType)
                {
                    continue; // we do not transpose the native type for performance reasons
                }

                FragmentEventType fragmentType = eventTypeStream.GetFragmentType(propertyName);
                if ((fragmentType == null) || (fragmentType.IsNative))
                {
                    continue; // we also ignore native classes as fragments for performance reasons
                }

                // may need to unwrap the fragment if the target type has this underlying type
                FragmentEventType targetFragment = null;
                if (insertIntoTargetType != null)
                {
                    targetFragment = insertIntoTargetType.GetFragmentType(columnNames[i]);
                }
                if ((insertIntoTargetType != null) &&
#pragma warning disable CS0253 // Possible unintended reference comparison; right hand side needs cast
                    (fragmentType.FragmentType.UnderlyingType == expressionReturnTypes[i]) &&
#pragma warning restore CS0253 // Possible unintended reference comparison; right hand side needs cast
                    ((targetFragment == null) || (targetFragment != null && targetFragment.IsNative)))
                {
                    EventPropertyGetter getter = eventTypeStream.GetGetter(propertyName);
                    Type returnType = eventTypeStream.GetPropertyType(propertyName);
                    exprEvaluators[i] = new SelectExprProcessorEvalByGetter(streamNum, getter, returnType);
                }
                else if ((insertIntoTargetType != null) && expressionReturnTypes[i] is Type &&
                            (fragmentType.FragmentType.UnderlyingType == ((Type) expressionReturnTypes[i]).GetElementType()) &&
                            ((targetFragment == null) || (targetFragment != null && targetFragment.IsNative)))
                {
                    // same for arrays: may need to unwrap the fragment if the target type has this underlying type
                    EventPropertyGetter getter = eventTypeStream.GetGetter(propertyName);
                    Type returnType = TypeHelper.GetArrayType(eventTypeStream.GetPropertyType(propertyName));
                    exprEvaluators[i] = new SelectExprProcessorEvalByGetter(streamNum, getter, returnType);
                }
                else
                {
                    EventPropertyGetter getter = eventTypeStream.GetGetter(propertyName);
                    FragmentEventType fragType = eventTypeStream.GetFragmentType(propertyName);
                    Type undType = fragType.FragmentType.UnderlyingType;
                    Type returnType = fragType.IsIndexed ? TypeHelper.GetArrayType(undType) : undType;
                    exprEvaluators[i] = new SelectExprProcessorEvalByGetterFragment(streamNum, getter, returnType);
                    if (!fragmentType.IsIndexed)
                    {
                        expressionReturnTypes[i] = fragmentType.FragmentType;
                    }
                    else
                    {
                        expressionReturnTypes[i] = new EventType[]
                        {
                            fragmentType.FragmentType
                        };
                    }
                }
            }

            // Find if there is any stream expression (ExprStreamNode) :
            // This is a special case for stream selection: select a, b from A as a, B as b
            // We'd like to maintain 'A' and 'B' EventType in the Map type, and 'a' and 'b' EventBeans in the event bean
            for (int i = 0; i < _selectionList.Count; i++)
            {
                Pair<ExprEvaluator, object> pair = HandleUnderlyingStreamInsert(
                    exprEvaluators[i], _namedWindowMgmtService, _eventAdapterService);
                if (pair != null)
                {
                    exprEvaluators[i] = pair.First;
                    expressionReturnTypes[i] = pair.Second;
                }
            }

            // Build event type that reflects all selected properties
            var selPropertyTypes = new LinkedHashMap<string, Object>();
            int count = 0;
            for (int i = 0; i < exprEvaluators.Length; i++)
            {
                object expressionReturnType = expressionReturnTypes[count];
                selPropertyTypes.Put(columnNames[count], expressionReturnType);
                count++;
            }
            if (!_selectedStreams.IsEmpty())
            {
                foreach (SelectClauseStreamCompiledSpec element in namedStreams)
                {
                    EventType eventTypeStream;
                    if (element.TableMetadata != null)
                    {
                        eventTypeStream = element.TableMetadata.PublicEventType;
                    }
                    else
                    {
                        eventTypeStream = _typeService.EventTypes[element.StreamNumber];
                    }
                    selPropertyTypes.Put(columnNames[count], eventTypeStream);
                    count++;
                }
                if (_isUsingWildcard && _typeService.EventTypes.Length > 1)
                {
                    for (int i = 0; i < _typeService.EventTypes.Length; i++)
                    {
                        EventType eventTypeStream = _typeService.EventTypes[i];
                        selPropertyTypes.Put(columnNames[count], eventTypeStream);
                        count++;
                    }
                }
            }

            // Handle stream selection
            EventType underlyingEventType = null;
            int underlyingStreamNumber = 0;
            bool underlyingIsFragmentEvent = false;
            EventPropertyGetter underlyingPropertyEventGetter = null;
            ExprEvaluator underlyingExprEvaluator = null;
            var representation = EventRepresentationUtil.GetRepresentation(
                _annotations, _configuration, AssignedType.NONE);

            if (!_selectedStreams.IsEmpty())
            {
                // Resolve underlying event type in the case of wildcard or non-named stream select.
                // Determine if the we are considering a tagged event or a stream name.
                if (_isUsingWildcard || (!unnamedStreams.IsEmpty()))
                {
                    if (!unnamedStreams.IsEmpty())
                    {
                        if (unnamedStreams[0].StreamSelected != null)
                        {
                            SelectClauseStreamCompiledSpec streamSpec = unnamedStreams[0].StreamSelected;

                            // the tag.* syntax for :  select tag.* from pattern [tag = A]
                            underlyingStreamNumber = streamSpec.StreamNumber;
                            if (streamSpec.IsFragmentEvent)
                            {
                                EventType compositeMap = _typeService.EventTypes[underlyingStreamNumber];
                                FragmentEventType fragment = compositeMap.GetFragmentType(streamSpec.StreamName);
                                underlyingEventType = fragment.FragmentType;
                                underlyingIsFragmentEvent = true;
                            }
                            else if (streamSpec.IsProperty)
                            {
                                // the property.* syntax for :  select property.* from A
                                string propertyName = streamSpec.StreamName;
                                Type propertyType = streamSpec.PropertyType;
                                int streamNumber = streamSpec.StreamNumber;

                                if (streamSpec.PropertyType.IsBuiltinDataType())
                                {
                                    throw new ExprValidationException(
                                        "The property wildcard syntax cannot be used on built-in types as returned by property '" +
                                        propertyName + "'");
                                }

                                // create or get an underlying type for that Class
                                underlyingEventType = _eventAdapterService.AddBeanType(
                                    propertyType.GetDefaultTypeName(), propertyType, false, false, false);
                                _selectExprEventTypeRegistry.Add(underlyingEventType);
                                underlyingPropertyEventGetter =
                                    _typeService.EventTypes[streamNumber].GetGetter(propertyName);
                                if (underlyingPropertyEventGetter == null)
                                {
                                    throw new ExprValidationException(
                                        "Unexpected error resolving property getter for property " + propertyName);
                                }
                            }
                            else
                            {
                                // the stream.* syntax for:  select a.* from A as a
                                underlyingEventType = _typeService.EventTypes[underlyingStreamNumber];
                            }
                        }
                        else
                        {
                            // handle case where the unnamed stream is a "transpose" function, for non-insert-into
                            if (_insertIntoDesc == null || insertIntoTargetType == null)
                            {
                                ExprNode expression = unnamedStreams[0].ExpressionSelectedAsStream.SelectExpression;
                                Type returnType = expression.ExprEvaluator.ReturnType;
                                if (returnType == typeof (Object[]) ||
                                    returnType.IsImplementsInterface(typeof (IDictionary<string, object>)) ||
                                    returnType.IsBuiltinDataType())
                                {
                                    throw new ExprValidationException(
                                        "Invalid expression return type '" + Name.Clean(returnType) +
                                        "' for transpose function");
                                }
                                underlyingEventType = _eventAdapterService.AddBeanType(
                                    returnType.GetDefaultTypeName(), returnType, false, false, false);
                                _selectExprEventTypeRegistry.Add(underlyingEventType);
                                underlyingExprEvaluator = expression.ExprEvaluator;
                            }
                        }
                    }
                    else
                    {
                        // no un-named stream selectors, but a wildcard was specified
                        if (_typeService.EventTypes.Length == 1)
                        {
                            // not a join, we are using the selected event
                            underlyingEventType = _typeService.EventTypes[0];
                            if (underlyingEventType is WrapperEventType)
                            {
                                singleStreamWrapper = true;
                            }
                        }
                        else
                        {
                            // For joins, all results are placed in a map with properties for each stream
                            underlyingEventType = null;
                        }
                    }
                }
            }

            var selectExprContext = new SelectExprContext(exprEvaluators, columnNames, _eventAdapterService);

            if (_insertIntoDesc == null)
            {
                if (!_selectedStreams.IsEmpty())
                {
                    EventType resultEventTypeX;
                    if (underlyingEventType != null)
                    {
                        TableMetadata tableMetadata = _tableService.GetTableMetadataFromEventType(underlyingEventType);
                        if (tableMetadata != null)
                        {
                            underlyingEventType = tableMetadata.PublicEventType;
                        }

                        resultEventTypeX = _eventAdapterService.CreateAnonymousWrapperType(
                            _statementId + "_wrapout_" + CollectionUtil.ToString(_assignedTypeNumberStack, "_"),
                            underlyingEventType, selPropertyTypes);

                        return new EvalSelectStreamWUnderlying(
                            selectExprContext, resultEventTypeX, namedStreams, _isUsingWildcard,
                            unnamedStreams, singleStreamWrapper, underlyingIsFragmentEvent, underlyingStreamNumber,
                            underlyingPropertyEventGetter, underlyingExprEvaluator, tableMetadata);
                    }
                    else
                    {
                        resultEventTypeX =
                            _eventAdapterService.CreateAnonymousMapType(
                                _statementId + "_mapout_" + CollectionUtil.ToString(_assignedTypeNumberStack, "_"),
                                selPropertyTypes, true);
                        return new EvalSelectStreamNoUnderlyingMap(
                            selectExprContext, resultEventTypeX, namedStreams, _isUsingWildcard);
                    }
                }

                if (_isUsingWildcard)
                {
                    EventType resultEventTypeX =
                        _eventAdapterService.CreateAnonymousWrapperType(
                            _statementId + "_wrapoutwild_" + CollectionUtil.ToString(_assignedTypeNumberStack, "_"),
                            eventType, selPropertyTypes);
                    if (singleStreamWrapper)
                    {
                        return new EvalSelectWildcardSSWrapper(selectExprContext, resultEventTypeX);
                    }
                    if (joinWildcardProcessor == null)
                    {
                        return new EvalSelectWildcard(selectExprContext, resultEventTypeX);
                    }
                    return new EvalSelectWildcardJoin(selectExprContext, resultEventTypeX, joinWildcardProcessor);
                }

                EventType resultEventType;
                if (representation == EventUnderlyingType.OBJECTARRAY)
                {
                    resultEventType =
                        _eventAdapterService.CreateAnonymousObjectArrayType(
                            _statementId + "_result_" + CollectionUtil.ToString(_assignedTypeNumberStack, "_"),
                            selPropertyTypes);
                }
                else if (representation == EventUnderlyingType.AVRO)
                {
                    resultEventType =
                        _eventAdapterService.CreateAnonymousAvroType(
                            _statementId + "_result_" + CollectionUtil.ToString(_assignedTypeNumberStack, "_"),
                            selPropertyTypes, _annotations, _statementName, _typeService.EngineURIQualifier);
                }
                else
                {
                    resultEventType =
                        _eventAdapterService.CreateAnonymousMapType(
                            _statementId + "_result_" + CollectionUtil.ToString(_assignedTypeNumberStack, "_"),
                            selPropertyTypes, true);
                }

                if (selectExprContext.ExpressionNodes.Length == 0)
                {
                    return new EvalSelectNoWildcardEmptyProps(selectExprContext, resultEventType);
                }
                else
                {
                    if (representation == EventUnderlyingType.OBJECTARRAY)
                    {
                        return new EvalSelectNoWildcardObjectArray(selectExprContext, resultEventType);
                    }
                    else if (representation == EventUnderlyingType.AVRO)
                    {
                        return
                            _eventAdapterService.EventAdapterAvroHandler.GetOutputFactory().MakeSelectNoWildcard(
                                selectExprContext, resultEventType, _tableService, _statementName,
                                _typeService.EngineURIQualifier);
                    }
                    return new EvalSelectNoWildcardMap(selectExprContext, resultEventType);
                }
            }

            EventType vaeInnerEventType = null;
            bool singleColumnWrapOrBeanCoercion = false;
            // Additional single-column coercion for non-wrapped type done by SelectExprInsertEventBeanFactory
            bool isRevisionEvent = false;

            try
            {
                if (!_selectedStreams.IsEmpty())
                {
                    EventType resultEventTypeX;

                    // handle "transpose" special function with predefined target type
                    if (insertIntoTargetType != null && _selectedStreams[0].ExpressionSelectedAsStream != null)
                    {
                        if (exprEvaluators.Length != 0)
                        {
                            throw new ExprValidationException(
                                "Cannot transpose additional properties in the select-clause to target event type '" +
                                insertIntoTargetType.Name +
                                "' with underlying type '" + insertIntoTargetType.UnderlyingType.FullName + "', the " +
                                EngineImportServiceConstants.EXT_SINGLEROW_FUNCTION_TRANSPOSE +
                                " function must occur alone in the select clause");
                        }
                        ExprNode expression = unnamedStreams[0].ExpressionSelectedAsStream.SelectExpression;
                        Type returnType = expression.ExprEvaluator.ReturnType;
                        if (insertIntoTargetType is ObjectArrayEventType && returnType == typeof (Object[]))
                        {
                            return
                                new SelectExprInsertEventBeanFactory.SelectExprInsertNativeExpressionCoerceObjectArray(
                                    insertIntoTargetType, expression.ExprEvaluator, _eventAdapterService);
                        }
                        else if (insertIntoTargetType is MapEventType &&
                                    returnType.IsImplementsInterface(typeof (IDictionary<string, object>)))
                        {
                            return
                                new SelectExprInsertEventBeanFactory.SelectExprInsertNativeExpressionCoerceMap(
                                    insertIntoTargetType, expression.ExprEvaluator, _eventAdapterService);
                        }
                        else if (insertIntoTargetType is BeanEventType &&
                                    TypeHelper.IsSubclassOrImplementsInterface(
                                        returnType, insertIntoTargetType.UnderlyingType))
                        {
                            return
                                new SelectExprInsertEventBeanFactory.SelectExprInsertNativeExpressionCoerceNative(
                                    insertIntoTargetType, expression.ExprEvaluator, _eventAdapterService);
                        }
                        else if (insertIntoTargetType is AvroSchemaEventType &&
                                    returnType.FullName.Equals(AvroConstantsNoDep.GENERIC_RECORD_CLASSNAME))
                        {
                            return
                                new SelectExprInsertEventBeanFactory.SelectExprInsertNativeExpressionCoerceAvro(
                                    insertIntoTargetType, expression.ExprEvaluator, _eventAdapterService);
                        }
                        else if (insertIntoTargetType is WrapperEventType)
                        {
                            // for native event types as they got renamed, they become wrappers
                            // check if the proposed wrapper is compatible with the existing wrapper
                            var existing = (WrapperEventType) insertIntoTargetType;
                            if (existing.UnderlyingEventType is BeanEventType)
                            {
                                var innerType = (BeanEventType) existing.UnderlyingEventType;
                                ExprEvaluator evalExprEvaluator =
                                    unnamedStreams[0].ExpressionSelectedAsStream.SelectExpression.ExprEvaluator;
                                if (
                                    !TypeHelper.IsSubclassOrImplementsInterface(
                                        evalExprEvaluator.ReturnType, innerType.UnderlyingType))
                                {
                                    throw new ExprValidationException(
                                        "Invalid expression return type '" + Name.Clean(evalExprEvaluator.ReturnType) +
                                        "' for transpose function, expected '" +
                                        innerType.UnderlyingType.Name + "'");
                                }
                                resultEventTypeX = _eventAdapterService.AddWrapperType(
                                    insertIntoTargetType.Name, existing.UnderlyingEventType, selPropertyTypes,
                                    false, true);
                                return new EvalSelectStreamWUnderlying(
                                    selectExprContext, resultEventTypeX, namedStreams, _isUsingWildcard,
                                    unnamedStreams, false, false, underlyingStreamNumber, null,
                                    evalExprEvaluator, null);
                            }
                        }
                        throw EvalInsertUtil.MakeEventTypeCastException(returnType, insertIntoTargetType);
                    }

                    if (underlyingEventType != null)
                    {
                        // a single stream was selected via "stream.*" and there is no column name
                        // recast as a Map-type
                        if (underlyingEventType is MapEventType && insertIntoTargetType is MapEventType)
                        {
                            return EvalSelectStreamWUndRecastMapFactory.Make(
                                _typeService.EventTypes, selectExprContext,
                                _selectedStreams[0].StreamSelected.StreamNumber, insertIntoTargetType, exprNodes,
                                _engineImportService, _statementName, _typeService.EngineURIQualifier);
                        }

                        // recast as a Object-array-type
                        if (underlyingEventType is ObjectArrayEventType && insertIntoTargetType is ObjectArrayEventType)
                        {
                            return EvalSelectStreamWUndRecastObjectArrayFactory.Make(
                                _typeService.EventTypes, selectExprContext,
                                _selectedStreams[0].StreamSelected.StreamNumber, insertIntoTargetType, exprNodes,
                                _engineImportService, _statementName, _typeService.EngineURIQualifier);
                        }

                        // recast as a Avro-type
                        if (underlyingEventType is AvroSchemaEventType && insertIntoTargetType is AvroSchemaEventType)
                        {
                            return
                                _eventAdapterService.EventAdapterAvroHandler.GetOutputFactory().MakeRecast(
                                    _typeService.EventTypes, selectExprContext,
                                    _selectedStreams[0].StreamSelected.StreamNumber,
                                    (AvroSchemaEventType) insertIntoTargetType, exprNodes, _statementName,
                                    _typeService.EngineURIQualifier);
                        }

                        // recast as a Bean-type
                        if (underlyingEventType is BeanEventType && insertIntoTargetType is BeanEventType)
                        {
                            return new EvalInsertBeanRecast(
                                insertIntoTargetType, _eventAdapterService,
                                _selectedStreams[0].StreamSelected.StreamNumber, _typeService.EventTypes);
                        }

                        // wrap if no recast possible
                        TableMetadata tableMetadata = _tableService.GetTableMetadataFromEventType(underlyingEventType);
                        if (tableMetadata != null)
                        {
                            underlyingEventType = tableMetadata.PublicEventType;
                        }
                        resultEventTypeX = _eventAdapterService.AddWrapperType(
                            _insertIntoDesc.EventTypeName, underlyingEventType, selPropertyTypes, false, true);
                        return new EvalSelectStreamWUnderlying(
                            selectExprContext, resultEventTypeX, namedStreams, _isUsingWildcard,
                            unnamedStreams, singleStreamWrapper, underlyingIsFragmentEvent, underlyingStreamNumber,
                            underlyingPropertyEventGetter, underlyingExprEvaluator, tableMetadata);
                    }
                    else
                    {
                        // there are one or more streams selected with column name such as "stream.* as columnOne"
                        if (insertIntoTargetType is BeanEventType)
                        {
                            string name = _selectedStreams[0].StreamSelected.StreamName;
                            string alias = _selectedStreams[0].StreamSelected.OptionalName;
                            string syntaxUsed = name + ".*" + (alias != null ? " as " + alias : "");
                            string syntaxInstead = name + (alias != null ? " as " + alias : "");
                            throw new ExprValidationException(
                                "The '" + syntaxUsed +
                                "' syntax is not allowed when inserting into an existing bean event type, use the '" +
                                syntaxInstead + "' syntax instead");
                        }
                        if (insertIntoTargetType == null || insertIntoTargetType is MapEventType)
                        {
                            resultEventTypeX = _eventAdapterService.AddNestableMapType(
                                _insertIntoDesc.EventTypeName, selPropertyTypes, null, false, false, false, false, true);
                            ISet<string> propertiesToUnwrap = GetEventBeanToObjectProps(
                                selPropertyTypes, resultEventTypeX);
                            if (propertiesToUnwrap.IsEmpty())
                            {
                                return new EvalSelectStreamNoUnderlyingMap(
                                    selectExprContext, resultEventTypeX, namedStreams, _isUsingWildcard);
                            }
                            else
                            {
                                return new EvalSelectStreamNoUndWEventBeanToObj(
                                    selectExprContext, resultEventTypeX, namedStreams, _isUsingWildcard,
                                    propertiesToUnwrap);
                            }
                        }
                        else if (insertIntoTargetType is ObjectArrayEventType)
                        {
                            ISet<string> propertiesToUnwrap = GetEventBeanToObjectProps(
                                selPropertyTypes, insertIntoTargetType);
                            if (propertiesToUnwrap.IsEmpty())
                            {
                                return new EvalSelectStreamNoUnderlyingObjectArray(
                                    selectExprContext, insertIntoTargetType, namedStreams, _isUsingWildcard);
                            }
                            else
                            {
                                return new EvalSelectStreamNoUndWEventBeanToObjObjArray(
                                    selectExprContext, insertIntoTargetType, namedStreams, _isUsingWildcard,
                                    propertiesToUnwrap);
                            }
                        }
                        else if (insertIntoTargetType is AvroSchemaEventType)
                        {
                            throw new ExprValidationException("Avro event type does not allow contained beans");
                        }
                        else
                        {
                            throw new IllegalStateException("Unrecognized event type " + insertIntoTargetType);
                        }
                    }
                }

                ValueAddEventProcessor vaeProcessor =
                    _valueAddEventService.GetValueAddProcessor(_insertIntoDesc.EventTypeName);
                EventType resultEventType;
                if (_isUsingWildcard)
                {
                    if (vaeProcessor != null)
                    {
                        resultEventType = vaeProcessor.ValueAddEventType;
                        isRevisionEvent = true;
                        vaeProcessor.ValidateEventType(eventType);
                    }
                    else
                    {
                        if (insertIntoTargetType != null)
                        {
                            // handle insert-into with fast coercion (no additional properties selected)
                            if (selPropertyTypes.IsEmpty())
                            {
                                if (insertIntoTargetType is BeanEventType && eventType is BeanEventType)
                                {
                                    return new EvalInsertBeanRecast(
                                        insertIntoTargetType, _eventAdapterService, 0, _typeService.EventTypes);
                                }
                                if (insertIntoTargetType is ObjectArrayEventType && eventType is ObjectArrayEventType)
                                {
                                    var target = (ObjectArrayEventType) insertIntoTargetType;
                                    var source = (ObjectArrayEventType) eventType;
                                    string msg = BaseNestableEventType.IsDeepEqualsProperties(
                                        eventType.Name, source.Types, target.Types);
                                    if (msg == null)
                                    {
                                        return new EvalInsertCoercionObjectArray(
                                            insertIntoTargetType, _eventAdapterService);
                                    }
                                }
                                if (insertIntoTargetType is MapEventType && eventType is MapEventType)
                                {
                                    return new EvalInsertCoercionMap(insertIntoTargetType, _eventAdapterService);
                                }
                                if (insertIntoTargetType is AvroSchemaEventType && eventType is AvroSchemaEventType)
                                {
                                    return new EvalInsertCoercionAvro(insertIntoTargetType, _eventAdapterService);
                                }
                                if (insertIntoTargetType is WrapperEventType && eventType is BeanEventType)
                                {
                                    var wrapperType = (WrapperEventType) insertIntoTargetType;
                                    if (wrapperType.UnderlyingEventType is BeanEventType)
                                    {
                                        return new EvalInsertBeanWrapRecast(
                                            wrapperType, _eventAdapterService, 0, _typeService.EventTypes);
                                    }
                                }
                            }

                            // handle insert-into by generating the writer with possible additional properties
                            SelectExprProcessor existingTypeProcessor =
                                SelectExprInsertEventBeanFactory.GetInsertUnderlyingNonJoin(
                                    _eventAdapterService, insertIntoTargetType, _isUsingWildcard, _typeService,
                                    exprEvaluators, columnNames, expressionReturnTypes, _engineImportService,
                                    _insertIntoDesc, columnNamesAsProvided, true, _statementName);
                            if (existingTypeProcessor != null)
                            {
                                return existingTypeProcessor;
                            }
                        }

                        if (selPropertyTypes.IsEmpty() && eventType is BeanEventType)
                        {
                            var beanEventType = (BeanEventType) eventType;
                            resultEventType = _eventAdapterService.AddBeanTypeByName(
                                _insertIntoDesc.EventTypeName, beanEventType.UnderlyingType, false);
                        }
                        else
                        {
                            resultEventType = _eventAdapterService.AddWrapperType(
                                _insertIntoDesc.EventTypeName, eventType, selPropertyTypes, false, true);
                        }
                    }

                    if (singleStreamWrapper)
                    {
                        if (!isRevisionEvent)
                        {
                            return new EvalInsertWildcardSSWrapper(selectExprContext, resultEventType);
                        }
                        else
                        {
                            return new EvalInsertWildcardSSWrapperRevision(
                                selectExprContext, resultEventType, vaeProcessor);
                        }
                    }
                    if (joinWildcardProcessor == null)
                    {
                        if (!isRevisionEvent)
                        {
                            if (resultEventType is WrapperEventType)
                            {
                                return new EvalInsertWildcardWrapper(selectExprContext, resultEventType);
                            }
                            else
                            {
                                return new EvalInsertWildcardBean(selectExprContext, resultEventType);
                            }
                        }
                        else
                        {
                            if (exprEvaluators.Length == 0)
                            {
                                return new EvalInsertWildcardRevision(selectExprContext, resultEventType, vaeProcessor);
                            }
                            else
                            {
                                EventType wrappingEventType =
                                    _eventAdapterService.AddWrapperType(
                                        _insertIntoDesc.EventTypeName + "_wrapped", eventType, selPropertyTypes, false,
                                        true);
                                return new EvalInsertWildcardRevisionWrapper(
                                    selectExprContext, resultEventType, vaeProcessor, wrappingEventType);
                            }
                        }
                    }
                    else
                    {
                        if (!isRevisionEvent)
                        {
                            return new EvalInsertWildcardJoin(selectExprContext, resultEventType, joinWildcardProcessor);
                        }
                        else
                        {
                            return new EvalInsertWildcardJoinRevision(
                                selectExprContext, resultEventType, joinWildcardProcessor, vaeProcessor);
                        }
                    }
                }

                // not using wildcard
                resultEventType = null;
                if ((columnNames.Length == 1) && (_insertIntoDesc.ColumnNames.Count == 0))
                {
                    if (insertIntoTargetType != null)
                    {
                        // check if the existing type and new type are compatible
                        object columnOneType = expressionReturnTypes[0];
                        if (insertIntoTargetType is WrapperEventType)
                        {
                            var wrapperType = (WrapperEventType) insertIntoTargetType;
                            // Map and Object both supported
#pragma warning disable CS0253 // Possible unintended reference comparison; right hand side needs cast
                            if (wrapperType.UnderlyingEventType.UnderlyingType == columnOneType)
#pragma warning restore CS0253 // Possible unintended reference comparison; right hand side needs cast
                            {
                                singleColumnWrapOrBeanCoercion = true;
                                resultEventType = insertIntoTargetType;
                            }
                        }
                        if ((insertIntoTargetType is BeanEventType) && (columnOneType is Type))
                        {
                            var beanType = (BeanEventType) insertIntoTargetType;
                            // Map and Object both supported
                            if (TypeHelper.IsSubclassOrImplementsInterface(
                                (Type) columnOneType, beanType.UnderlyingType))
                            {
                                singleColumnWrapOrBeanCoercion = true;
                                resultEventType = insertIntoTargetType;
                            }
                        }
                    }
                }
                if (singleColumnWrapOrBeanCoercion)
                {
                    if (!isRevisionEvent)
                    {
                        if (resultEventType is WrapperEventType)
                        {
                            var wrapper = (WrapperEventType) resultEventType;
                            if (wrapper.UnderlyingEventType is MapEventType)
                            {
                                return new EvalInsertNoWildcardSingleColCoercionMapWrap(selectExprContext, wrapper);
                            }
                            else if (wrapper.UnderlyingEventType is ObjectArrayEventType)
                            {
                                return new EvalInsertNoWildcardSingleColCoercionObjectArrayWrap(
                                    selectExprContext, wrapper);
                            }
                            else if (wrapper.UnderlyingEventType is AvroSchemaEventType)
                            {
                                return new EvalInsertNoWildcardSingleColCoercionAvroWrap(selectExprContext, wrapper);
                            }
                            else if (wrapper.UnderlyingEventType is VariantEventType)
                            {
                                var variantEventType = (VariantEventType) wrapper.UnderlyingEventType;
                                vaeProcessor = _valueAddEventService.GetValueAddProcessor(variantEventType.Name);
                                return new EvalInsertNoWildcardSingleColCoercionBeanWrapVariant(
                                    selectExprContext, wrapper, vaeProcessor);
                            }
                            else
                            {
                                return new EvalInsertNoWildcardSingleColCoercionBeanWrap(selectExprContext, wrapper);
                            }
                        }
                        else
                        {
                            if (resultEventType is BeanEventType)
                            {
                                return new EvalInsertNoWildcardSingleColCoercionBean(selectExprContext, resultEventType);
                            }
                        }
                    }
                    else
                    {
                        if (resultEventType is BeanEventType)
                        {
                            return new EvalInsertNoWildcardSingleColCoercionRevisionBean(
                                selectExprContext, resultEventType, vaeProcessor, vaeInnerEventType);
                        }
                        else
                        {
                            Func<EventAdapterService, object, EventType, EventBean> func;
                            if (resultEventType is MapEventType)
                            {
                                func =
                                    (eventAdapterService, und, type) =>
                                        eventAdapterService.AdapterForTypedMap((IDictionary<string, object>) und, type);
                            }
                            else if (resultEventType is ObjectArrayEventType)
                            {
                                func =
                                    (eventAdapterService, und, type) =>
                                        eventAdapterService.AdapterForTypedObjectArray((Object[]) und, type);
                            }
                            else if (resultEventType is AvroSchemaEventType)
                            {
                                func =
                                    (eventAdapterService, und, type) =>
                                        eventAdapterService.AdapterForTypedAvro(und, type);
                            }
                            else
                            {
                                func =
                                    (eventAdapterService, und, type) =>
                                        eventAdapterService.AdapterForTypedObject(und, type);
                            }
                            return new EvalInsertNoWildcardSingleColCoercionRevisionFunc(
                                selectExprContext, resultEventType, vaeProcessor, vaeInnerEventType, func);
                        }
                    }
                }
                if (resultEventType == null)
                {
                    if (vaeProcessor != null)
                    {
                        // Use an anonymous type if the target is not a variant stream
                        if (_valueAddEventService.GetValueAddProcessor(_insertIntoDesc.EventTypeName) == null)
                        {
                            resultEventType =
                                _eventAdapterService.CreateAnonymousMapType(
                                    _statementId + "_vae_" + CollectionUtil.ToString(_assignedTypeNumberStack, "_"),
                                    selPropertyTypes, true);
                        }
                        else
                        {
                            string statementName = "stmt_" + _statementId + "_insert";
                            resultEventType = _eventAdapterService.AddNestableMapType(
                                statementName, selPropertyTypes, null, false, false, false, false, true);
                        }
                    }
                    else
                    {
                        EventType existingType = insertIntoTargetType;
                        if (existingType == null)
                        {
                            // The type may however be an auto-import or fully-qualified class name
                            Type clazz = null;
                            try
                            {
                                clazz = _engineImportService.ResolveType(_insertIntoDesc.EventTypeName, false);
                            }
                            catch (EngineImportException)
                            {
                                Log.Debug(
                                    "Target stream name '" + _insertIntoDesc.EventTypeName +
                                    "' is not resolved as a class name");
                            }
                            if (clazz != null)
                            {
                                existingType = _eventAdapterService.AddBeanType(clazz.GetDefaultTypeName(), clazz, false, false, false);
                            }
                        }

                        SelectExprProcessor selectExprInsertEventBean = null;
                        if (existingType != null)
                        {
                            selectExprInsertEventBean =
                                SelectExprInsertEventBeanFactory.GetInsertUnderlyingNonJoin(
                                    _eventAdapterService, existingType, _isUsingWildcard, _typeService, exprEvaluators,
                                    columnNames, expressionReturnTypes, _engineImportService, _insertIntoDesc,
                                    columnNamesAsProvided, false, _statementName);
                        }
                        if (selectExprInsertEventBean != null)
                        {
                            return selectExprInsertEventBean;
                        }
                        else
                        {
                            // use the provided override-type if there is one
                            if (_optionalInsertIntoOverrideType != null)
                            {
                                resultEventType = insertIntoTargetType;
                            }
                            else if (existingType is AvroSchemaEventType)
                            {
                                _eventAdapterService.EventAdapterAvroHandler.AvroCompat(existingType, selPropertyTypes);
                                resultEventType = existingType;
                            }
                            else
                            {
                                var @out = EventRepresentationUtil.GetRepresentation(
                                    _annotations, _configuration, AssignedType.NONE);
                                if (@out == EventUnderlyingType.MAP)
                                {
                                    resultEventType =
                                        _eventAdapterService.AddNestableMapType(
                                            _insertIntoDesc.EventTypeName, selPropertyTypes, null, false, false, false,
                                            false, true);
                                }
                                else if (@out == EventUnderlyingType.OBJECTARRAY)
                                {
                                    resultEventType =
                                        _eventAdapterService.AddNestableObjectArrayType(
                                            _insertIntoDesc.EventTypeName, selPropertyTypes, null, false, false, false,
                                            false, true, false, null);
                                }
                                else if (@out == EventUnderlyingType.AVRO)
                                {
                                    resultEventType = _eventAdapterService.AddAvroType(
                                        _insertIntoDesc.EventTypeName, selPropertyTypes, false, false, false, false,
                                        true, _annotations, null, _statementName, _typeService.EngineURIQualifier);
                                }
                                else
                                {
                                    throw new IllegalStateException("Unrecognized code " + @out);
                                }
                            }
                        }
                    }
                }
                if (vaeProcessor != null)
                {
                    vaeProcessor.ValidateEventType(resultEventType);
                    vaeInnerEventType = resultEventType;
                    resultEventType = vaeProcessor.ValueAddEventType;
                    isRevisionEvent = true;
                }

                if (!isRevisionEvent)
                {
                    if (resultEventType is MapEventType)
                    {
                        return new EvalInsertNoWildcardMap(selectExprContext, resultEventType);
                    }
                    else if (resultEventType is ObjectArrayEventType)
                    {
                        return MakeObjectArrayConsiderReorder(
                            selectExprContext, (ObjectArrayEventType) resultEventType, _statementName,
                            _typeService.EngineURIQualifier);
                    }
                    else if (resultEventType is AvroSchemaEventType)
                    {
                        return
                            _eventAdapterService.EventAdapterAvroHandler.GetOutputFactory().MakeSelectNoWildcard(
                                selectExprContext, resultEventType, _tableService, _statementName,
                                _typeService.EngineURIQualifier);
                    }
                    else
                    {
                        throw new IllegalStateException("Unrecognized output type " + resultEventType);
                    }
                }
                else
                {
                    return new EvalInsertNoWildcardRevision(
                        selectExprContext, resultEventType, vaeProcessor, vaeInnerEventType);
                }
            }
            catch (EventAdapterException ex)
            {
                Log.Debug("Exception provided by event adapter: " + ex.Message, ex);
                throw new ExprValidationException(ex.Message, ex);
            }
        }

        private bool IsGroupByRollupNullableExpression(ExprNode expr, GroupByRollupInfo groupByRollupInfo)
        {
            // if all levels include this key, we are fine
            foreach (AggregationGroupByRollupLevel level in groupByRollupInfo.RollupDesc.Levels)
            {
                if (level.IsAggregationTop)
                {
                    return true;
                }
                bool found = false;
                foreach (int rollupKeyIndex in level.RollupKeys)
                {
                    ExprNode groupExpression = groupByRollupInfo.ExprNodes[rollupKeyIndex];
                    if (ExprNodeUtility.DeepEquals(groupExpression, expr, false))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return true;
                }
            }
            return false;
        }

        private SelectExprProcessor MakeObjectArrayConsiderReorder(
            SelectExprContext selectExprContext,
            ObjectArrayEventType resultEventType,
            string statementName,
            string engineURI)
        {
            var wideners = new TypeWidener[selectExprContext.ColumnNames.Length];
            var remapped = new int[selectExprContext.ColumnNames.Length];
            bool needRemap = false;
            for (int i = 0; i < selectExprContext.ColumnNames.Length; i++)
            {
                string colName = selectExprContext.ColumnNames[i];
                int index = CollectionUtil.FindItem(resultEventType.PropertyNames, colName);
                if (index == -1)
                {
                    throw new ExprValidationException(
                        "Could not find property '" + colName + "' in " +
                        GetTypeNameConsiderTable(resultEventType, _tableService));
                }
                remapped[i] = index;
                if (index != i)
                {
                    needRemap = true;
                }
                Type sourceColumnType = selectExprContext.ExpressionNodes[i].ReturnType;
                Type targetPropType = resultEventType.GetPropertyType(colName);
                wideners[i] = TypeWidenerFactory.GetCheckPropertyAssignType(
                    colName, sourceColumnType, targetPropType, colName, false,
                    _eventAdapterService.GetTypeWidenerCustomizer(resultEventType), statementName, engineURI);
            }

            if (!needRemap)
            {
                return new EvalInsertNoWildcardObjectArray(selectExprContext, resultEventType);
            }
            if (CollectionUtil.IsAllNullArray(wideners))
            {
                return new EvalInsertNoWildcardObjectArrayRemap(selectExprContext, resultEventType, remapped);
            }
            return new EvalInsertNoWildcardObjectArrayRemapWWiden(
                selectExprContext, resultEventType, remapped, wideners);
        }

        private string GetTypeNameConsiderTable(ObjectArrayEventType resultEventType, TableService tableService)
        {
            TableMetadata metadata = tableService.GetTableMetadataFromEventType(resultEventType);
            if (metadata != null)
            {
                return "table '" + metadata.TableName + "'";
            }
            return "type '" + resultEventType.Name + "'";
        }

        private Pair<ExprEvaluator, Object> HandleUnderlyingStreamInsert(
            ExprEvaluator exprEvaluator,
            NamedWindowMgmtService namedWindowMgmtService,
            EventAdapterService eventAdapterService)
        {
            if (!(exprEvaluator is ExprStreamUnderlyingNode))
            {
                return null;
            }
            var undNode = (ExprStreamUnderlyingNode) exprEvaluator;
            int streamNum = undNode.StreamId;
            Type returnType = undNode.ExprEvaluator.ReturnType;
            EventType namedWindowAsType = GetNamedWindowUnderlyingType(
                namedWindowMgmtService, eventAdapterService, _typeService.EventTypes[streamNum]);
            TableMetadata tableMetadata = _tableService.GetTableMetadataFromEventType(
                _typeService.EventTypes[streamNum]);

            EventType eventTypeStream;
            ExprEvaluator evaluator;
            if (tableMetadata != null)
            {
                eventTypeStream = tableMetadata.PublicEventType;
                evaluator = new SelectExprProcessorEvalStreamInsertTable(streamNum, undNode, tableMetadata, returnType);
            }
            else if (namedWindowAsType == null)
            {
                eventTypeStream = _typeService.EventTypes[streamNum];
                evaluator = new SelectExprProcessorEvalStreamInsertUnd(undNode, streamNum, returnType);
            }
            else
            {
                eventTypeStream = namedWindowAsType;
                evaluator = new SelectExprProcessorEvalStreamInsertNamedWindow(
                    streamNum, namedWindowAsType, returnType, eventAdapterService);
            }

            return new Pair<ExprEvaluator, Object>(evaluator, eventTypeStream);
        }

        private EventType GetNamedWindowUnderlyingType(
            NamedWindowMgmtService namedWindowMgmtService,
            EventAdapterService eventAdapterService,
            EventType eventType)
        {
            if (!namedWindowMgmtService.IsNamedWindow(eventType.Name))
            {
                return null;
            }
            NamedWindowProcessor processor = namedWindowMgmtService.GetProcessor(eventType.Name);
            if (processor.EventTypeAsName == null)
            {
                return null;
            }
            return eventAdapterService.GetEventTypeByName(processor.EventTypeAsName);
        }

        private TypeAndFunctionPair HandleTypableExpression(ExprEvaluator exprEvaluator, int expressionNum)
        {
            if (!(exprEvaluator is ExprEvaluatorTypableReturn))
            {
                return null;
            }

            var typable = (ExprEvaluatorTypableReturn) exprEvaluator;
            var eventTypeExpr = typable.RowProperties;
            if (eventTypeExpr == null)
            {
                return null;
            }

            EventType mapType =
                _eventAdapterService.CreateAnonymousMapType(
                    _statementId + "_innereval_" + CollectionUtil.ToString(_assignedTypeNumberStack, "_") + "_" +
                    expressionNum, eventTypeExpr, true);
            var evaluatorFragment = new SelectExprProcessorEvalTypableMap(mapType, exprEvaluator, _eventAdapterService);

            return new TypeAndFunctionPair(mapType, evaluatorFragment);
        }

        private TypeAndFunctionPair HandleInsertIntoEnumeration(
            string insertIntoColName,
            EPType insertIntoTarget,
            ExprEvaluator exprEvaluator,
            EngineImportService engineImportService)
        {
            if (!(exprEvaluator is ExprEvaluatorEnumeration) || insertIntoTarget == null
                || (!EPTypeHelper.IsCarryEvent(insertIntoTarget)))
            {
                return null;
            }

            var enumeration = (ExprEvaluatorEnumeration) exprEvaluator;
            EventType eventTypeSingle = enumeration.GetEventTypeSingle(_eventAdapterService, _statementId);
            EventType eventTypeColl = enumeration.GetEventTypeCollection(_eventAdapterService, _statementId);
            EventType sourceType = eventTypeSingle ?? eventTypeColl;
            if (eventTypeColl == null && eventTypeSingle == null)
            {
                return null; // enumeration is untyped events (select-clause provided to subquery or 'new' operator)
            }
            if (((EventTypeSPI) sourceType).Metadata.TypeClass == TypeClass.ANONYMOUS)
            {
                return null; // we don't allow anonymous types here, thus excluding subquery multi-column selection
            }

            // check type INFO
            EventType targetType = EPTypeHelper.GetEventType(insertIntoTarget);
            CheckTypeCompatible(insertIntoColName, targetType, sourceType);

            // handle collection target - produce EventBean[]
            if (insertIntoTarget is EventMultiValuedEPType)
            {
                if (eventTypeColl != null)
                {
                    var evaluatorFragmentX = new ProxyExprEvaluator
                    {
                        ProcEvaluate = eventsParams =>
                        {
                            ICollection<EventBean> events =
                                enumeration.EvaluateGetROCollectionEvents(eventsParams);
                            if (events == null)
                            {
                                return null;
                            }
                            return events.ToArray();
                        },
                        ProcReturnType = () => TypeHelper.GetArrayType(targetType.UnderlyingType)
                    };
                    return new TypeAndFunctionPair(
                        new EventType[]
                        {
                            targetType
                        }, evaluatorFragmentX);
                }
                var evaluatorFragmentY = new ProxyExprEvaluator
                {
                    ProcEvaluate = eventsParams =>
                    {
                        EventBean @event = enumeration.EvaluateGetEventBean(eventsParams);
                        if (@event == null)
                        {
                            return null;
                        }
                        return new EventBean[]
                        {
                            @event
                        };
                    },
                    ProcReturnType = () => TypeHelper.GetArrayType(targetType.UnderlyingType)
                };
                return new TypeAndFunctionPair(
                    new EventType[]
                    {
                        targetType
                    }, evaluatorFragmentY);
            }

            // handle single-bean target
            // handle single-source
            if (eventTypeSingle != null)
            {
                var evaluatorFragmentX = new ProxyExprEvaluator
                {
                    ProcEvaluate = eventsParams => enumeration.EvaluateGetEventBean(eventsParams),
                    ProcReturnType = () => targetType.UnderlyingType
                };
                return new TypeAndFunctionPair(targetType, evaluatorFragmentX);
            }

            // handle collection-source by taking the first
            var evaluatorFragment = new ProxyExprEvaluator
            {
                ProcEvaluate = eventsParams =>
                {
                    ICollection<EventBean> events =
                        enumeration.EvaluateGetROCollectionEvents(eventsParams);
                    if (events == null || events.Count == 0)
                    {
                        return null;
                    }
                    return EventBeanUtility.GetNonemptyFirstEvent(events);
                },
                ProcReturnType = () => targetType.UnderlyingType
            };
            return new TypeAndFunctionPair(targetType, evaluatorFragment);
        }

        private void CheckTypeCompatible(string insertIntoCol, EventType targetType, EventType selectedType)
        {
            if (!EventTypeUtility.IsTypeOrSubTypeOf(targetType, selectedType))
            {
                throw new ExprValidationException(
                    "Incompatible type detected attempting to insert into column '" +
                    insertIntoCol + "' type '" + targetType.Name + "' compared to selected type '" + selectedType.Name +
                    "'");
            }
        }

        private TypeAndFunctionPair HandleInsertIntoTypableExpression(
            EPType insertIntoTarget,
            ExprEvaluator exprEvaluator,
            EngineImportService engineImportService)
        {
            if (!(exprEvaluator is ExprEvaluatorTypableReturn)
                || insertIntoTarget == null
                || (!EPTypeHelper.IsCarryEvent(insertIntoTarget)))
            {
                return null;
            }

            EventType targetType = EPTypeHelper.GetEventType(insertIntoTarget);
            var typable = (ExprEvaluatorTypableReturn) exprEvaluator;
            if (typable.IsMultirow == null)
            {
                // not typable after all
                return null;
            }
            IDictionary<string, object> eventTypeExpr = typable.RowProperties;
            if (eventTypeExpr == null)
            {
                return null;
            }

            ICollection<WriteablePropertyDescriptor> writables = _eventAdapterService.GetWriteableProperties(
                targetType, false);
            var written = new List<WriteablePropertyDescriptor>();
            var writtenOffered = new List<KeyValuePair<string, object>>();

            // from IDictionary<string, Object> determine properties and type widening that may be required
            foreach (var offeredProperty in eventTypeExpr)
            {
                WriteablePropertyDescriptor writable = EventTypeUtility.FindWritable(offeredProperty.Key, writables);
                if (writable == null)
                {
                    throw new ExprValidationException(
                        "Failed to find property '" + offeredProperty.Key + "' among properties for target event type '" +
                        targetType.Name + "'");
                }
                written.Add(writable);
                writtenOffered.Add(offeredProperty);
            }

            // determine widening and column type compatibility
            var wideners = new TypeWidener[written.Count];
            var typeWidenerCustomizer = _eventAdapterService.GetTypeWidenerCustomizer(targetType);
            for (int i = 0; i < written.Count; i++)
            {
                Type expected = written[i].PropertyType;
                var provided = writtenOffered[i];
                if (provided.Value is Type)
                {
                    wideners[i] = TypeWidenerFactory.GetCheckPropertyAssignType(
                        provided.Key, (Type) provided.Value,
                        expected, written[i].PropertyName, false, typeWidenerCustomizer, _statementName,
                        _typeService.EngineURIQualifier);
                }
            }
            bool hasWideners = !CollectionUtil.IsAllNullArray(wideners);

            // obtain factory
            WriteablePropertyDescriptor[] writtenArray = written.ToArray();
            EventBeanManufacturer manufacturer;
            try
            {
                manufacturer = _eventAdapterService.GetManufacturer(
                    targetType, writtenArray, engineImportService, false);
            }
            catch (EventBeanManufactureException e)
            {
                throw new ExprValidationException("Failed to obtain eventbean factory: " + e.Message, e);
            }

            // handle collection
            EventBeanManufacturer factory = manufacturer;
            if (insertIntoTarget is EventMultiValuedEPType && typable.IsMultirow.GetValueOrDefault())
            {
                var evaluatorFragmentX = new ProxyExprEvaluator
                {
                    ProcEvaluate = eventsParams =>
                    {
                        object[][] rows = typable.EvaluateTypableMulti(
                            eventsParams.EventsPerStream,
                            eventsParams.IsNewData,
                            eventsParams.ExprEvaluatorContext
                            );
                        if (rows == null)
                        {
                            return null;
                        }
                        if (rows.Length == 0)
                        {
                            return new EventBean[0];
                        }
                        if (hasWideners)
                        {
                            ApplyWideners(rows, wideners);
                        }
                        var events = new EventBean[rows.Length];
                        for (int i = 0; i < events.Length; i++)
                        {
                            events[i] = factory.Make(rows[i]);
                        }
                        return events;
                    },
                    ProcReturnType = () => TypeHelper.GetArrayType(targetType.UnderlyingType)
                };

                return new TypeAndFunctionPair(
                    new EventType[]
                    {
                        targetType
                    }, evaluatorFragmentX);
            }
            else if (insertIntoTarget is EventMultiValuedEPType && !typable.IsMultirow.GetValueOrDefault())
            {
                var evaluatorFragmentX = new ProxyExprEvaluator
                {
                    ProcEvaluate = eventsParams =>
                    {
                        object[] row = typable.EvaluateTypableSingle(
                            eventsParams.EventsPerStream,
                            eventsParams.IsNewData,
                            eventsParams.ExprEvaluatorContext
                            );
                        if (row == null)
                        {
                            return null;
                        }
                        if (hasWideners)
                        {
                            ApplyWideners(row, wideners);
                        }
                        return new EventBean[]
                        {
                            factory.Make(row)
                        };
                    },
                    ProcReturnType = () => TypeHelper.GetArrayType(targetType.UnderlyingType)
                };
                return new TypeAndFunctionPair(
                    new EventType[]
                    {
                        targetType
                    }, evaluatorFragmentX);
            }
            else if (insertIntoTarget is EventEPType && !typable.IsMultirow.GetValueOrDefault())
            {
                var evaluatorFragmentX = new ProxyExprEvaluator
                {
                    ProcEvaluate = eventsParams =>
                    {
                        object[] row = typable.EvaluateTypableSingle(
                            eventsParams.EventsPerStream,
                            eventsParams.IsNewData,
                            eventsParams.ExprEvaluatorContext
                            );
                        if (row == null)
                        {
                            return null;
                        }
                        if (hasWideners)
                        {
                            ApplyWideners(row, wideners);
                        }
                        return factory.Make(row);
                    },
                    ProcReturnType = () => TypeHelper.GetArrayType(targetType.UnderlyingType)
                };
                return new TypeAndFunctionPair(targetType, evaluatorFragmentX);
            }

            // we are discarding all but the first row
            var evaluatorFragment = new ProxyExprEvaluator
            {
                ProcEvaluate = eventsParams =>
                {
                    object[][] rows = typable.EvaluateTypableMulti(
                        eventsParams.EventsPerStream,
                        eventsParams.IsNewData,
                        eventsParams.ExprEvaluatorContext
                        );
                    if (rows == null)
                    {
                        return null;
                    }
                    if (rows.Length == 0)
                    {
                        return new EventBean[0];
                    }
                    if (hasWideners)
                    {
                        ApplyWideners(rows[0], wideners);
                    }
                    return factory.Make(rows[0]);
                },
                ProcReturnType = () => TypeHelper.GetArrayType(targetType.UnderlyingType)
            };
            return new TypeAndFunctionPair(targetType, evaluatorFragment);
        }

        private void ApplyWideners(Object[] row, TypeWidener[] wideners)
        {
            for (int i = 0; i < wideners.Length; i++)
            {
                if (wideners[i] != null)
                {
                    row[i] = wideners[i].Invoke(row[i]);
                }
            }
        }

        private void ApplyWideners(Object[][] rows, TypeWidener[] wideners)
        {
            foreach (var row in rows)
            {
                ApplyWideners(row, wideners);
            }
        }

        private TypeAndFunctionPair HandleAtEventbeanEnumeration(bool isEventBeans, ExprEvaluator evaluator)
        {
            if (!(evaluator is ExprEvaluatorEnumeration) || !isEventBeans)
            {
                return null;
            }

            var enumEval = (ExprEvaluatorEnumeration) evaluator;
            EventType eventTypeSingle = enumEval.GetEventTypeSingle(_eventAdapterService, _statementId);
            if (eventTypeSingle != null)
            {
                TableMetadata tableMetadata = _tableService.GetTableMetadataFromEventType(eventTypeSingle);
                if (tableMetadata == null)
                {
                    var evaluatorFragmentX = new ProxyExprEvaluator
                    {
                        ProcEvaluate = eventsParams => enumEval.EvaluateGetEventBean(eventsParams),
                        ProcReturnType = () => eventTypeSingle.UnderlyingType
                    };
                    return new TypeAndFunctionPair(eventTypeSingle, evaluatorFragmentX);
                }
                var evaluatorFragment = new ProxyExprEvaluator
                {
                    ProcEvaluate = eventsParams =>
                    {
                        EventBean @event = enumEval.EvaluateGetEventBean(eventsParams);
                        if (@event == null)
                        {
                            return null;
                        }
                        return tableMetadata.EventToPublic.Convert(@event, eventsParams);
                    },
                    ProcReturnType = () => tableMetadata.PublicEventType.UnderlyingType
                };
                return new TypeAndFunctionPair(tableMetadata.PublicEventType, evaluatorFragment);
            }

            EventType eventTypeColl = enumEval.GetEventTypeCollection(_eventAdapterService, _statementId);
            if (eventTypeColl != null)
            {
                TableMetadata tableMetadata = _tableService.GetTableMetadataFromEventType(eventTypeColl);
                if (tableMetadata == null)
                {
                    var evaluatorFragmentX = new ProxyExprEvaluator
                    {
                        ProcEvaluate = eventsParams =>
                        {
                            // the protocol is EventBean[]
                            Object result = enumEval.EvaluateGetROCollectionEvents(eventsParams);
                            if (result is ICollection<EventBean>)
                            {
                                var events = (ICollection<EventBean>) result;
                                return events.ToArray();
                            }
                            return result;
                        },
                        ProcReturnType = () => TypeHelper.GetArrayType(eventTypeColl.UnderlyingType)
                    };
                    return new TypeAndFunctionPair(
                        new EventType[]
                        {
                            eventTypeColl
                        }, evaluatorFragmentX);
                }
                var evaluatorFragment = new ProxyExprEvaluator
                {
                    ProcEvaluate = eventsParams =>
                    {
                        // the protocol is EventBean[]
                        Object result = enumEval.EvaluateGetROCollectionEvents(eventsParams);
                        if (result == null)
                        {
                            return null;
                        }
                        if (result is ICollection<EventBean>)
                        {
                            var eventsX = (ICollection<EventBean>) result;
                            var @out = new EventBean[eventsX.Count];
                            int index = 0;
                            foreach (EventBean @event in eventsX)
                            {
                                @out[index++] = tableMetadata.EventToPublic.Convert(
                                    @event, eventsParams);
                            }
                            return @out;
                        }
                        var events = (EventBean[]) result;
                        for (int i = 0; i < events.Length; i++)
                        {
                            events[i] = tableMetadata.EventToPublic.Convert(
                                events[i], eventsParams);
                        }
                        return events;
                    },
                    ProcReturnType =
                        () => TypeHelper.GetArrayType(tableMetadata.PublicEventType.UnderlyingType)
                };
                return new TypeAndFunctionPair(
                    new EventType[]
                    {
                        tableMetadata.PublicEventType
                    }, evaluatorFragment);
            }

            return null;
        }

        private class TypeAndFunctionPair
        {
            internal TypeAndFunctionPair(Object type, ExprEvaluator function)
            {
                Type = type;
                Function = function;
            }

            public Object Type { get; private set; }

            public ExprEvaluator Function { get; private set; }
        }
    }
} // end of namespace