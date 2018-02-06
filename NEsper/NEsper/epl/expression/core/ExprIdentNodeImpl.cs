///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.parse;
using com.espertech.esper.events.property;
using com.espertech.esper.filter;

namespace com.espertech.esper.epl.expression.core
{
	/// <summary>
	/// Represents an stream property identifier in a filter expressiun tree.
	/// </summary>
	[Serializable]
    public class ExprIdentNodeImpl
        : ExprNodeBase
        , ExprIdentNode
	{
	    // select myprop from...        is a simple property, no stream supplied
	    // select s0.myprop from...     is a simple property with a stream supplied, or a nested property (cannot tell until resolved)
	    // select indexed[1] from ...   is a indexed property

	    private readonly string _unresolvedPropertyName;
	    private string _streamOrPropertyName;

	    private string _resolvedStreamName;
	    private string _resolvedPropertyName;

        [NonSerialized]
        private ExprIdentNodeEvaluator _evaluator;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="unresolvedPropertyName">is the event property name in unresolved form, ie. unvalidated against streams</param>
	    public ExprIdentNodeImpl(string unresolvedPropertyName)
	    {
	        if (unresolvedPropertyName == null)
	        {
	            throw new ArgumentException("Property name is null");
	        }
	        _unresolvedPropertyName = unresolvedPropertyName;
	        _streamOrPropertyName = null;
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="unresolvedPropertyName">is the event property name in unresolved form, ie. unvalidated against streams</param>
	    /// <param name="streamOrPropertyName">is the stream name, or if not a valid stream name a possible nested property namein one of the streams.
	    /// </param>
	    public ExprIdentNodeImpl(string unresolvedPropertyName, string streamOrPropertyName)
	    {
	        if (unresolvedPropertyName == null)
	        {
	            throw new ArgumentException("Property name is null");
	        }
	        if (streamOrPropertyName == null)
	        {
	            throw new ArgumentException("Stream (or property name) name is null");
	        }
	        _unresolvedPropertyName = unresolvedPropertyName;
	        _streamOrPropertyName = streamOrPropertyName;
	    }

	    public ExprIdentNodeImpl(EventType eventType, string propertyName, int streamNumber)
        {
	        _unresolvedPropertyName = propertyName;
	        _resolvedPropertyName = propertyName;
	        EventPropertyGetter propertyGetter = eventType.GetGetter(propertyName);
	        if (propertyGetter == null) {
	            throw new ArgumentException("Ident-node constructor could not locate property " + propertyName);
	        }
	        Type propertyType = eventType.GetPropertyType(propertyName);
	        _evaluator = new ExprIdentNodeEvaluatorImpl(streamNumber, propertyGetter, propertyType, this);
	    }

	    public override ExprEvaluator ExprEvaluator
	    {
	        get { return _evaluator; }
	    }

	    /// <summary>
	    /// For unit testing, returns unresolved property name.
	    /// </summary>
	    /// <value>property name</value>
	    public virtual string UnresolvedPropertyName
	    {
	        get { return _unresolvedPropertyName; }
	    }

	    /// <summary>
	    /// For unit testing, returns stream or property name candidate.
	    /// </summary>
	    /// <value>stream name, or property name of a nested property of one of the streams</value>
        public virtual string StreamOrPropertyName
	    {
	        get { return _streamOrPropertyName; }
	        set { _streamOrPropertyName = value; }
	    }

	    /// <summary>
	    /// Returns the unresolved property name in it's complete form, including
	    /// the stream name if there is one.
	    /// </summary>
	    /// <value>property name</value>
	    public string FullUnresolvedName
	    {
	        get
	        {
	            if (_streamOrPropertyName == null)
	            {
	                return _unresolvedPropertyName;
	            }
	            else
	            {
	                return _streamOrPropertyName + "." + _unresolvedPropertyName;
	            }
	        }
	    }

	    public bool IsFilterLookupEligible
	    {
	        get { return _evaluator.StreamNum == 0 && !(_evaluator.IsContextEvaluated); }
	    }

	    public FilterSpecLookupable FilterLookupable
	    {
	        get { return new FilterSpecLookupable(_resolvedPropertyName, _evaluator.Getter, _evaluator.ReturnType, false); }
	    }

	    public override ExprNode Validate(ExprValidationContext validationContext)
	    {
	        // rewrite expression into a table-access expression
	        if (validationContext.StreamTypeService.HasTableTypes) {
	            ExprTableIdentNode tableIdentNode = validationContext.TableService.GetTableIdentNode(validationContext.StreamTypeService, _unresolvedPropertyName, _streamOrPropertyName);
	            if (tableIdentNode != null) {
	                return tableIdentNode;
	            }
	        }

	        string unescapedPropertyName = PropertyParser.UnescapeBacktick(_unresolvedPropertyName);
	        Pair<PropertyResolutionDescriptor, string> propertyInfoPair = ExprIdentNodeUtil.GetTypeFromStream(validationContext.StreamTypeService, unescapedPropertyName, _streamOrPropertyName, false);
	        _resolvedStreamName = propertyInfoPair.Second;
	        int streamNum = propertyInfoPair.First.StreamNum;
	        Type propertyType = propertyInfoPair.First.PropertyType;
	        _resolvedPropertyName = propertyInfoPair.First.PropertyName;
	        EventPropertyGetter propertyGetter;
	        try {
	            propertyGetter = propertyInfoPair.First.StreamEventType.GetGetter(_resolvedPropertyName);
	        }
	        catch (PropertyAccessException ex) {
	            throw new ExprValidationException("Property '" + _unresolvedPropertyName + "' is not valid: " + ex.Message, ex);
	        }

	        if (propertyGetter == null)
	        {
	            throw new ExprValidationException("Property getter returned was invalid for property '" + _unresolvedPropertyName + "'");
	        }

	        var audit = AuditEnum.PROPERTY.GetAudit(validationContext.Annotations);
	        if (audit != null) {
	            _evaluator = new ExprIdentNodeEvaluatorLogging(streamNum, propertyGetter, propertyType, this, _resolvedPropertyName, validationContext.StatementName, validationContext.StreamTypeService.EngineURIQualifier);
	        }
	        else {
	            _evaluator = new ExprIdentNodeEvaluatorImpl(streamNum, propertyGetter, propertyType, this);
	        }

	        // if running in a context, take the property value from context
            if (validationContext.ContextDescriptor != null && !validationContext.IsFilterExpression)
            {
	            EventType fromType = validationContext.StreamTypeService.EventTypes[streamNum];
	            string contextPropertyName = validationContext.ContextDescriptor.ContextPropertyRegistry.GetPartitionContextPropertyName(fromType, _resolvedPropertyName);
	            if (contextPropertyName != null) {
	                EventType contextType = validationContext.ContextDescriptor.ContextPropertyRegistry.ContextEventType;
	                _evaluator = new ExprIdentNodeEvaluatorContext(streamNum, contextType.GetPropertyType(contextPropertyName), contextType.GetGetter(contextPropertyName));
	            }
	        }
	        return null;
	    }

	    public override bool IsConstantResult
	    {
	        get { return false; }
	    }

	    /// <summary>
	    /// Returns stream id supplying the property value.
	    /// </summary>
	    /// <value>stream number</value>
	    public int StreamId
	    {
	        get
	        {
	            if (_evaluator == null)
	            {
	                throw new IllegalStateException("Identifier expression has not been validated");
	            }
	            return _evaluator.StreamNum;
	        }
	    }

	    public int? StreamReferencedIfAny
	    {
	        get { return StreamId; }
	    }

	    public string RootPropertyNameIfAny
	    {
	        get { return ResolvedPropertyNameRoot; }
	    }

	    public virtual Type ReturnType
	    {
	        get
	        {
	            if (_evaluator == null)
	            {
	                throw new IllegalStateException("Identifier expression has not been validated");
	            }
	            return _evaluator.ReturnType;
	        }
	    }

	    /// <summary>
	    /// Returns stream name as resolved by lookup of property in streams.
	    /// </summary>
	    /// <value>stream name</value>
	    public string ResolvedStreamName
	    {
	        get
	        {
	            if (_resolvedStreamName == null)
	            {
	                throw new IllegalStateException("Identifier node has not been validated");
	            }
	            return _resolvedStreamName;
	        }
	    }

	    /// <summary>
	    /// Return property name as resolved by lookup in streams.
	    /// </summary>
	    /// <value>property name</value>
	    public string ResolvedPropertyName
	    {
	        get
	        {
	            if (_resolvedPropertyName == null)
	            {
	                throw new IllegalStateException("Identifier node has not been validated");
	            }
	            return _resolvedPropertyName;
	        }
	    }

	    /// <summary>
	    /// Returns the root of the resolved property name, if any.
	    /// </summary>
	    /// <value>root</value>
	    public string ResolvedPropertyNameRoot
	    {
	        get
	        {
	            if (_resolvedPropertyName == null)
	            {
	                throw new IllegalStateException("Identifier node has not been validated");
	            }
	            if (_resolvedPropertyName.IndexOf('[') != -1)
	            {
	                return _resolvedPropertyName.Substring(0, _resolvedPropertyName.IndexOf('['));
	            }
	            if (_resolvedPropertyName.IndexOf('(') != -1)
	            {
	                return _resolvedPropertyName.Substring(0, _resolvedPropertyName.IndexOf('('));
	            }
	            if (_resolvedPropertyName.IndexOf('.') != -1)
	            {
	                return _resolvedPropertyName.Substring(0, _resolvedPropertyName.IndexOf('.'));
	            }
	            return _resolvedPropertyName;
	        }
	    }

	    public override string ToString()
	    {
	        return "unresolvedPropertyName=" + (_unresolvedPropertyName ?? "null") +
	                " streamOrPropertyName=" + (_streamOrPropertyName ?? "null") +
	                " resolvedPropertyName=" + (_resolvedPropertyName ?? "null");
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
	        ToPrecedenceFreeEPL(writer, _streamOrPropertyName, _unresolvedPropertyName);
	    }

        public static void ToPrecedenceFreeEPL(TextWriter writer, string streamOrPropertyName, string unresolvedPropertyName)
        {
	        if (streamOrPropertyName != null) {
	            writer.Write(ASTUtil.UnescapeDot(streamOrPropertyName));
                writer.Write('.');
	        }
	        writer.Write(ASTUtil.UnescapeDot(PropertyParser.UnescapeBacktick(unresolvedPropertyName)));
	    }

	    public override ExprPrecedenceEnum Precedence
	    {
	        get { return ExprPrecedenceEnum.UNARY; }
	    }

	    public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
	    {
	        if (!(node is ExprIdentNode))
	        {
	            return false;
	        }

	        ExprIdentNode other = (ExprIdentNode) node;

            if (ignoreStreamPrefix && _resolvedPropertyName != null && other.ResolvedPropertyName != null && _resolvedPropertyName.Equals(other.ResolvedPropertyName))
                return true;
            if (_streamOrPropertyName != null ? !_streamOrPropertyName.Equals(other.StreamOrPropertyName) : other.StreamOrPropertyName != null)
	            return false;
	        if (_unresolvedPropertyName != null ? !_unresolvedPropertyName.Equals(other.UnresolvedPropertyName) : other.UnresolvedPropertyName != null)
	            return false;
	        return true;
	    }

	    public ExprIdentNodeEvaluator ExprEvaluatorIdent
	    {
	        get { return _evaluator; }
	    }
	}
} // end of namespace
