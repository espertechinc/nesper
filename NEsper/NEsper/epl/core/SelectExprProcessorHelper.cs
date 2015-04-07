///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core
{
	/// <summary>
	/// Processor for select-clause expressions that handles a list of selection items represented by
	/// expression nodes. Computes results based on matching events.
	/// </summary>
	public class SelectExprProcessorHelper
	{
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	    private readonly ICollection<int> _assignedTypeNumberStack;
	    private readonly IList<SelectClauseExprCompiledSpec> _selectionList;
	    private readonly IList<SelectExprStreamDesc> _selectedStreams;
	    private readonly InsertIntoDesc _insertIntoDesc;
	    private EventType _optionalInsertIntoOverrideType;
	    private readonly bool _isUsingWildcard;
	    private readonly StreamTypeService _typeService;
	    private readonly EventAdapterService _eventAdapterService;
	    private readonly ValueAddEventService _valueAddEventService;
	    private readonly SelectExprEventTypeRegistry _selectExprEventTypeRegistry;
	    private readonly MethodResolutionService _methodResolutionService;
	    private readonly string _statementId;
	    private readonly Attribute[] _annotations;
	    private readonly ConfigurationInformation _configuration;
	    private readonly NamedWindowService _namedWindowService;
	    private readonly TableService _tableService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="assignedTypeNumberStack">The assigned type number stack.</param>
        /// <param name="selectionList">list of select-clause items</param>
        /// <param name="selectedStreams">The selected streams.</param>
        /// <param name="insertIntoDesc">descriptor for insert-into clause contains column names overriding select clause names</param>
        /// <param name="optionalInsertIntoOverrideType">Type of the optional insert into override.</param>
        /// <param name="isUsingWildcard">true if the wildcard (*) appears in the select clause</param>
        /// <param name="typeService">service for information about streams</param>
        /// <param name="eventAdapterService">service for generating events and handling event types</param>
        /// <param name="valueAddEventService">service that handles update events</param>
        /// <param name="selectExprEventTypeRegistry">service for statement to type registry</param>
        /// <param name="methodResolutionService">for resolving methods</param>
        /// <param name="statementId">The statement identifier.</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="namedWindowService">The named window service.</param>
        /// <param name="tableService">The table service.</param>
        /// <throws>com.espertech.esper.epl.expression.core.ExprValidationException thrown if any of the expressions don't validate</throws>
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
	        MethodResolutionService methodResolutionService,
	        string statementId,
	        Attribute[] annotations,
	        ConfigurationInformation configuration,
	        NamedWindowService namedWindowService,
	        TableService tableService)
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
	        _methodResolutionService = methodResolutionService;
	        _statementId = statementId;
	        _annotations = annotations;
	        _configuration = configuration;
	        _namedWindowService = namedWindowService;
	        _tableService = tableService;
	    }

	    public SelectExprProcessor Evaluator
	    {
	        get
	        {
	            // Get the named and un-named stream selectors (i.e. select s0.* from S0 as s0), if any
	            IList<SelectClauseStreamCompiledSpec> namedStreams = new List<SelectClauseStreamCompiledSpec>();
	            IList<SelectExprStreamDesc> unnamedStreams = new List<SelectExprStreamDesc>();
	            foreach (var spec in _selectedStreams)
	            {
	                if ((spec.StreamSelected != null && spec.StreamSelected.OptionalName == null) ||
	                    (spec.ExpressionSelectedAsStream != null)) // handle special "transpose(...)" function
	                {
	                    unnamedStreams.Add(spec);
	                }
	                else
	                {
	                    namedStreams.Add(spec.StreamSelected);
	                    if (spec.StreamSelected.IsProperty)
	                    {
	                        throw new ExprValidationException("The property wildcard syntax must be used without column name");
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
	                throw new ArgumentException("Empty selection list not supported");
	            }

	            foreach (var entry in _selectionList)
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
	                    _assignedTypeNumberStack, _statementId, _typeService.StreamNames, _typeService.EventTypes,
	                    _eventAdapterService, null, _selectExprEventTypeRegistry, _methodResolutionService, _annotations,
	                    _configuration, _tableService);
	            }

	            // Resolve underlying event type in the case of wildcard select
	            EventType eventType = null;
	            var singleStreamWrapper = false;
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
	                    var tableMetadata = _tableService.GetTableMetadata(_insertIntoDesc.EventTypeName);
	                    if (tableMetadata != null)
	                    {
	                        insertIntoTargetType = tableMetadata.InternalEventType;
	                        _optionalInsertIntoOverrideType = insertIntoTargetType;
	                    }
	                }
	            }

	            // Obtain insert-into per-column type information, when available
	            var insertIntoTargetsPerCol = DetermineInsertedEventTypeTargets(insertIntoTargetType, _selectionList);

	            // Get expression nodes
	            var exprEvaluators = new ExprEvaluator[_selectionList.Count];
	            var exprNodes = new ExprNode[_selectionList.Count];
	            var expressionReturnTypes = new object[_selectionList.Count];
	            for (var i = 0; i < _selectionList.Count; i++)
	            {
	                var spec = _selectionList[i];
	                var expr = spec.SelectExpression;
	                var evaluator = expr.ExprEvaluator;
	                exprNodes[i] = expr;

	                // if there is insert-into specification, use that
	                if (_insertIntoDesc != null)
	                {
	                    // handle insert-into, with well-defined target event-typed column, and enumeration
	                    var pairX = HandleInsertIntoEnumeration(
	                        spec.ProvidedName, insertIntoTargetsPerCol[i], evaluator,
	                        _methodResolutionService.EngineImportService);
	                    if (pairX != null)
	                    {
	                        expressionReturnTypes[i] = pairX.Type;
	                        exprEvaluators[i] = pairX.Function;
	                        continue;
	                    }

	                    // handle insert-into with well-defined target event-typed column, and typable expression
	                    pairX = HandleInsertIntoTypableExpression(
	                        insertIntoTargetsPerCol[i], evaluator, _methodResolutionService.EngineImportService);
	                    if (pairX != null)
	                    {
	                        expressionReturnTypes[i] = pairX.Type;
	                        exprEvaluators[i] = pairX.Function;
	                        continue;
	                    }
	                }

	                // handle @eventbean annotation, i.e. well-defined type through enumeration
	                var pair = HandleAtEventbeanEnumeration(spec.IsEvents, evaluator);
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

	                // assign normal expected return type
	                exprEvaluators[i] = evaluator;
	                expressionReturnTypes[i] = exprEvaluators[i].ReturnType;
	            }

	            int count;
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
	                var numStreamColumnsJoin = 0;
	                if (_isUsingWildcard && _typeService.EventTypes.Length > 1)
	                {
	                    numStreamColumnsJoin = _typeService.EventTypes.Length;
	                }
	                columnNames = new string[_selectionList.Count + namedStreams.Count + numStreamColumnsJoin];
	                columnNamesAsProvided = new string[columnNames.Length];
	                count = 0;
	                foreach (var aSelectionList in _selectionList)
	                {
	                    columnNames[count] = aSelectionList.AssignedName;
	                    columnNamesAsProvided[count] = aSelectionList.ProvidedName;
	                    count++;
	                }
	                foreach (var aSelectionList in namedStreams)
	                {
	                    columnNames[count] = aSelectionList.OptionalName;
	                    columnNamesAsProvided[count] = aSelectionList.OptionalName;
	                    count++;
	                }
	                // for wildcard joins, add the streams themselves
	                if (_isUsingWildcard && _typeService.EventTypes.Length > 1)
	                {
	                    foreach (var streamName in _typeService.StreamNames)
	                    {
	                        columnNames[count] = streamName;
	                        columnNamesAsProvided[count] = streamName;
	                        count++;
	                    }
	                }
	            }
	            else // handle regular column names
	            {
	                columnNames = new string[_selectionList.Count];
	                columnNamesAsProvided = new string[_selectionList.Count];
	                for (var i = 0; i < _selectionList.Count; i++)
	                {
	                    columnNames[i] = _selectionList[i].AssignedName;
	                    columnNamesAsProvided[i] = _selectionList[i].ProvidedName;
	                }
	            }

	            // Find if there is any fragment event types:
	            // This is a special case for fragments: select a, b from pattern [a=A -> b=B]
	            // We'd like to maintain 'A' and 'B' EventType in the Map type, and 'a' and 'b' EventBeans in the event bean
	            for (var i = 0; i < _selectionList.Count; i++)
	            {
	                if (!(exprNodes[i] is ExprIdentNode))
	                {
	                    continue;
	                }

	                var identNode = (ExprIdentNode) exprNodes[i];
	                var propertyName = identNode.ResolvedPropertyName;
	                var streamNum = identNode.StreamId;

	                var eventTypeStream = _typeService.EventTypes[streamNum];
	                if (eventTypeStream is NativeEventType)
	                {
	                    continue; // we do not transpose the native type for performance reasons
	                }

	                var fragmentType = eventTypeStream.GetFragmentType(propertyName);
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
	                    (ReferenceEquals(fragmentType.FragmentType.UnderlyingType, expressionReturnTypes[i])) &&
	                    ((targetFragment == null) || (targetFragment != null && targetFragment.IsNative)))
	                {
	                    ExprEvaluator evaluatorFragment;

	                    // A match was found, we replace the expression
	                    var getter = eventTypeStream.GetGetter(propertyName);
	                    var returnType = eventTypeStream.GetPropertyType(propertyName);
	                    evaluatorFragment = new ProxyExprEvaluator
	                    {
	                        ProcEvaluate = args =>
	                        {
	                            EventBean streamEvent = args.EventsPerStream[streamNum];
	                            if (streamEvent == null)
	                            {
	                                return null;
	                            }
	                            return getter.Get(streamEvent);
	                        },
	                        ProcReturnType = () => { return returnType; },
	                    };
	                    exprEvaluators[i] = evaluatorFragment;
	                }
	                    // same for arrays: may need to unwrap the fragment if the target type has this underlying type
	                else if ((insertIntoTargetType != null) && expressionReturnTypes[i] is Type &&
	                         (fragmentType.FragmentType.UnderlyingType == ((Type) expressionReturnTypes[i]).GetElementType()) &&
	                         ((targetFragment == null) || (targetFragment != null && targetFragment.IsNative)))
	                {
	                    ExprEvaluator evaluatorFragment;
	                    var getter = eventTypeStream.GetGetter(propertyName);
	                    var returnType = TypeHelper.GetArrayType(eventTypeStream.GetPropertyType(propertyName));
	                    evaluatorFragment = new ProxyExprEvaluator
	                    {
	                        ProcEvaluate = args =>
	                        {
	                            EventBean streamEvent = args.EventsPerStream[streamNum];
	                            if (streamEvent == null)
	                            {
	                                return null;
	                            }
	                            return getter.Get(streamEvent);
	                        },
	                        ProcReturnType = () => { return returnType; },
	                    };
	                    exprEvaluators[i] = evaluatorFragment;
	                }
	                else
	                {
	                    ExprEvaluator evaluatorFragment;
	                    var getter = eventTypeStream.GetGetter(propertyName);
	                    var fragType = eventTypeStream.GetFragmentType(propertyName);
	                    var undType = fragType.FragmentType.UnderlyingType;
	                    var returnType = fragType.IsIndexed ? TypeHelper.GetArrayType(undType) : undType;

	                    // A match was found, we replace the expression
	                    evaluatorFragment = new ProxyExprEvaluator
	                    {
	                        ProcEvaluate = args =>
	                        {
	                            EventBean streamEvent = args.EventsPerStream[streamNum];
	                            if (streamEvent == null)
	                            {
	                                return null;
	                            }
	                            return getter.GetFragment(streamEvent);
	                        },
	                        ProcReturnType = () => { return returnType; },
	                    };

	                    exprEvaluators[i] = evaluatorFragment;
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
	            for (var i = 0; i < _selectionList.Count; i++)
	            {
	                var pair = HandleUnderlyingStreamInsert(exprEvaluators[i], _namedWindowService, _eventAdapterService);
	                if (pair != null)
	                {
	                    exprEvaluators[i] = pair.First;
	                    expressionReturnTypes[i] = pair.Second;
	                }
	            }

	            // Build event type that reflects all selected properties
	            IDictionary<string, object> selPropertyTypes = new LinkedHashMap<string, object>();
	            count = 0;
	            for (var i = 0; i < exprEvaluators.Length; i++)
	            {
	                var expressionReturnType = expressionReturnTypes[count];
	                selPropertyTypes.Put(columnNames[count], expressionReturnType);
	                count++;
	            }
	            if (!_selectedStreams.IsEmpty())
	            {
	                foreach (var element in namedStreams)
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
	                    for (var i = 0; i < _typeService.EventTypes.Length; i++)
	                    {
	                        var eventTypeStream = _typeService.EventTypes[i];
	                        selPropertyTypes.Put(columnNames[count], eventTypeStream);
	                        count++;
	                    }
	                }
	            }

	            // Handle stream selection
	            EventType underlyingEventType = null;
	            var underlyingStreamNumber = 0;
	            var underlyingIsFragmentEvent = false;
	            EventPropertyGetter underlyingPropertyEventGetter = null;
	            ExprEvaluator underlyingExprEvaluator = null;
	            var useMapOutput = EventRepresentationUtil.IsMap(_annotations, _configuration, AssignedType.NONE);

	            if (!_selectedStreams.IsEmpty())
	            {
	                // Resolve underlying event type in the case of wildcard or non-named stream select.
	                // Determine if the we are considering a tagged event or a stream name.
	                if ((_isUsingWildcard) || (!unnamedStreams.IsEmpty()))
	                {
	                    if (!unnamedStreams.IsEmpty())
	                    {
	                        if (unnamedStreams[0].StreamSelected != null)
	                        {
	                            var streamSpec = unnamedStreams[0].StreamSelected;

	                            // the tag.* syntax for :  select tag.* from pattern [tag = A]
	                            underlyingStreamNumber = streamSpec.StreamNumber;
	                            if (streamSpec.IsFragmentEvent)
	                            {
	                                var compositeMap = _typeService.EventTypes[underlyingStreamNumber];
	                                var fragment = compositeMap.GetFragmentType(streamSpec.StreamName);
	                                underlyingEventType = fragment.FragmentType;
	                                underlyingIsFragmentEvent = true;
	                            }
	                                // the property.* syntax for :  select property.* from A
	                            else if (streamSpec.IsProperty)
	                            {
	                                var propertyName = streamSpec.StreamName;
	                                var propertyType = streamSpec.PropertyType;
	                                var streamNumber = streamSpec.StreamNumber;

	                                if (TypeHelper.IsBuiltinDataType(streamSpec.PropertyType))
	                                {
	                                    throw new ExprValidationException(
	                                        "The property wildcard syntax cannot be used on built-in types as returned by property '" +
	                                        propertyName + "'");
	                                }

	                                // create or get an underlying type for that Class
	                                underlyingEventType = _eventAdapterService.AddBeanType(
	                                    propertyType.Name, propertyType, false, false, false);
	                                _selectExprEventTypeRegistry.Add(underlyingEventType);
	                                underlyingPropertyEventGetter = _typeService.EventTypes[streamNumber].GetGetter(
	                                    propertyName);
	                                if (underlyingPropertyEventGetter == null)
	                                {
	                                    throw new ExprValidationException(
	                                        "Unexpected error resolving property getter for property " + propertyName);
	                                }
	                            }
	                                // the stream.* syntax for:  select a.* from A as a
	                            else
	                            {
	                                underlyingEventType = _typeService.EventTypes[underlyingStreamNumber];
	                            }
	                        }
	                            // handle case where the unnamed stream is a "transpose" function, for non-insert-into
	                        else
	                        {
	                            if (_insertIntoDesc == null || insertIntoTargetType == null)
	                            {
	                                var expression = unnamedStreams[0].ExpressionSelectedAsStream.SelectExpression;
	                                var returnType = expression.ExprEvaluator.ReturnType;
	                                if (returnType == typeof (object[]) || returnType.IsGenericDictionary() ||
	                                    TypeHelper.IsBuiltinDataType(returnType))
	                                {
	                                    throw new ExprValidationException(
	                                        "Invalid expression return type '" + returnType.FullName +
	                                        "' for transpose function");
	                                }
	                                underlyingEventType = _eventAdapterService.AddBeanType(
	                                    returnType.Name, returnType, false, false, false);
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

	            EventType resultEventType;
	            if (_insertIntoDesc == null)
	            {
	                if (!_selectedStreams.IsEmpty())
	                {
	                    if (underlyingEventType != null)
	                    {
	                        var tableMetadata = _tableService.GetTableMetadataFromEventType(underlyingEventType);
	                        if (tableMetadata != null)
	                        {
	                            underlyingEventType = tableMetadata.PublicEventType;
	                        }
	                        resultEventType =
	                            _eventAdapterService.CreateAnonymousWrapperType(
	                                _statementId + "_wrapout_" + CollectionUtil.ToString(_assignedTypeNumberStack, "_"),
	                                underlyingEventType, selPropertyTypes);
	                        return new EvalSelectStreamWUnderlying(
	                            selectExprContext, resultEventType, namedStreams, _isUsingWildcard,
	                            unnamedStreams, singleStreamWrapper, underlyingIsFragmentEvent, underlyingStreamNumber,
	                            underlyingPropertyEventGetter, underlyingExprEvaluator, tableMetadata);
	                    }
	                    else
	                    {
	                        resultEventType =
	                            _eventAdapterService.CreateAnonymousMapType(
	                                _statementId + "_mapout_" + CollectionUtil.ToString(_assignedTypeNumberStack, "_"),
	                                selPropertyTypes);
	                        return new EvalSelectStreamNoUnderlyingMap(
	                            selectExprContext, resultEventType, namedStreams, _isUsingWildcard);
	                    }
	                }

	                if (_isUsingWildcard)
	                {
	                    resultEventType =
	                        _eventAdapterService.CreateAnonymousWrapperType(
	                            _statementId + "_wrapoutwild_" + CollectionUtil.ToString(_assignedTypeNumberStack, "_"),
	                            eventType, selPropertyTypes);
	                    if (singleStreamWrapper)
	                    {
	                        return new EvalSelectWildcardSSWrapper(selectExprContext, resultEventType);
	                    }
	                    if (joinWildcardProcessor == null)
	                    {
	                        return new EvalSelectWildcard(selectExprContext, resultEventType);
	                    }
	                    return new EvalSelectWildcardJoin(selectExprContext, resultEventType, joinWildcardProcessor);
	                }

	                if (!useMapOutput)
	                {
	                    resultEventType =
	                        _eventAdapterService.CreateAnonymousObjectArrayType(
	                            _statementId + "_result_" + CollectionUtil.ToString(_assignedTypeNumberStack, "_"),
	                            selPropertyTypes);
	                }
	                else
	                {
	                    resultEventType =
	                        _eventAdapterService.CreateAnonymousMapType(
	                            _statementId + "_result_" + CollectionUtil.ToString(_assignedTypeNumberStack, "_"),
	                            selPropertyTypes);
	                }
	                if (selectExprContext.ExpressionNodes.Length == 0)
	                {
	                    return new EvalSelectNoWildcardEmptyProps(selectExprContext, resultEventType);
	                }
	                else
	                {
	                    if (!useMapOutput)
	                    {
	                        return new EvalSelectNoWildcardObjectArray(selectExprContext, resultEventType);
	                    }
	                    return new EvalSelectNoWildcardMap(selectExprContext, resultEventType);
	                }
	            }

	            EventType vaeInnerEventType = null;
	            var singleColumnWrapOrBeanCoercion = false;
	                // Additional single-column coercion for non-wrapped type done by SelectExprInsertEventBeanFactory
	            var isRevisionEvent = false;

	            try
	            {
	                if (!_selectedStreams.IsEmpty())
	                {
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
	                        var expression = unnamedStreams[0].ExpressionSelectedAsStream.SelectExpression;
	                        var returnType = expression.ExprEvaluator.ReturnType;
	                        if (insertIntoTargetType is ObjectArrayEventType && returnType == typeof (object[]))
	                        {
	                            return
	                                new SelectExprInsertEventBeanFactory.SelectExprInsertNativeExpressionCoerceObjectArray(
	                                    insertIntoTargetType, expression.ExprEvaluator, _eventAdapterService);
	                        }
	                        else if (insertIntoTargetType is MapEventType && returnType.IsGenericDictionary())
	                        {
	                            return
	                                new SelectExprInsertEventBeanFactory.SelectExprInsertNativeExpressionCoerceMap(
	                                    insertIntoTargetType, expression.ExprEvaluator, _eventAdapterService);
	                        }
	                        else if (insertIntoTargetType is BeanEventType &&
	                                 TypeHelper.IsSubclassOrImplementsInterface(returnType, insertIntoTargetType.UnderlyingType))
	                        {
	                            return
	                                new SelectExprInsertEventBeanFactory.SelectExprInsertNativeExpressionCoerceNative(
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
	                                        "Invalid expression return type '" + evalExprEvaluator.ReturnType +
	                                        "' for transpose function, expected '" + innerType.UnderlyingType.Name + "'");
	                                }
	                                resultEventType = _eventAdapterService.AddWrapperType(
	                                    insertIntoTargetType.Name, existing.UnderlyingEventType, selPropertyTypes, false,
	                                    true);
	                                return new EvalSelectStreamWUnderlying(
	                                    selectExprContext, resultEventType, namedStreams, _isUsingWildcard,
	                                    unnamedStreams, false, false, underlyingStreamNumber, null, evalExprEvaluator, null);
	                            }
	                        }
	                        throw EvalInsertUtil.MakeEventTypeCastException(returnType, insertIntoTargetType);
	                    }

	                    if (underlyingEventType != null)
	                        // a single stream was selected via "stream.*" and there is no column name
	                    {
	                        // recast as a Map-type
	                        if (underlyingEventType is MapEventType && insertIntoTargetType is MapEventType)
	                        {
	                            return EvalSelectStreamWUndRecastMapFactory.Make(
	                                _typeService.EventTypes, selectExprContext, _selectedStreams[0].StreamSelected.StreamNumber,
	                                insertIntoTargetType, exprNodes, _methodResolutionService.EngineImportService);
	                        }

	                        // recast as a Object-array-type
	                        if (underlyingEventType is ObjectArrayEventType && insertIntoTargetType is ObjectArrayEventType)
	                        {
	                            return EvalSelectStreamWUndRecastObjectArrayFactory.Make(
	                                _typeService.EventTypes, selectExprContext, _selectedStreams[0].StreamSelected.StreamNumber,
	                                insertIntoTargetType, exprNodes, _methodResolutionService.EngineImportService);
	                        }

	                        // recast as a Bean-type
	                        if (underlyingEventType is BeanEventType && insertIntoTargetType is BeanEventType)
	                        {
	                            return new EvalInsertBeanRecast(
	                                insertIntoTargetType, _eventAdapterService, _selectedStreams[0].StreamSelected.StreamNumber,
	                                _typeService.EventTypes);
	                        }

	                        // wrap if no recast possible
	                        var tableMetadata = _tableService.GetTableMetadataFromEventType(underlyingEventType);
	                        if (tableMetadata != null)
	                        {
	                            underlyingEventType = tableMetadata.PublicEventType;
	                        }
	                        resultEventType = _eventAdapterService.AddWrapperType(
	                            _insertIntoDesc.EventTypeName, underlyingEventType, selPropertyTypes, false, true);
	                        return new EvalSelectStreamWUnderlying(
	                            selectExprContext, resultEventType, namedStreams, _isUsingWildcard,
	                            unnamedStreams, singleStreamWrapper, underlyingIsFragmentEvent, underlyingStreamNumber,
	                            underlyingPropertyEventGetter, underlyingExprEvaluator, tableMetadata);
	                    }
	                    else // there are one or more streams selected with column name such as "stream.* as columnOne"
	                    {
	                        if (insertIntoTargetType is BeanEventType)
	                        {
	                            string name = _selectedStreams[0].StreamSelected.StreamName;
	                            string alias = _selectedStreams[0].StreamSelected.OptionalName;
	                            var syntaxUsed = name + ".*" + (alias != null ? " as " + alias : "");
	                            var syntaxInstead = name + (alias != null ? " as " + alias : "");
	                            throw new ExprValidationException(
	                                "The '" + syntaxUsed +
	                                "' syntax is not allowed when inserting into an existing bean event type, use the '" +
	                                syntaxInstead + "' syntax instead");
	                        }
	                        if (insertIntoTargetType == null || insertIntoTargetType is MapEventType)
	                        {
	                            resultEventType = _eventAdapterService.AddNestableMapType(
	                                _insertIntoDesc.EventTypeName, selPropertyTypes, null, false, false, false, false, true);
	                            var propertiesToUnwrap = GetEventBeanToObjectProps(selPropertyTypes, resultEventType);
	                            if (propertiesToUnwrap.IsEmpty())
	                            {
	                                return new EvalSelectStreamNoUnderlyingMap(
	                                    selectExprContext, resultEventType, namedStreams, _isUsingWildcard);
	                            }
	                            else
	                            {
	                                return new EvalSelectStreamNoUndWEventBeanToObj(
	                                    selectExprContext, resultEventType, namedStreams, _isUsingWildcard, propertiesToUnwrap);
	                            }
	                        }
	                        else
	                        {
	                            var propertiesToUnwrap = GetEventBeanToObjectProps(selPropertyTypes, insertIntoTargetType);
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
	                    }
	                }

	                var vaeProcessor = _valueAddEventService.GetValueAddProcessor(_insertIntoDesc.EventTypeName);
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
	                                    var msg = BaseNestableEventType.IsDeepEqualsProperties(
	                                        eventType.Name, source.Types, target.Types);
	                                    if (msg == null)
	                                    {
	                                        return new EvalInsertCoercionObjectArray(insertIntoTargetType, _eventAdapterService);
	                                    }
	                                }
	                                if (insertIntoTargetType is MapEventType && eventType is MapEventType)
	                                {
	                                    return new EvalInsertCoercionMap(insertIntoTargetType, _eventAdapterService);
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
	                            var existingTypeProcessor =
	                                SelectExprInsertEventBeanFactory.GetInsertUnderlyingNonJoin(
	                                    _eventAdapterService, insertIntoTargetType, _isUsingWildcard, _typeService,
	                                    exprEvaluators, columnNames, expressionReturnTypes,
	                                    _methodResolutionService.EngineImportService, _insertIntoDesc, columnNamesAsProvided,
	                                    true);
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
	                            return new EvalInsertWildcardSSWrapperRevision(selectExprContext, resultEventType, vaeProcessor);
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
	                                var wrappingEventType =
	                                    _eventAdapterService.AddWrapperType(
	                                        _insertIntoDesc.EventTypeName + "_wrapped", eventType, selPropertyTypes, false, true);
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
	                        var columnOneType = expressionReturnTypes[0];
	                        if (insertIntoTargetType is WrapperEventType)
	                        {
	                            var wrapperType = (WrapperEventType) insertIntoTargetType;
	                            // Map and Object both supported
	                            if (wrapperType.UnderlyingEventType.UnderlyingType == columnOneType)
	                            {
	                                singleColumnWrapOrBeanCoercion = true;
	                                resultEventType = insertIntoTargetType;
	                            }
	                        }
	                        if ((insertIntoTargetType is BeanEventType) && (columnOneType is Type))
	                        {
	                            var beanType = (BeanEventType) insertIntoTargetType;
	                            // Map and Object both supported
	                            if (TypeHelper.IsSubclassOrImplementsInterface((Type) columnOneType, beanType.UnderlyingType))
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
	                                return new EvalInsertNoWildcardSingleColCoercionMapWrap(selectExprContext, resultEventType);
	                            }
	                            else if (wrapper.UnderlyingEventType is ObjectArrayEventType)
	                            {
	                                return new EvalInsertNoWildcardSingleColCoercionObjectArrayWrap(
	                                    selectExprContext, resultEventType);
	                            }
	                            else if (wrapper.UnderlyingEventType is VariantEventType)
	                            {
	                                var variantEventType = (VariantEventType) wrapper.UnderlyingEventType;
	                                vaeProcessor = _valueAddEventService.GetValueAddProcessor(variantEventType.Name);
	                                return new EvalInsertNoWildcardSingleColCoercionBeanWrapVariant(
	                                    selectExprContext, resultEventType, vaeProcessor);
	                            }
	                            else
	                            {
	                                return new EvalInsertNoWildcardSingleColCoercionBeanWrap(selectExprContext, resultEventType);
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
	                        if (resultEventType is MapEventType)
	                        {
	                            return new EvalInsertNoWildcardSingleColCoercionRevisionMap(
	                                selectExprContext, resultEventType, vaeProcessor, vaeInnerEventType);
	                        }
	                        else if (resultEventType is ObjectArrayEventType)
	                        {
	                            return new EvalInsertNoWildcardSingleColCoercionRevisionObjectArray(
	                                selectExprContext, resultEventType, vaeProcessor, vaeInnerEventType);
	                        }
	                        else if (resultEventType is BeanEventType)
	                        {
	                            return new EvalInsertNoWildcardSingleColCoercionRevisionBean(
	                                selectExprContext, resultEventType, vaeProcessor, vaeInnerEventType);
	                        }
	                        else
	                        {
	                            return new EvalInsertNoWildcardSingleColCoercionRevisionBeanWrap(
	                                selectExprContext, resultEventType, vaeProcessor, vaeInnerEventType);
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
	                                    selPropertyTypes);
	                        }
	                        else
	                        {
	                            var statementName = "stmt_" + _statementId + "_insert";
	                            resultEventType = _eventAdapterService.AddNestableMapType(
	                                statementName, selPropertyTypes, null, false, false, false, false, true);
	                        }
	                    }
	                    else
	                    {
	                        var existingType = insertIntoTargetType;
	                        if (existingType == null)
	                        {
	                            // The type may however be an auto-import or fully-qualified class name
	                            Type clazz = null;
	                            try
	                            {
	                                clazz = _methodResolutionService.ResolveType(_insertIntoDesc.EventTypeName);
	                            }
	                            catch (EngineImportException e)
	                            {
	                                Log.Debug(
	                                    "Target stream name '" + _insertIntoDesc.EventTypeName +
	                                    "' is not resolved as a class name");
	                            }
	                            if (clazz != null)
	                            {
	                                existingType = _eventAdapterService.AddBeanType(clazz.Name, clazz, false, false, false);
	                            }
	                        }

	                        SelectExprProcessor selectExprInsertEventBean = null;
	                        if (existingType != null)
	                        {
	                            selectExprInsertEventBean =
	                                SelectExprInsertEventBeanFactory.GetInsertUnderlyingNonJoin(
	                                    _eventAdapterService, existingType, _isUsingWildcard, _typeService, exprEvaluators,
	                                    columnNames, expressionReturnTypes, _methodResolutionService.EngineImportService,
	                                    _insertIntoDesc, columnNamesAsProvided, false);
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
	                            else
	                            {
	                                var useMap = EventRepresentationUtil.IsMap(_annotations, _configuration, AssignedType.NONE);
	                                if (useMap)
	                                {
	                                    resultEventType = _eventAdapterService.AddNestableMapType(
	                                        _insertIntoDesc.EventTypeName, selPropertyTypes, null, false, false, false, false,
	                                        true);
	                                }
	                                else
	                                {
	                                    resultEventType =
	                                        _eventAdapterService.AddNestableObjectArrayType(
	                                            _insertIntoDesc.EventTypeName, selPropertyTypes, null, false, false, false,
	                                            false, true, false, null);
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
	                    else
	                    {
	                        return MakeObjectArrayConsiderReorder(selectExprContext, (ObjectArrayEventType) resultEventType);
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
	    }

	    private SelectExprProcessor MakeObjectArrayConsiderReorder(SelectExprContext selectExprContext, ObjectArrayEventType resultEventType)

	    {
	        // for single-property it is allowed to insert into that property directly, but not for tables
	        if (selectExprContext.ColumnNames.Length == 1 &&
	            CollectionUtil.FindItem(resultEventType.PropertyNames, selectExprContext.ColumnNames[0]) == -1 &&
	            _tableService.GetTableMetadataFromEventType(resultEventType) == null){
	            return new EvalInsertNoWildcardObjectArray(selectExprContext, resultEventType);
	        }

	        var wideners = new TypeWidener[selectExprContext.ColumnNames.Length];
	        var remapped = new int[selectExprContext.ColumnNames.Length];
	        var needRemap = false;
	        for (var i = 0; i < selectExprContext.ColumnNames.Length; i++) {
	            var colName = selectExprContext.ColumnNames[i];
	            var index = CollectionUtil.FindItem(resultEventType.PropertyNames, colName);
	            if (index == -1) {
	                throw new ExprValidationException("Could not find property '" + colName + "' in " + GetTypeNameConsiderTable(resultEventType, _tableService));
	            }
	            remapped[i] = index;
	            if (index != i) {
	                needRemap = true;
	            }
	            Type sourceColumnType = selectExprContext.ExpressionNodes[i].ReturnType;
	            var targetPropType = resultEventType.GetPropertyType(colName);
	            wideners[i] = TypeWidenerFactory.GetCheckPropertyAssignType(colName, sourceColumnType, targetPropType, colName);
	        }

	        if (!needRemap) {
	            return new EvalInsertNoWildcardObjectArray(selectExprContext, resultEventType);
	        }
	        if (CollectionUtil.IsAllNullArray(wideners)) {
	            return new EvalInsertNoWildcardObjectArrayRemap(selectExprContext, resultEventType, remapped);
	        }
	        return new EvalInsertNoWildcardObjectArrayRemapWWiden(selectExprContext, resultEventType, remapped, wideners);
	    }

	    private string GetTypeNameConsiderTable(ObjectArrayEventType resultEventType, TableService tableService) {
	        var metadata = tableService.GetTableMetadataFromEventType(resultEventType);
	        if (metadata != null) {
	            return "table '" + metadata.TableName + "'";
	        }
	        return "type '" + resultEventType.Name + "'";
	    }

	    private Pair<ExprEvaluator, object> HandleUnderlyingStreamInsert(ExprEvaluator exprEvaluator, NamedWindowService namedWindowService, EventAdapterService eventAdapterService) {
	        if (!(exprEvaluator is ExprStreamUnderlyingNode)) {
	            return null;
	        }
	        var undNode = (ExprStreamUnderlyingNode) exprEvaluator;
	        var streamNum = undNode.StreamId;
	        Type returnType = undNode.ExprEvaluator.ReturnType;
	        var namedWindowAsType = GetNamedWindowUnderlyingType(namedWindowService, eventAdapterService, _typeService.EventTypes[streamNum]);
	        var tableMetadata = _tableService.GetTableMetadataFromEventType(_typeService.EventTypes[streamNum]);

	        EventType eventTypeStream;
	        ExprEvaluator evaluator;
	        if (tableMetadata != null) {
	            eventTypeStream = tableMetadata.PublicEventType;
	            evaluator = new ProxyExprEvaluator
	            {
	                ProcEvaluate = args =>  {
	                    if (InstrumentationHelper.ENABLED) {
	                        InstrumentationHelper.Get().QExprStreamUndSelectClause(undNode);
	                    }
	                    EventBean @event = args.EventsPerStream == null ? null : args.EventsPerStream[streamNum];
	                    if (@event != null) {
	                        @event = tableMetadata.EventToPublic.Convert(@event, args.EventsPerStream, args.IsNewData, args.ExprEvaluatorContext);
	                    }
	                    if (InstrumentationHelper.ENABLED) {
	                        InstrumentationHelper.Get().AExprStreamUndSelectClause(@event);
	                    }
	                    return @event;
	                },

	                ProcReturnType = () =>  {
	                    return returnType;
	                },
	            };
	        }
	        else if (namedWindowAsType == null) {
	            eventTypeStream = _typeService.EventTypes[streamNum];
	            evaluator = new ProxyExprEvaluator
	            {
	                ProcEvaluate = args =>  {
	                    if (InstrumentationHelper.ENABLED) {
	                        InstrumentationHelper.Get().QExprStreamUndSelectClause(undNode);
	                        EventBean @event = args.EventsPerStream == null ? null : args.EventsPerStream[streamNum];
	                        InstrumentationHelper.Get().AExprStreamUndSelectClause(@event);
	                        return @event;
	                    }
	                    return args.EventsPerStream == null ? null : args.EventsPerStream[streamNum];
	                },

	                ProcReturnType = () =>  {
	                    return returnType;
	                },
	            };
	        }
	        else {
	            eventTypeStream = namedWindowAsType;
	            evaluator = new ProxyExprEvaluator
	            {
	                ProcEvaluate = args =>  {
	                    EventBean @event = args.EventsPerStream[streamNum];
	                    if (@event == null) {
	                        return null;
	                    }
	                    return eventAdapterService.AdapterForType(@event.Underlying, namedWindowAsType);
	                },

	                ProcReturnType = () =>  {
	                    return returnType;
	                },
	            };
	        }

	        return new Pair<ExprEvaluator, object>(evaluator, eventTypeStream);
	    }

	    private EventType GetNamedWindowUnderlyingType(NamedWindowService namedWindowService, EventAdapterService eventAdapterService, EventType eventType)
        {
	        if (!namedWindowService.IsNamedWindow(eventType.Name)) {
	            return null;
	        }
	        var processor = namedWindowService.GetProcessor(eventType.Name);
	        if (processor.GetEventTypeAsName() == null) {
	            return null;
	        }
	        return eventAdapterService.GetEventTypeByName(processor.GetEventTypeAsName());
	    }

	    private static EPType[] DetermineInsertedEventTypeTargets(EventType targetType, IList<SelectClauseExprCompiledSpec> selectionList)
        {
	        var targets = new EPType[selectionList.Count];
	        if (targetType == null) {
	            return targets;
	        }

	        for (var i = 0; i < selectionList.Count; i++) {
	            var expr = selectionList[i];
	            if (expr.ProvidedName == null) {
	                continue;
	            }

	            var desc = targetType.GetPropertyDescriptor(expr.ProvidedName);
	            if (desc == null) {
	                continue;
	            }

	            if (!desc.IsFragment) {
	                continue;
	            }

	            var fragmentEventType = targetType.GetFragmentType(expr.ProvidedName);
	            if (fragmentEventType == null) {
	                continue;
	            }

	            if (fragmentEventType.IsIndexed) {
	                targets[i] = EPTypeHelper.CollectionOfEvents(fragmentEventType.FragmentType);
	            }
	            else {
	                targets[i] = EPTypeHelper.SingleEvent(fragmentEventType.FragmentType);
	            }
	        }

	        return targets;
	    }

	    private TypeAndFunctionPair HandleTypableExpression(ExprEvaluator exprEvaluator, int expressionNum)

	    {
	        if (!(exprEvaluator is ExprEvaluatorTypableReturn)) {
	            return null;
	        }

	        var typable = (ExprEvaluatorTypableReturn) exprEvaluator;
	        var eventTypeExpr = typable.RowProperties;
	        if (eventTypeExpr == null) {
	            return null;
	        }

	        var mapType = _eventAdapterService.CreateAnonymousMapType(_statementId + "_innereval_" + CollectionUtil.ToString(_assignedTypeNumberStack, "_") + "_" + expressionNum, eventTypeExpr);
	        var innerEvaluator = exprEvaluator;
	        ExprEvaluator evaluatorFragment = new ProxyExprEvaluator
	        {
	            ProcEvaluate = args =>
	            {
	                var values = (IDictionary<string, object>) innerEvaluator.Evaluate(args);
	                if (values == null) {
	                    return _eventAdapterService.AdapterForTypedMap(Collections.GetEmptyMap<string, object>(), mapType);
	                }
	                return _eventAdapterService.AdapterForTypedMap(values, mapType);
	            },
	            ProcReturnType = () =>
	            {
	                return typeof(IDictionary<string, object>);
	            },
	        };

	        return new TypeAndFunctionPair(mapType, evaluatorFragment);
	    }

	    private TypeAndFunctionPair HandleInsertIntoEnumeration(string insertIntoColName, EPType insertIntoTarget, ExprEvaluator exprEvaluator, EngineImportService engineImportService)
	    {
	        if (!(exprEvaluator is ExprEvaluatorEnumeration) || insertIntoTarget == null
	                || (!EPTypeHelper.IsCarryEvent(insertIntoTarget))) {
	            return null;
	        }

	        var enumeration = (ExprEvaluatorEnumeration) exprEvaluator;
	        var eventTypeSingle = enumeration.GetEventTypeSingle(_eventAdapterService, _statementId);
	        var eventTypeColl = enumeration.GetEventTypeCollection(_eventAdapterService, _statementId);
	        var sourceType = eventTypeSingle ?? eventTypeColl;
	        if (eventTypeColl == null && eventTypeSingle == null) {
	            return null;    // enumeration is untyped events (select-clause provided to subquery or 'new' operator)
	        }
	        if (((EventTypeSPI)sourceType).Metadata.TypeClass == TypeClass.ANONYMOUS) {
	            return null;    // we don't allow anonymous types here, thus excluding subquery multi-column selection
	        }

	        // check type info
	        var targetType = EPTypeHelper.GetEventType(insertIntoTarget);
	        CheckTypeCompatible(insertIntoColName, targetType, sourceType);

            ExprEvaluator evaluatorFragment;

	        // handle collection target - produce EventBean[]
	        if (insertIntoTarget is EventMultiValuedEPType) {
	            if (eventTypeColl != null) {
	                evaluatorFragment = new ProxyExprEvaluator
	                {
	                    ProcEvaluate = args =>  {
	                        var events = enumeration.EvaluateGetROCollectionEvents(
                                args.EventsPerStream,
                                args.IsNewData,
                                args.ExprEvaluatorContext);
	                        if (events == null) {
	                            return null;
	                        }
	                        return events.ToArray();
	                    },
	                    ProcReturnType = () =>  {
	                        return TypeHelper.GetArrayType(targetType.UnderlyingType);
	                    },
	                };
	                return new TypeAndFunctionPair(new EventType[] {targetType}, evaluatorFragment);
	            }
	            evaluatorFragment = new ProxyExprEvaluator
	            {
	                ProcEvaluate = args =>  {
                        var @event = enumeration.EvaluateGetEventBean(
                            args.EventsPerStream,
                            args.IsNewData,
                            args.ExprEvaluatorContext);
	                    if (@event == null) {
	                        return null;
	                    }
	                    return new EventBean[] {@event};
	                },
	                ProcReturnType = () =>  {
	                    return TypeHelper.GetArrayType(targetType.UnderlyingType);
	                },
	            };
	            return new TypeAndFunctionPair(new EventType[] {targetType}, evaluatorFragment);
	        }

	        // handle single-bean target
	        // handle single-source
	        if (eventTypeSingle != null) {
	            evaluatorFragment = new ProxyExprEvaluator
	            {
	                ProcEvaluate = args =>  {
	                    return enumeration.EvaluateGetEventBean(
                            args.EventsPerStream,
                            args.IsNewData,
                            args.ExprEvaluatorContext);
	                },
	                ProcReturnType = () =>  {
	                    return targetType.UnderlyingType;
	                },
	            };
	            return new TypeAndFunctionPair(targetType, evaluatorFragment);
	        }

	        // handle collection-source by taking the first
	        evaluatorFragment = new ProxyExprEvaluator
	        {
	            ProcEvaluate = args =>  {
	                var events = enumeration.EvaluateGetROCollectionEvents(
                        args.EventsPerStream,
                        args.IsNewData,
                        args.ExprEvaluatorContext);
	                if (events == null || events.Count == 0) {
	                    return null;
	                }
	                return EventBeanUtility.GetNonemptyFirstEvent(events);
	            },
	            ProcReturnType = () =>  {
	                return targetType.UnderlyingType;
	            },
	        };
	        return new TypeAndFunctionPair(targetType, evaluatorFragment);
	    }

	    private void CheckTypeCompatible(string insertIntoCol, EventType targetType, EventType selectedType)
	            {
	        if (!EventTypeUtility.IsTypeOrSubTypeOf(targetType, selectedType)) {
	            throw new ExprValidationException(
	                    "Incompatible type detected attempting to insert into column '" +
	                            insertIntoCol + "' type '" + targetType.Name + "' compared to selected type '" + selectedType.Name + "'");
	        }
	    }

	    private TypeAndFunctionPair HandleInsertIntoTypableExpression(EPType insertIntoTarget, ExprEvaluator exprEvaluator, EngineImportService engineImportService)
	    {
	        if (!(exprEvaluator is ExprEvaluatorTypableReturn)
	                || insertIntoTarget == null
	                || (!EPTypeHelper.IsCarryEvent(insertIntoTarget))) {
	            return null;
	        }

	        var targetType = EPTypeHelper.GetEventType(insertIntoTarget);
	        var typable = (ExprEvaluatorTypableReturn) exprEvaluator;
	        if (typable.IsMultirow == null) { // not typable after all
	            return null;
	        }
	        var eventTypeExpr = typable.RowProperties;
	        if (eventTypeExpr == null) {
	            return null;
	        }

	        ICollection<WriteablePropertyDescriptor> writables = _eventAdapterService.GetWriteableProperties(targetType, false);
	        IList<WriteablePropertyDescriptor> written = new List<WriteablePropertyDescriptor>();
	        IList<KeyValuePair<string, object>> writtenOffered = new List<KeyValuePair<string, object>>();

	        // from Map<String, Object> determine properties and type widening that may be required
	        foreach (KeyValuePair<string, object> offeredProperty in eventTypeExpr) {
	            var writable = EventTypeUtility.FindWritable(offeredProperty.Key, writables);
	            if (writable == null) {
	                throw new ExprValidationException("Failed to find property '" + offeredProperty.Key + "' among properties for target event type '" + targetType.Name + "'");
	            }
	            written.Add(writable);
	            writtenOffered.Add(offeredProperty);
	        }

	        // determine widening and column type compatibility
	        var wideners = new TypeWidener[written.Count];
	        for (var i = 0; i < written.Count; i++) {
	            var expected = written[i].PropertyType;
	            var provided = writtenOffered[i];
	            if (provided.Value is Type) {
	                wideners[i] = TypeWidenerFactory.GetCheckPropertyAssignType(provided.Key, (Type) provided.Value,
	                        expected, written[i].PropertyName);
	            }
	        }
	        var hasWideners = !CollectionUtil.IsAllNullArray(wideners);

	        // obtain factory
	        WriteablePropertyDescriptor[] writtenArray = written.ToArray();
	        EventBeanManufacturer manufacturer;
	        try {
	            manufacturer = _eventAdapterService.GetManufacturer(targetType, writtenArray, engineImportService, false);
	        }
	        catch (EventBeanManufactureException e) {
	            throw new ExprValidationException("Failed to obtain eventbean factory: " + e.Message, e);
	        }

            ExprEvaluator evaluatorFragment;

	        // handle collection
	        var factory = manufacturer;
	        if (insertIntoTarget is EventMultiValuedEPType && typable.IsMultirow.Value) {
	            evaluatorFragment = new ProxyExprEvaluator
	            {
	                ProcEvaluate = args =>
	                {
	                    var rows = typable.EvaluateTypableMulti(
                            args.EventsPerStream,
                            args.IsNewData,
                            args.ExprEvaluatorContext);
	                    if (rows == null) {
	                        return null;
	                    }
	                    if (rows.Length == 0) {
	                        return new EventBean[0];
	                    }
	                    if (hasWideners) {
	                        ApplyWideners(rows, wideners);
	                    }
	                    var events = new EventBean[rows.Length];
	                    for (var i = 0; i < events.Length; i++) {
	                        events[i] = factory.Make(rows[i]);
	                    }
	                    return events;
	                },
	                ProcReturnType = () =>
	                {
	                    return TypeHelper.GetArrayType(targetType.UnderlyingType);
	                },
	            };

	            return new TypeAndFunctionPair(new EventType[] {targetType}, evaluatorFragment);
	        }
	        else if (insertIntoTarget is EventMultiValuedEPType && !typable.IsMultirow.Value) {
	            evaluatorFragment = new ProxyExprEvaluator
	            {
	                ProcEvaluate = args =>
	                {
	                    var row = typable.EvaluateTypableSingle(
                            args.EventsPerStream,
                            args.IsNewData,
                            args.ExprEvaluatorContext);
	                    if (row == null) {
	                        return null;
	                    }
	                    if (hasWideners) {
	                        ApplyWideners(row, wideners);
	                    }
	                    return new EventBean[] {factory.Make(row)};
	                },
	                ProcReturnType = () =>
	                {
	                    return TypeHelper.GetArrayType(targetType.UnderlyingType);
	                },
	            };
	            return new TypeAndFunctionPair(new EventType[] {targetType}, evaluatorFragment);
	        }
	        else if (insertIntoTarget is EventEPType && !typable.IsMultirow.Value) {
	            evaluatorFragment = new ProxyExprEvaluator
	            {
	                ProcEvaluate = args =>
	                {
	                    var row = typable.EvaluateTypableSingle(
                            args.EventsPerStream,
                            args.IsNewData,
                            args.ExprEvaluatorContext);
	                    if (row == null) {
	                        return null;
	                    }
	                    if (hasWideners) {
	                        ApplyWideners(row, wideners);
	                    }
	                    return factory.Make(row);
	                },
	                ProcReturnType = () =>
	                {
	                    return TypeHelper.GetArrayType(targetType.UnderlyingType);
	                },
	            };
	            return new TypeAndFunctionPair(targetType, evaluatorFragment);
	        }

	        // we are discarding all but the first row
	        evaluatorFragment = new ProxyExprEvaluator
	        {
	            ProcEvaluate = args =>
	            {
	                var rows = typable.EvaluateTypableMulti(
                        args.EventsPerStream, 
                        args.IsNewData, 
                        args.ExprEvaluatorContext);
	                if (rows == null) {
	                    return null;
	                }
	                if (rows.Length == 0) {
	                    return new EventBean[0];
	                }
	                if (hasWideners) {
	                    ApplyWideners(rows[0], wideners);
	                }
	                return factory.Make(rows[0]);
	            },
	            ProcReturnType = () =>
	            {
	                return TypeHelper.GetArrayType(targetType.UnderlyingType);
	            },
	        };
	        return new TypeAndFunctionPair(targetType, evaluatorFragment);
	    }

	    private void ApplyWideners(object[] row, TypeWidener[] wideners) {
	        for (var i = 0; i < wideners.Length; i++) {
	            if (wideners[i] != null) {
	                row[i] = wideners[i].Invoke(row[i]);
	            }
	        }
	    }

	    private void ApplyWideners(object[][] rows, TypeWidener[] wideners) {
	        foreach (var row in rows) {
	            ApplyWideners(row, wideners);
	        }
	    }

	    private TypeAndFunctionPair HandleAtEventbeanEnumeration(bool isEventBeans, ExprEvaluator evaluator)

	    {
	        ExprEvaluator evaluatorFragment;

	        if (!(evaluator is ExprEvaluatorEnumeration) || !isEventBeans) {
	            return null;
	        }

	        var enumEval = (ExprEvaluatorEnumeration) evaluator;
	        var eventTypeSingle = enumEval.GetEventTypeSingle(_eventAdapterService, _statementId);
	        if (eventTypeSingle != null) {
	            var tableMetadata = _tableService.GetTableMetadataFromEventType(eventTypeSingle);
	            if (tableMetadata == null) {
	                evaluatorFragment = new ProxyExprEvaluator
	                {
	                    ProcEvaluate = args =>
	                    {
	                        return enumEval.EvaluateGetEventBean(
	                            args.EventsPerStream,
	                            args.IsNewData,
	                            args.ExprEvaluatorContext);
	                    },
	                    ProcReturnType = () => {
	                        return eventTypeSingle.UnderlyingType;
	                    },
	                };
	                return new TypeAndFunctionPair(eventTypeSingle, evaluatorFragment);
	            }
	            evaluatorFragment = new ProxyExprEvaluator
	            {
	                ProcEvaluate = args =>
	                {
	                    var @event = enumEval.EvaluateGetEventBean(
	                        args.EventsPerStream,
	                        args.IsNewData,
	                        args.ExprEvaluatorContext);
	                    if (@event == null) {
	                        return null;
	                    }
	                    return tableMetadata.EventToPublic.Convert(
	                        @event,
                            args.EventsPerStream,
	                        args.IsNewData,
	                        args.ExprEvaluatorContext);
	                },
	                ProcReturnType = () => {
	                    return tableMetadata.PublicEventType.UnderlyingType;
	                },
	            };
	            return new TypeAndFunctionPair(tableMetadata.PublicEventType, evaluatorFragment);
	        }

	        var eventTypeColl = enumEval.GetEventTypeCollection(_eventAdapterService, _statementId);
	        if (eventTypeColl != null) {
	            var tableMetadata = _tableService.GetTableMetadataFromEventType(eventTypeColl);
	            if (tableMetadata == null) {
	                evaluatorFragment = new ProxyExprEvaluator
	                {
	                    ProcEvaluate = args =>  {
	                        // the protocol is EventBean[]
                            object result = enumEval.EvaluateGetROCollectionEvents(
                                args.EventsPerStream,
                                args.IsNewData,
                                args.ExprEvaluatorContext);
	                        if (result != null && result is ICollection<EventBean>) {
	                            var events = (ICollection<EventBean>) result;
	                            return events.ToArray();
	                        }
	                        return result;
	                    },
	                    ProcReturnType = () => {
	                        return TypeHelper.GetArrayType(eventTypeColl.UnderlyingType);
	                    },
	                };
	                return new TypeAndFunctionPair(new EventType[]{eventTypeColl}, evaluatorFragment);
	            }
	            evaluatorFragment = new ProxyExprEvaluator
	            {
	                ProcEvaluate = args =>  {
	                    // the protocol is EventBean[]
	                    object result = enumEval.EvaluateGetROCollectionEvents(
                            args.EventsPerStream,
                            args.IsNewData,
                            args.ExprEvaluatorContext);
	                    if (result == null) {
	                        return null;
	                    }
                        if (result is ICollection<EventBean>)
                        {
	                        var eventsX = (ICollection<EventBean>) result;
	                        var @out = new EventBean[eventsX.Count];
	                        var index = 0;
	                        foreach (var @event in eventsX) {
	                            @out[index++] = tableMetadata.EventToPublic.Convert(
                                    @event,
                                    args.EventsPerStream,
                                    args.IsNewData,
                                    args.ExprEvaluatorContext);
	                        }
	                        return @out;
	                    }
	                    var events = (EventBean[]) result;
	                    for (var i = 0; i < events.Length; i++)
	                    {
	                        events[i] = tableMetadata.EventToPublic.Convert(
	                            events[i],
	                            args.EventsPerStream,
	                            args.IsNewData,
	                            args.ExprEvaluatorContext);
	                    }
	                    return events;
	                },
	                ProcReturnType = () => {
	                    return TypeHelper.GetArrayType(tableMetadata.PublicEventType.UnderlyingType);
	                },
	            };
	            return new TypeAndFunctionPair(new EventType[]{tableMetadata.PublicEventType}, evaluatorFragment);
	        }

	        return null;
	    }

	    // Determine which properties provided by the Map must be downcast from EventBean to Object
	    private static ISet<string> GetEventBeanToObjectProps(IDictionary<string, object> selPropertyTypes, EventType resultEventType)
        {
	        if (!(resultEventType is BaseNestableEventType)) {
	            return Collections.GetEmptySet<string>();
	        }
	        var mapEventType = (BaseNestableEventType) resultEventType;
	        ISet<string> props = null;
	        foreach (KeyValuePair<string, object> entry in selPropertyTypes) {
	            if (entry.Value is BeanEventType && mapEventType.Types.Get(entry.Key) is Type) {
	                if (props == null) {
	                    props = new HashSet<string>();
	                }
	                props.Add(entry.Key);
	            }
	        }
	        if (props == null) {
	            return Collections.GetEmptySet<string>();
	        }
	        return props;
	    }

	    private static void VerifyInsertInto(InsertIntoDesc insertIntoDesc, IList<SelectClauseExprCompiledSpec> selectionList)
	    {
	        // Verify all column names are unique
	        ISet<string> names = new HashSet<string>();
	        foreach (var element in insertIntoDesc.ColumnNames)
	        {
	            if (names.Contains(element))
	            {
	                throw new ExprValidationException("Property name '" + element + "' appears more then once in insert-into clause");
	            }
	            names.Add(element);
	        }

	        // Verify number of columns matches the select clause
	        if ( (!insertIntoDesc.ColumnNames.IsEmpty()) &&
	                (insertIntoDesc.ColumnNames.Count != selectionList.Count) )
	        {
	            throw new ExprValidationException("Number of supplied values in the select or values clause does not match insert-into clause");
	        }
	    }

	    internal class TypeAndFunctionPair
        {
            internal TypeAndFunctionPair(object type, ExprEvaluator function)
            {
	            Type = type;
	            Function = function;
	        }

	        public object Type { get; private set; }

	        public ExprEvaluator Function { get; private set; }
        }
	}
} // end of namespace
