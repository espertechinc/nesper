///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    /// Represents an stream property identifier in a filter expressiun tree.
    /// </summary>
    public class ExprIdentNodeImpl : ExprNodeBase,
        ExprIdentNode,
        ExprNode,
        ExprForgeInstrumentable
    {
        // select myprop from...        is a simple property, no stream supplied
        // select s0.myprop from...     is a simple property with a stream supplied, or a nested property (cannot tell until resolved)
        // select indexed[1] from ...   is a indexed property

        private readonly string unresolvedPropertyName;
        private string streamOrPropertyName;

        private string resolvedStreamName;
        private string resolvedPropertyName;
        [NonSerialized] private StatementCompileTimeServices compileTimeServices;
        [NonSerialized] private ExprIdentNodeEvaluator evaluator;
        [NonSerialized] private StatementRawInfo statementRawInfo;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="unresolvedPropertyName">is the event property name in unresolved form, ie. unvalidated against streams</param>
        public ExprIdentNodeImpl(string unresolvedPropertyName)
        {
            if (unresolvedPropertyName == null) {
                throw new ArgumentException("Property name is null");
            }

            this.unresolvedPropertyName = unresolvedPropertyName;
            streamOrPropertyName = null;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="unresolvedPropertyName">is the event property name in unresolved form, ie. unvalidated against streams</param>
        /// <param name="streamOrPropertyName">is the stream name, or if not a valid stream name a possible nested property namein one of the streams.
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

            this.unresolvedPropertyName = unresolvedPropertyName;
            this.streamOrPropertyName = streamOrPropertyName;
        }

        public ExprIdentNodeImpl(
            EventType eventType,
            string propertyName,
            int streamNumber)
        {
            unresolvedPropertyName = propertyName;
            resolvedPropertyName = propertyName;
            var propertyGetter = ((EventTypeSPI)eventType).GetGetterSPI(propertyName);
            if (propertyGetter == null) {
                throw new ArgumentException("Ident-node constructor could not locate property " + propertyName);
            }

            var propertyType = eventType.GetPropertyType(propertyName);
            evaluator = new ExprIdentNodeEvaluatorImpl(
                streamNumber,
                propertyGetter,
                propertyType.GetBoxedType(),
                this,
                (EventTypeSPI)eventType,
                true,
                false);
        }

        public override ExprForge Forge {
            get {
                if (resolvedPropertyName == null) {
                    throw CheckValidatedException();
                }

                return this;
            }
        }

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

        /// <summary>
        /// For unit testing, returns unresolved property name.
        /// </summary>
        /// <value>property name</value>
        public string UnresolvedPropertyName => unresolvedPropertyName;

        /// <summary>
        /// For unit testing, returns stream or property name candidate.
        /// </summary>
        /// <value>stream name, or property name of a nested property of one of the streams</value>
        public string StreamOrPropertyName {
            get => streamOrPropertyName;
            set => streamOrPropertyName = value;
        }

        public bool IsOptionalEvent {
            get => evaluator.OptionalEvent;
            set => evaluator.OptionalEvent = value;
        }

        /// <summary>
        /// Returns the unresolved property name in it's complete form, including
        /// the stream name if there is one.
        /// </summary>
        /// <value>property name</value>
        public string FullUnresolvedName {
            get {
                if (streamOrPropertyName == null) {
                    return unresolvedPropertyName;
                }
                else {
                    return streamOrPropertyName + "." + unresolvedPropertyName;
                }
            }
        }

        public bool IsFilterLookupEligible =>
            evaluator.StreamNum == 0 &&
            !evaluator.IsContextEvaluated &&
            !(evaluator.EvaluationType == null || evaluator.EvaluationType == null);

        public ExprFilterSpecLookupableForge FilterLookupable {
            get {
                var type = evaluator.EvaluationType;
                var serde =
                    compileTimeServices.SerdeResolver.SerdeForFilter(type, statementRawInfo);
                var eval = new ExprEventEvaluatorForgeFromProp(evaluator.Getter);
                return new ExprFilterSpecLookupableForge(resolvedPropertyName, eval, null, type, false, serde);
            }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            compileTimeServices = validationContext.StatementCompileTimeService;
            statementRawInfo = validationContext.StatementRawInfo;

            // rewrite expression into a table-access expression
            if (validationContext.StreamTypeService.HasTableTypes) {
                var tableIdentNode = TableCompileTimeUtil.GetTableIdentNode(
                    validationContext.StreamTypeService,
                    unresolvedPropertyName,
                    streamOrPropertyName,
                    validationContext.TableCompileTimeResolver);
                if (tableIdentNode != null) {
                    return tableIdentNode;
                }
            }

            var propertyInfoPair = ExprIdentNodeUtil.GetTypeFromStream(
                validationContext.StreamTypeService,
                unresolvedPropertyName,
                streamOrPropertyName,
                false,
                validationContext.TableCompileTimeResolver);
            resolvedStreamName = propertyInfoPair.Second;
            var streamNum = propertyInfoPair.First.StreamNum;
            resolvedPropertyName = propertyInfoPair.First.PropertyName;
            var eventType = propertyInfoPair.First.StreamEventType;
            EventPropertyGetterSPI propertyGetter;
            try {
                propertyGetter = ((EventTypeSPI)eventType).GetGetterSPI(resolvedPropertyName);
            }
            catch (PropertyAccessException ex) {
                throw new ExprValidationException(
                    "Property '" + unresolvedPropertyName + "' is not valid: " + ex.Message,
                    ex);
            }

            if (propertyGetter == null) {
                throw new ExprValidationException(
                    "Property getter not available for property '" + unresolvedPropertyName + "'");
            }

            var audit = AuditEnum.PROPERTY.GetAudit(validationContext.Annotations) != null;
            var propertyTypeUnboxed = eventType.GetPropertyType(propertyInfoPair.First.PropertyName);
            var propertyType = propertyTypeUnboxed.GetBoxedType();
            evaluator = new ExprIdentNodeEvaluatorImpl(
                streamNum,
                propertyGetter,
                propertyType,
                this,
                (EventTypeSPI)eventType,
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
                    var contextType = (EventTypeSPI)validationContext.ContextDescriptor.ContextPropertyRegistry
                        .ContextEventType;
                    var contextPropertyType = contextType.GetPropertyType(contextPropertyName).GetBoxedType();
                    evaluator = new ExprIdentNodeEvaluatorContext(
                        streamNum,
                        contextPropertyType,
                        contextType.GetGetterSPI(contextPropertyName),
                        (EventTypeSPI)eventType);
                }
            }

            return null;
        }

        public bool IsConstantResult => false;

        /// <summary>
        /// Returns stream id supplying the property value.
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

        public int? StreamReferencedIfAny => StreamId;

        public string RootPropertyNameIfAny => ResolvedPropertyNameRoot;

        /// <summary>
        /// Returns stream name as resolved by lookup of property in streams.
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
        /// Return property name as resolved by lookup in streams.
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
        /// Returns the root of the resolved property name, if any.
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
                   unresolvedPropertyName +
                   " streamOrPropertyName=" +
                   streamOrPropertyName +
                   " resolvedPropertyName=" +
                   resolvedPropertyName;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ToPrecedenceFreeEPL(writer, streamOrPropertyName, unresolvedPropertyName, flags);
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

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprIdentNode other)) {
                return false;
            }

            if (ignoreStreamPrefix &&
                resolvedPropertyName != null &&
                other.ResolvedPropertyName != null &&
                resolvedPropertyName.Equals(other.ResolvedPropertyName)) {
                return true;
            }

            if (!streamOrPropertyName?.Equals(other.StreamOrPropertyName) ?? other.StreamOrPropertyName != null) {
                return false;
            }

            if (!unresolvedPropertyName?.Equals(other.UnresolvedPropertyName) ?? other.UnresolvedPropertyName != null) {
                return false;
            }

            return true;
        }

        public ExprIdentNodeEvaluator ExprEvaluatorIdent => evaluator;

        public ExprEnumerationForgeDesc GetEnumerationForge(
            StreamTypeService streamTypeService,
            ContextCompileTimeDescriptor contextDescriptor)
        {
            var fragmentEventType = evaluator.EventType.GetFragmentType(ResolvedPropertyName);
            if (fragmentEventType == null || fragmentEventType.IsIndexed) {
                return null;
            }

            var forge = new ExprIdentNodeFragmentTypeEnumerationForge(
                resolvedPropertyName,
                StreamId,
                fragmentEventType.FragmentType,
                evaluator.EventType.GetGetterSPI(resolvedPropertyName));
            return new ExprEnumerationForgeDesc(forge, streamTypeService.IStreamOnly[StreamId], -1);
        }
    }
} // end of namespace