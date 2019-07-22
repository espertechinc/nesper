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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.metrics.instrumentation;
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

        [NonSerialized] private ExprIdentNodeEvaluator evaluator;
        private string resolvedPropertyName;

        private string resolvedStreamName;
        private string streamOrPropertyName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="unresolvedPropertyName">is the event property name in unresolved form, ie. unvalidated against streams</param>
        public ExprIdentNodeImpl(string unresolvedPropertyName)
        {
            UnresolvedPropertyName = unresolvedPropertyName ?? throw new ArgumentException("Property name is null");
            streamOrPropertyName = null;
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
            if (unresolvedPropertyName == null) {
                throw new ArgumentException("Property name is null");
            }

            if (streamOrPropertyName == null) {
                throw new ArgumentException("Stream (or property name) name is null");
            }

            UnresolvedPropertyName = unresolvedPropertyName;
            this.streamOrPropertyName = streamOrPropertyName;
        }

        public ExprIdentNodeImpl(
            EventType eventType,
            string propertyName,
            int streamNumber)
        {
            UnresolvedPropertyName = propertyName;
            resolvedPropertyName = propertyName;
            var propertyGetter = ((EventTypeSPI) eventType).GetGetterSPI(propertyName);
            if (propertyGetter == null) {
                throw new ArgumentException("Ident-node constructor could not locate property " + propertyName);
            }

            var propertyType = eventType.GetPropertyType(propertyName);
            evaluator = new ExprIdentNodeEvaluatorImpl(
                streamNumber,
                propertyGetter,
                propertyType.GetBoxedType(),
                this,
                eventType,
                true,
                false);
        }

        public bool IsConstantResult => false;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public Type EvaluationType => evaluator.EvaluationType;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return evaluator.Codegen(requiredType, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(),
                this,
                "ExprIdent",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope).Build();
        }

        public ExprEvaluator ExprEvaluator => evaluator;

        public override ExprForge Forge {
            get {
                if (resolvedPropertyName == null) {
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
            get => streamOrPropertyName;
            set => streamOrPropertyName = value;
        }

        public bool FilterLookupEligible => evaluator.StreamNum == 0 && !evaluator.IsContextEvaluated;

        public ExprFilterSpecLookupableForge FilterLookupable => new ExprFilterSpecLookupableForge(
            resolvedPropertyName,
            evaluator.Getter,
            evaluator.EvaluationType,
            false);

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // rewrite expression into a table-access expression
            if (validationContext.StreamTypeService.HasTableTypes) {
                var tableIdentNode = TableCompileTimeUtil.GetTableIdentNode(
                    validationContext.StreamTypeService,
                    UnresolvedPropertyName,
                    streamOrPropertyName,
                    validationContext.TableCompileTimeResolver);
                if (tableIdentNode != null) {
                    return tableIdentNode;
                }
            }

            var unescapedPropertyName = PropertyParser.UnescapeBacktickForProperty(UnresolvedPropertyName);
            var propertyInfoPair = ExprIdentNodeUtil.GetTypeFromStream(
                validationContext.StreamTypeService,
                unescapedPropertyName,
                streamOrPropertyName,
                false,
                validationContext.TableCompileTimeResolver);
            resolvedStreamName = propertyInfoPair.Second;
            int streamNum = propertyInfoPair.First.StreamNum;
            var propertyType = Boxing.GetBoxedType(propertyInfoPair.First.PropertyType);
            resolvedPropertyName = propertyInfoPair.First.PropertyName;
            EventType eventType = propertyInfoPair.First.StreamEventType;
            EventPropertyGetterSPI propertyGetter;
            try {
                propertyGetter = ((EventTypeSPI) eventType).GetGetterSPI(resolvedPropertyName);
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
            evaluator = new ExprIdentNodeEvaluatorImpl(
                streamNum,
                propertyGetter,
                propertyType,
                this,
                eventType,
                validationContext.StreamTypeService.IsOptionalStreams,
                audit);

            // if running in a context, take the property value from context
            if (validationContext.ContextDescriptor != null && !validationContext.IsFilterExpression) {
                var fromType = validationContext.StreamTypeService.EventTypes[streamNum];
                var contextPropertyName =
                    validationContext.ContextDescriptor.ContextPropertyRegistry.GetPartitionContextPropertyName(
                        fromType,
                        resolvedPropertyName);
                if (contextPropertyName != null) {
                    var contextType = (EventTypeSPI) validationContext.ContextDescriptor.ContextPropertyRegistry
                        .ContextEventType;
                    var type = contextType.GetPropertyType(contextPropertyName).GetBoxedType();
                    evaluator = new ExprIdentNodeEvaluatorContext(
                        streamNum,
                        type,
                        contextType.GetGetterSPI(contextPropertyName));
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
                resolvedPropertyName != null &&
                other.ResolvedPropertyName != null &&
                resolvedPropertyName.Equals(other.ResolvedPropertyName)) {
                return true;
            }

            if (streamOrPropertyName != null
                ? !streamOrPropertyName.Equals(other.StreamOrPropertyName)
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

        public ExprIdentNodeEvaluator ExprEvaluatorIdent => evaluator;

        public bool IsOptionalEvent {
            set => evaluator.OptionalEvent = value;
        }

        /// <summary>
        ///     Returns the unresolved property name in it's complete form, including
        ///     the stream name if there is one.
        /// </summary>
        /// <value>property name</value>
        public string FullUnresolvedName {
            get {
                if (streamOrPropertyName == null) {
                    return UnresolvedPropertyName;
                }

                return streamOrPropertyName + "." + UnresolvedPropertyName;
            }
        }

        /// <summary>
        ///     Returns stream id supplying the property value.
        /// </summary>
        /// <value>stream number</value>
        public int StreamId {
            get {
                if (evaluator == null) {
                    throw new IllegalStateException("Identifier expression has not been validated");
                }

                return evaluator.StreamNum;
            }
        }

        public Type Type {
            get {
                if (evaluator == null) {
                    throw new IllegalStateException("Identifier expression has not been validated");
                }

                return evaluator.EvaluationType;
            }
        }

        /// <summary>
        ///     Returns stream name as resolved by lookup of property in streams.
        /// </summary>
        /// <value>stream name</value>
        public string ResolvedStreamName {
            get {
                if (resolvedStreamName == null) {
                    throw new IllegalStateException("Identifier node has not been validated");
                }

                return resolvedStreamName;
            }
        }

        /// <summary>
        ///     Return property name as resolved by lookup in streams.
        /// </summary>
        /// <value>property name</value>
        public string ResolvedPropertyName {
            get {
                if (resolvedPropertyName == null) {
                    throw new IllegalStateException("Identifier node has not been validated");
                }

                return resolvedPropertyName;
            }
        }

        /// <summary>
        ///     Returns the root of the resolved property name, if any.
        /// </summary>
        /// <value>root</value>
        public string ResolvedPropertyNameRoot {
            get {
                if (resolvedPropertyName == null) {
                    throw new IllegalStateException("Identifier node has not been validated");
                }

                if (resolvedPropertyName.IndexOf('[') != -1) {
                    return resolvedPropertyName.Substring(0, resolvedPropertyName.IndexOf('['));
                }

                if (resolvedPropertyName.IndexOf('(') != -1) {
                    return resolvedPropertyName.Substring(0, resolvedPropertyName.IndexOf('('));
                }

                if (resolvedPropertyName.IndexOf('.') != -1) {
                    return resolvedPropertyName.Substring(0, resolvedPropertyName.IndexOf('.'));
                }

                return resolvedPropertyName;
            }
        }

        public override string ToString()
        {
            return "unresolvedPropertyName=" +
                   UnresolvedPropertyName +
                   " streamOrPropertyName=" +
                   streamOrPropertyName +
                   " resolvedPropertyName=" +
                   resolvedPropertyName;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ToPrecedenceFreeEPL(writer, streamOrPropertyName, UnresolvedPropertyName);
        }

        public static void ToPrecedenceFreeEPL(
            TextWriter writer,
            string streamOrPropertyName,
            string unresolvedPropertyName)
        {
            if (streamOrPropertyName != null) {
                writer.Write(StringValue.UnescapeDot(streamOrPropertyName));
                writer.Write('.');
            }

            writer.Write(StringValue.UnescapeDot(StringValue.UnescapeBacktick(unresolvedPropertyName)));
        }
    }
} // end of namespace