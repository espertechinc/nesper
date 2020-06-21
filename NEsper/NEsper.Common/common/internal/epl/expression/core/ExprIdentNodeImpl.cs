///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Represents an stream property identifier in a filter expressiun tree.
    /// </summary>
    public class ExprIdentNodeImpl : ExprNodeBase,
        ExprIdentNode,
        ExprNode,
        ExprForgeInstrumentable
    {
        // select myprop params from[] is a simple property, no stream supplied
        // select s0.myprop params from[] is a simple property with a stream supplied, or a nested property (cannot tell until resolved)
        // select indexed[1] from ...   is a indexed property

        private string _resolvedPropertyName;
        private string _resolvedStreamName;
        private string _streamOrPropertyName;

        [NonSerialized] private StatementCompileTimeServices _compileTimeServices;
        [NonSerialized] private ExprIdentNodeEvaluator _evaluator;
        [NonSerialized] private StatementRawInfo _statementRawInfo;
        
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="unresolvedPropertyName">is the event property name in unresolved form, ie. unvalidated against streams</param>
        public ExprIdentNodeImpl(string unresolvedPropertyName)
        {
            UnresolvedPropertyName = unresolvedPropertyName ?? throw new ArgumentException("Property name is null");
            _streamOrPropertyName = null;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="unresolvedPropertyName">is the event property name in unresolved form, ie. unvalidated against streams</param>
        /// <param name="streamOrPropertyName">
        ///     is the stream name, or if not a valid stream name a possible nested property namein one of the streams.
        /// </param>
        public ExprIdentNodeImpl(
            string unresolvedPropertyName,
            string streamOrPropertyName)
        {
            UnresolvedPropertyName = unresolvedPropertyName ?? throw new ArgumentException("Property name is null");
            _streamOrPropertyName = streamOrPropertyName ?? throw new ArgumentException("Stream (or property name) name is null");
        }

        public ExprIdentNodeImpl(
            EventType eventType,
            string propertyName,
            int streamNumber)
        {
            UnresolvedPropertyName = propertyName;
            _resolvedPropertyName = propertyName;
            var propertyGetter = ((EventTypeSPI) eventType).GetGetterSPI(propertyName);
            if (propertyGetter == null) {
                throw new ArgumentException("Ident-node constructor could not locate property " + propertyName);
            }

            var propertyType = eventType.GetPropertyType(propertyName);
            _evaluator = new ExprIdentNodeEvaluatorImpl(
                streamNumber,
                propertyGetter,
                propertyType.GetBoxedType(),
                this,
                (EventTypeSPI) eventType,
                true,
                false);
        }

        public bool IsConstantResult => false;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public Type EvaluationType => _evaluator.EvaluationType;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return _evaluator.Codegen(requiredType, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var builder = new InstrumentationBuilderExpr(
                GetType(),
                this,
                "ExprIdent",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
            return builder.Build();
        }

        public ExprEvaluator ExprEvaluator => _evaluator;

        public override ExprForge Forge {
            get {
                if (_resolvedPropertyName == null) {
                    throw CheckValidatedException();
                }

                return this;
            }
        }

        /// <summary>
        ///     For unit testing, returns unresolved property name.
        /// </summary>
        /// <returns>property name</returns>
        public string UnresolvedPropertyName { get; }

        /// <summary>
        ///     For unit testing, returns stream or property name candidate.
        /// </summary>
        /// <returns>stream name, or property name of a nested property of one of the streams</returns>
        public string StreamOrPropertyName {
            get => _streamOrPropertyName;
            set => _streamOrPropertyName = value;
        }

        public bool FilterLookupEligible => _evaluator.StreamNum == 0 && !_evaluator.IsContextEvaluated;

        public ExprFilterSpecLookupableForge FilterLookupable {
            get {
                var serde = _compileTimeServices.SerdeResolver.SerdeForFilter(_evaluator.EvaluationType, _statementRawInfo);
                var eval = new ExprEventEvaluatorForgeFromProp(_evaluator.Getter);
                return new ExprFilterSpecLookupableForge(_resolvedPropertyName, eval, null, _evaluator.EvaluationType, false, serde);
            }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            _compileTimeServices = validationContext.StatementCompileTimeService;
            _statementRawInfo = validationContext.StatementRawInfo;

            // rewrite expression into a table-access expression
            if (validationContext.StreamTypeService.HasTableTypes) {
                var tableIdentNode = TableCompileTimeUtil.GetTableIdentNode(
                    validationContext.StreamTypeService,
                    UnresolvedPropertyName,
                    _streamOrPropertyName,
                    validationContext.TableCompileTimeResolver);
                if (tableIdentNode != null) {
                    return tableIdentNode;
                }
            }

            var unescapedPropertyName = PropertyParser.UnescapeBacktickForProperty(UnresolvedPropertyName);
            var propertyInfoPair = ExprIdentNodeUtil.GetTypeFromStream(
                validationContext.StreamTypeService,
                unescapedPropertyName,
                _streamOrPropertyName,
                false,
                validationContext.TableCompileTimeResolver);
            _resolvedStreamName = propertyInfoPair.Second;
            int streamNum = propertyInfoPair.First.StreamNum;
            var propertyType = propertyInfoPair.First.PropertyType; // GetBoxedType()
            _resolvedPropertyName = propertyInfoPair.First.PropertyName;
            EventType eventType = propertyInfoPair.First.StreamEventType;
            EventPropertyGetterSPI propertyGetter;
            try {
                propertyGetter = ((EventTypeSPI) eventType).GetGetterSPI(_resolvedPropertyName);
            }
            catch (PropertyAccessException ex) {
                throw new ExprValidationException(
                    "Property '" + UnresolvedPropertyName + "' is not valid: " + ex.Message,
                    ex);
            }

            if (propertyGetter == null) {
                throw new ExprValidationException(
                    "Property getter not available for property '" + UnresolvedPropertyName + "'");
            }

            var audit = AuditEnum.PROPERTY.GetAudit(validationContext.Annotations) != null;
            _evaluator = new ExprIdentNodeEvaluatorImpl(
                streamNum,
                propertyGetter,
                propertyType.GetBoxedType(),
                this,
                (EventTypeSPI) eventType,
                validationContext.StreamTypeService.IsOptionalStreams,
                audit);

            // if running in a context, take the property value from context
            if (validationContext.ContextDescriptor != null && !validationContext.IsFilterExpression) {
                var fromType = validationContext.StreamTypeService.EventTypes[streamNum];
                var contextPropertyName =
                    validationContext.ContextDescriptor.ContextPropertyRegistry.GetPartitionContextPropertyName(
                        fromType,
                        _resolvedPropertyName);
                if (contextPropertyName != null) {
                    var contextType = (EventTypeSPI) validationContext.ContextDescriptor.ContextPropertyRegistry
                        .ContextEventType;
                    var type = contextType.GetPropertyType(contextPropertyName).GetBoxedType();
                    _evaluator = new ExprIdentNodeEvaluatorContext(
                        streamNum,
                        type,
                        contextType.GetGetterSPI(contextPropertyName),
                        (EventTypeSPI) eventType);
                }
            }

            return null;
        }

        public int? StreamReferencedIfAny => StreamId;

        public string RootPropertyNameIfAny => ResolvedPropertyNameRoot;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprIdentNode)) {
                return false;
            }

            var other = (ExprIdentNode) node;

            if (ignoreStreamPrefix &&
                _resolvedPropertyName != null &&
                other.ResolvedPropertyName != null &&
                _resolvedPropertyName.Equals(other.ResolvedPropertyName)) {
                return true;
            }

            if (_streamOrPropertyName != null
                ? !_streamOrPropertyName.Equals(other.StreamOrPropertyName)
                : other.StreamOrPropertyName != null) {
                return false;
            }

            if (UnresolvedPropertyName != null
                ? !UnresolvedPropertyName.Equals(other.UnresolvedPropertyName)
                : other.UnresolvedPropertyName != null) {
                return false;
            }

            return true;
        }

        public ExprIdentNodeEvaluator ExprEvaluatorIdent => _evaluator;

        public bool IsOptionalEvent {
            set => _evaluator.OptionalEvent = value;
        }

        /// <summary>
        ///     Returns the unresolved property name in it's complete form, including
        ///     the stream name if there is one.
        /// </summary>
        /// <value>property name</value>
        public string FullUnresolvedName {
            get {
                if (_streamOrPropertyName == null) {
                    return UnresolvedPropertyName;
                }

                return _streamOrPropertyName + "." + UnresolvedPropertyName;
            }
        }

        /// <summary>
        ///     Returns stream id supplying the property value.
        /// </summary>
        /// <value>stream number</value>
        public int StreamId {
            get {
                if (_evaluator == null) {
                    throw new IllegalStateException("Identifier expression has not been validated");
                }

                return _evaluator.StreamNum;
            }
        }

        public Type Type {
            get {
                if (_evaluator == null) {
                    throw new IllegalStateException("Identifier expression has not been validated");
                }

                return _evaluator.EvaluationType;
            }
        }

        /// <summary>
        ///     Returns stream name as resolved by lookup of property in streams.
        /// </summary>
        /// <value>stream name</value>
        public string ResolvedStreamName {
            get {
                if (_resolvedStreamName == null) {
                    throw new IllegalStateException("Identifier node has not been validated");
                }

                return _resolvedStreamName;
            }
        }

        /// <summary>
        ///     Return property name as resolved by lookup in streams.
        /// </summary>
        /// <value>property name</value>
        public string ResolvedPropertyName {
            get {
                if (_resolvedPropertyName == null) {
                    throw new IllegalStateException("Identifier node has not been validated");
                }

                return _resolvedPropertyName;
            }
        }

        /// <summary>
        ///     Returns the root of the resolved property name, if any.
        /// </summary>
        /// <value>root</value>
        public string ResolvedPropertyNameRoot {
            get {
                if (_resolvedPropertyName == null) {
                    throw new IllegalStateException("Identifier node has not been validated");
                }

                if (_resolvedPropertyName.IndexOf('[') != -1) {
                    return _resolvedPropertyName.Substring(0, _resolvedPropertyName.IndexOf('['));
                }

                if (_resolvedPropertyName.IndexOf('(') != -1) {
                    return _resolvedPropertyName.Substring(0, _resolvedPropertyName.IndexOf('('));
                }

                if (_resolvedPropertyName.IndexOf('.') != -1) {
                    return _resolvedPropertyName.Substring(0, _resolvedPropertyName.IndexOf('.'));
                }

                return _resolvedPropertyName;
            }
        }

        public override string ToString()
        {
            return "unresolvedPropertyName=" +
                   UnresolvedPropertyName +
                   " streamOrPropertyName=" +
                   _streamOrPropertyName +
                   " resolvedPropertyName=" +
                   _resolvedPropertyName;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ToPrecedenceFreeEPL(writer, _streamOrPropertyName, UnresolvedPropertyName, flags);
        }

        public static void ToPrecedenceFreeEPL(
            TextWriter writer,
            string streamOrPropertyName,
            string unresolvedPropertyName,
            ExprNodeRenderableFlags flags)
        {
            if (streamOrPropertyName != null && flags.IsWithStreamPrefix) {
                writer.Write(StringValue.UnescapeDot(streamOrPropertyName));
                writer.Write('.');
            }

            writer.Write(StringValue.UnescapeDot(StringValue.UnescapeBacktick(unresolvedPropertyName)));
        }
        
        public ExprEnumerationForgeDesc GetEnumerationForge(
            StreamTypeService streamTypeService,
            ContextCompileTimeDescriptor contextDescriptor)
        {
            var fragmentEventType = _evaluator.EventType.GetFragmentType(ResolvedPropertyName);
            if (fragmentEventType == null || fragmentEventType.IsIndexed) {
                return null;
            }
            var forge = new ExprIdentNodeFragmentTypeEnumerationForge(
                _resolvedPropertyName, StreamId, fragmentEventType.FragmentType, _evaluator.EventType.GetGetterSPI(_resolvedPropertyName));
            return new ExprEnumerationForgeDesc(forge, streamTypeService.IStreamOnly[StreamId], -1);
        }
    }
} // end of namespace