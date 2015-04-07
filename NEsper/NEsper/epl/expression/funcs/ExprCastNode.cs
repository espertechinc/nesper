///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.pattern.observer;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.funcs
{
    /// <summary>
    /// Represents the CAST(expression, type) function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprCastNode : ExprNodeBase
    {
        internal delegate Object ComputeCaster(Object input, EvaluateParams evaluateParams);

        private readonly String _typeIdentifier;
        private ComputeCaster _typeCaster;
        private Type _targetType;
        private Type _sourceType;
        private bool _isConstant;

        [NonSerialized] private ExprEvaluator _exprEvaluator;

        /// <summary>Ctor. </summary>
        /// <param name="typeIdentifier">the the name of the type to cast to</param>
        public ExprCastNode(String typeIdentifier)
        {
            _typeIdentifier = typeIdentifier;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return _exprEvaluator; }
        }

        /// <summary>Returns the name of the type of cast to. </summary>
        /// <value>type name</value>
        public string TypeIdentifier
        {
            get { return _typeIdentifier; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length == 0 || ChildNodes.Length > 2)
            {
                throw new ExprValidationException("Cast function node must have one or two child expressions");
            }

            var valueEvaluator = ChildNodes[0].ExprEvaluator;
            _sourceType = valueEvaluator.ReturnType;

            var typeIdentifier = _typeIdentifier.Trim();

            // determine date format parameter
            var namedParams = ExprNodeUtility.GetNamedExpressionsHandleDups(ChildNodes);
            ExprNodeUtility.ValidateNamed(namedParams, new String[] {"dateformat"});
            ExprNamedParameterNode dateFormatParameter = namedParams.Get("dateformat");
            if (dateFormatParameter != null) {
                ExprNodeUtility.ValidateNamedExpectType(dateFormatParameter, typeof(string));
            }

            // identify target type
            // try the primitive names including "string"
            _targetType = TypeHelper.GetPrimitiveTypeForName(typeIdentifier).GetBoxedType();
            _isConstant = true;

            if (dateFormatParameter != null) {
                if (_sourceType != typeof (string)) {
                    throw new ExprValidationException("Use of the '" + dateFormatParameter.ParameterName + "' named parameter requires a string-type input");
                }

                if (_targetType == null) {
                    try {
                        _targetType = TypeHelper.GetTypeForSimpleName(typeIdentifier);
                    }
                    catch (TypeLoadException e) {
                        // expected
                    }
                }

                // dynamic or static date format
                String staticDateFormat = null;
                ExprEvaluator dynamicDateFormat = null;
                Boolean iso8601Format = false;
                if (!dateFormatParameter.ChildNodes[0].IsConstantResult) {
                    dynamicDateFormat = dateFormatParameter.ChildNodes[0].ExprEvaluator;
                }
                else {
                    staticDateFormat = (string) dateFormatParameter.ChildNodes[0].ExprEvaluator.Evaluate(
                        new EvaluateParams(null, true, validationContext.ExprEvaluatorContext));
                    if (staticDateFormat.ToLower().Trim() == "iso") {
                        iso8601Format = true;
                    }
                    else {
                        try {
                            DateTime dateTimeTemp;
                            DateTime.TryParseExact("", staticDateFormat, null, DateTimeStyles.None, out dateTimeTemp);
                            //new SimpleDateFormat(staticDateFormat);
                        }
                        catch (Exception ex) {
                            throw new ExprValidationException(
                                "Invalid date format '" + staticDateFormat + "': " + ex.Message, ex);
                        }
                    }
                }
                if (_targetType == typeof(DateTime?) || typeIdentifier.ToLower() == "date") {
                    _targetType = typeof(DateTime?);
                    if (staticDateFormat != null) {
                        if (iso8601Format) {
                            _typeCaster = StringToDateWStaticISOFormatComputer();
                        }
                        else {
                            _typeCaster = StringToDateWStaticFormatComputer(staticDateFormat);
                        }
                    }
                    else {
                        _typeCaster = StringToDateWDynamicFormatComputer(dynamicDateFormat);
                        _isConstant = false;
                    }
                }
                else if (_targetType == typeof(long?)) {
                    _targetType = typeof(long?);
                    if (staticDateFormat != null) {
                        if (iso8601Format) {
                            _typeCaster = StringToLongWStaticISOFormatComputer();
                        }
                        else {
                            _typeCaster = StringToLongWStaticFormatComputer(staticDateFormat);
                        }
                    }
                    else {
                        _typeCaster = StringToLongWDynamicFormatComputer(dynamicDateFormat);
                        _isConstant = false;
                    }
                }
                else {
                    throw new ExprValidationException("Use of the '" + dateFormatParameter.ParameterName + "' named parameter requires a target type of datetime or long");
                }
            }
            else if (_targetType == null)
            {
                try
                {
                    _targetType = TypeHelper.ResolveType(_typeIdentifier, true);
                }
                catch (Exception e)
                {
                    throw new ExprValidationException(
                        "Type as listed in cast function by name '" + _typeIdentifier + "' cannot be loaded", e);
                }
            }

            _sourceType = _sourceType.GetBoxedType();
            _targetType = _targetType.GetBoxedType();


            if (_typeCaster != null)
            {
                // no-op
            }
            else if (_sourceType == _targetType)
            {
                _typeCaster = (o, evaluateParams) => o;
            }
            else if (_targetType == typeof(string))
            {
                _typeCaster = (o, evaluateParams) => Convert.ToString(o);
            }
            else if (_sourceType == typeof(string))
            {
                var typeParser = SimpleTypeParserFactory.GetParser(_targetType);
                _typeCaster = (o, evaluateParams) => typeParser((string)o);
            }
            else
            {
                var typeCaster = CastHelper.GetTypeCaster(_sourceType, _targetType);
                _typeCaster = (o, evaluateParams) => typeCaster.Invoke(o);
            }

            // determine constant or not
            // - basically, if the terms are constant then the cast can be computed now and stored away
            // - for future use.
            if (_isConstant && ChildNodes[0].IsConstantResult)
            {
                var evaluateParams = new EvaluateParams(null, true, validationContext.ExprEvaluatorContext);
                var inputValue = valueEvaluator.Evaluate(evaluateParams);
                var constantValue = _typeCaster.Invoke(inputValue, evaluateParams);
                _exprEvaluator = new ExprCastNodeConstEval(this, constantValue);
            }
            else
            {
                _exprEvaluator = new ExprCastNodeNonConstEval(this, valueEvaluator, _typeCaster);
            }

            return null;
        }

        public override bool IsConstantResult
        {
            get { return _isConstant; }
        }

        public Type TargetType
        {
            get { return _targetType; }
        }

        /*
        public Object Evaluate(EvaluateParams evaluateParams)
        {
            Mutable<object> result = new Mutable<object>();

            using (Instrument.With(
                i => i.QExprCast(this),
                i => i.AExprCast(result)))
            {
                result.Value = _exprEvaluator.Evaluate(evaluateParams);
                if (result.Value == null)
                {
                    return null;
                }

                if (_typeCaster != null)
                {
                    result.Value = _typeCaster.Invoke(result.Value);
                    if (result.Value != null)
                    {
                        return result.Value;
                    }
                }

                // Simple dynamic-cast
                if (_targetType.IsInstanceOfType(result.Value))
                {
                    if (!_targetType.IsValueType)
                    {
                        return result.Value;
                    }
                }

                try
                {
                    result.Value = Convert.ChangeType(result.Value, _targetType);
                    return result.Value;
                }
                catch (FormatException) // Occurs when numeric types parse strings
                {
                    result.Value = null;
                    return null;
                }
                catch (InvalidCastException)
                {
                    result.Value = null;
                    return null;
                }
            }
        }
        */

        internal static EPException HandleParseException(string formatString, string date, FormatException ex)
        {
            return new EPException("Exception parsing date '" + date + "' format '" + formatString + "': " + ex.Message, ex);
        }

        internal static EPException HandleParseISOException(string date, ScheduleParameterException ex)
        {
            return new EPException("Exception parsing iso8601 date '" + date + "': " + ex.Message, ex);
        }

        internal static DateTime ParseSafe(string input, string dateFormat)
        {
            try
            {
                return DateTime.ParseExact(
                    input, dateFormat, (IFormatProvider) null, DateTimeStyles.None);
            }
            catch (FormatException e)
            {
                throw HandleParseException(dateFormat, input.ToString(), e);
            }
        }

        internal static DateTime StringToDateWStaticISOFormat(object input)
        {
            try
            {
                return TimerScheduleISO8601Parser.ParseDate(input.ToString()).Value;
            }
            catch (ScheduleParameterException e)
            {
                throw HandleParseISOException(input.ToString(), e);
            }
        }

        internal static ComputeCaster StringToLongWStaticISOFormatComputer()
        {
            return (input, evaluateParams) => StringToDateWStaticISOFormat(input).TimeInMillis();
        }

        internal static ComputeCaster StringToDateWStaticISOFormatComputer()
        {
            return (input, evaluateParams) => StringToDateWStaticISOFormat(input);
        }

        internal static ComputeCaster StringToLongWStaticFormatComputer(string dateFormat)
        {
            return (input, evaluateParams) => ParseSafe(input.ToString(), dateFormat).TimeInMillis();
        }
        
        internal static ComputeCaster StringToDateWStaticFormatComputer(string dateFormat)
        {
            return (input, evaluateParams) => ParseSafe(input.ToString(), dateFormat);
        }

        internal static DateTime StringToDateWDynamicFormat(
            ExprEvaluator dateFormatEval,
            EvaluateParams evaluateParams,
            object input)
        {
            var format = dateFormatEval.Evaluate(evaluateParams);
            if (format == null)
            {
                throw new EPException("Null date format returned by 'dateformat' expression");
            }

            return ParseSafe(input.ToString(), (string)format);
        }

        internal static ComputeCaster StringToLongWDynamicFormatComputer(ExprEvaluator dateFormatEval)
        {
            return (input, evaluateParams) => StringToDateWDynamicFormat(dateFormatEval, evaluateParams, input).TimeInMillis();
        }

        internal static ComputeCaster StringToDateWDynamicFormatComputer(ExprEvaluator dateFormatEval)
        {
            return (input, evaluateParams) => StringToDateWDynamicFormat(dateFormatEval, evaluateParams, input);
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("cast(");
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            writer.Write(",");
            writer.Write(_typeIdentifier);
            for (int i = 1; i < ChildNodes.Length; i++)
            {
                writer.Write(",");
                ChildNodes[i].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            }
            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node)
        {
            var other = node as ExprCastNode;
            return (other != null) && (_typeIdentifier == other._typeIdentifier);
        }
    }
}
