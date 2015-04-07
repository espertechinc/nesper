///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.ops
{
	/// <summary>
	/// Represents the "new {...}" operator in an expression tree.
	/// </summary>
	[Serializable]
    public class ExprNewStructNode : ExprNodeBase , ExprEvaluatorTypableReturn
    {
	    private readonly string[] _columnNames;
	    [NonSerialized] private LinkedHashMap<string, object> _eventType;
	    [NonSerialized] private ExprEvaluator[] _evaluators;
	    private bool _isAllConstants;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    public ExprNewStructNode(string[] columnNames)
	    {
	        this._columnNames = columnNames;
	    }

	    public override ExprEvaluator ExprEvaluator
	    {
	        get { return this; }
	    }

	    public override ExprNode Validate(ExprValidationContext validationContext)
	    {
	        _eventType = new LinkedHashMap<string, object>();
	        _evaluators = ExprNodeUtility.GetEvaluators(this.ChildNodes);

	        for (var i = 0; i < _columnNames.Length; i++) {
	            _isAllConstants = _isAllConstants && this.ChildNodes[i].IsConstantResult;
	            if (_eventType.ContainsKey(_columnNames[i])) {
	                throw new ExprValidationException("Failed to validate new-keyword property names, property '" + _columnNames[i] + "' has already been declared");
	            }

	            IDictionary<string, object> eventTypeResult = null;
	            if (_evaluators[i] is ExprEvaluatorTypableReturn) {
	                eventTypeResult = ((ExprEvaluatorTypableReturn) _evaluators[i]).RowProperties;
	            }
	            if (eventTypeResult != null) {
	                _eventType.Put(_columnNames[i], eventTypeResult);
	            }
	            else {
	                var classResult = _evaluators[i].ReturnType.GetBoxedType();
	                _eventType.Put(_columnNames[i], classResult);
	            }
	        }
	        return null;
	    }

	    public string[] ColumnNames
	    {
	        get { return _columnNames; }
	    }

	    public override bool IsConstantResult
	    {
	        get { return _isAllConstants; }
	    }

	    public Type ReturnType
	    {
	        get { return typeof (IDictionary<string, object>); }
	    }

	    public IDictionary<string, object> RowProperties
	    {
	        get { return _eventType; }
	    }

	    public object Evaluate(EvaluateParams evaluateParams)
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprNew(this);}
	        IDictionary<string, object> props = new Dictionary<string, object>();
	        for (var i = 0; i < _evaluators.Length; i++) {
	            props.Put(_columnNames[i], _evaluators[i].Evaluate(evaluateParams));
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprNew(props);}
	        return props;
	    }

	    public bool? IsMultirow
	    {
	        get { return false; } // New itself can only return a single row
	    }

	    public object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
	        var rows = new object[_columnNames.Length];
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, context);
            for (var i = 0; i < _columnNames.Length; i++)
	        {
	            rows[i] = _evaluators[i].Evaluate(evaluateParams);
	        }
	        return rows;
	    }

	    public object[][] EvaluateTypableMulti(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
	        return null;
	    }

	    public override bool EqualsNode(ExprNode node)
	    {
	        var other = node as ExprNewStructNode;
	        return other != null && CompatExtensions.DeepEquals(other._columnNames, _columnNames);
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
	        writer.Write("new{");
	        var delimiter = "";
	        for (var i = 0; i < this.ChildNodes.Length; i++) {
	            writer.Write(delimiter);
                writer.Write(_columnNames[i]);
	            var expr = this.ChildNodes[i];

	            var outputexpr = true;
	            if (expr is ExprIdentNode) {
	                var prop = (ExprIdentNode) expr;
	                if (prop.ResolvedPropertyName.Equals( _columnNames[i])) {
	                    outputexpr = false;
	                }
	            }

	            if (outputexpr) {
                    writer.Write("=");
	                expr.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
	            }
	            delimiter = ",";
	        }
	        writer.Write("}");
	    }

	    public override ExprPrecedenceEnum Precedence
	    {
	        get { return ExprPrecedenceEnum.UNARY; }
	    }
    }
} // end of namespace
