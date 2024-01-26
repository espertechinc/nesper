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

using com.espertech.esper.common.client;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
	public class SelectEvalStreamWUndRecastJsonFactory
	{

		public static SelectExprProcessorForge Make(
			EventType[] eventTypes,
			SelectExprForgeContext selectExprForgeContext,
			int streamNumber,
			EventType targetType,
			ExprNode[] exprNodes,
			ImportServiceCompileTime importService,
			string statementName)
		{
			var jsonResultType = (JsonEventType)targetType;
			var jsonStreamType = (JsonEventType)eventTypes[streamNumber];

			// (A) fully assignment-compatible: same number, name and type of fields, no additional expressions: Straight repackage
			if (jsonResultType.IsDeepEqualsConsiderOrder(jsonStreamType) &&
			    selectExprForgeContext.ExprForges.Length == 0) {
				return new JsonInsertProcessorStraightFieldAssign(streamNumber, jsonStreamType, jsonResultType);
			}

			// (B) not completely assignable: find matching properties
			var writables =
				EventTypeUtility.GetWriteableProperties(jsonResultType, true, false);
			IList<Item> items = new List<Item>();
			IList<WriteablePropertyDescriptor> written = new List<WriteablePropertyDescriptor>();

			// find the properties coming from the providing source stream
			foreach (var writeable in writables) {
				var propertyName = writeable.PropertyName;

				var fieldSource = jsonStreamType.Detail.FieldDescriptors.Get(propertyName);
				var fieldTarget = jsonResultType.Detail.FieldDescriptors.Get(propertyName);

				if (fieldSource != null) {
					var setOneType = jsonResultType.Types.Get(propertyName);
					var setOneTypeFound = jsonResultType.Types.ContainsKey(propertyName);
					var setTwoType = jsonStreamType.Types.Get(propertyName);
					var message = BaseNestableEventUtil.ComparePropType(
						propertyName,
						setOneType,
						setOneTypeFound,
						setTwoType,
						jsonResultType.Name);
					if (message != null) {
						throw new ExprValidationException(message.Message, message);
					}

					items.Add(new Item(fieldTarget, fieldSource, null, null));
					written.Add(writeable);
				}
			}

			// find the properties coming from the expressions of the select clause
			for (var i = 0; i < selectExprForgeContext.ExprForges.Length; i++) {
				var columnName = selectExprForgeContext.ColumnNames[i];
				var forge = selectExprForgeContext.ExprForges[i];
				var exprNode = exprNodes[i];

				var writable = FindWritable(columnName, writables);
				if (writable == null) {
					throw new ExprValidationException(
						"Failed to find column '" + columnName + "' in target type '" + jsonResultType.Name + "'");
				}

				var fieldTarget = jsonResultType.Detail.FieldDescriptors.Get(writable.PropertyName);

				TypeWidenerSPI widener;
				try {
					widener = TypeWidenerFactory.GetCheckPropertyAssignType(
						ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(exprNode),
						exprNode.Forge.EvaluationType,
						writable.PropertyType,
						columnName,
						false,
						null,
						statementName);
				}
				catch (TypeWidenerException ex) {
					throw new ExprValidationException(ex.Message, ex);
				}

				items.Add(new Item(fieldTarget, null, forge, widener));
				written.Add(writable);
			}

			// make manufacturer
			var itemsArr = items.ToArray();
			return new JsonInsertProcessorExpressions(streamNumber, itemsArr, jsonStreamType, jsonResultType);
		}

		private static WriteablePropertyDescriptor FindWritable(
			string columnName,
			ISet<WriteablePropertyDescriptor> writables)
		{
			foreach (var writable in writables) {
				if (writable.PropertyName.Equals(columnName)) {
					return writable;
				}
			}

			return null;
		}

		internal class JsonInsertProcessorStraightFieldAssign : SelectExprProcessorForge
		{
			private readonly int _underlyingStreamNumber;
			private readonly JsonEventType _sourceType;
			private readonly JsonEventType _resultType;

			internal JsonInsertProcessorStraightFieldAssign(
				int underlyingStreamNumber,
				JsonEventType sourceType,
				JsonEventType resultType)
			{
				_underlyingStreamNumber = underlyingStreamNumber;
				_sourceType = sourceType;
				_resultType = resultType;
			}

			public EventType ResultEventType => _resultType;

			public CodegenMethod ProcessCodegen(
				CodegenExpression resultEventType,
				CodegenExpression eventBeanFactory,
				CodegenMethodScope codegenMethodScope,
				SelectExprProcessorCodegenSymbol selectSymbol,
				ExprForgeCodegenSymbol exprSymbol,
				CodegenClassScope codegenClassScope)
			{
				var methodNode = codegenMethodScope.MakeChild(
					typeof(EventBean),
					GetType(),
					codegenClassScope);
				var refEPS = exprSymbol.GetAddEps(methodNode);
				methodNode.Block
					.DeclareVar(_resultType.UnderlyingType, "und", NewInstance(_resultType.UnderlyingType))
					.DeclareVar(
						_sourceType.UnderlyingType,
						"src",
						CastUnderlying(
							_sourceType.UnderlyingType,
							ArrayAtIndex(refEPS, Constant(_underlyingStreamNumber))));
				foreach (var sourceFieldEntry in _sourceType.Detail
					         .FieldDescriptors) {
					var targetField = _resultType.Detail.FieldDescriptors.Get(sourceFieldEntry.Key);
					methodNode.Block.AssignRef(
						"und." + targetField.FieldName,
						Ref("src." + sourceFieldEntry.Value.FieldName));
				}

				methodNode.Block.MethodReturn(
					ExprDotMethod(eventBeanFactory, "AdapterForTypedJson", Ref("und"), resultEventType));
				return methodNode;
			}
		}

		internal class JsonInsertProcessorExpressions : SelectExprProcessorForge
		{
			private readonly int _underlyingStreamNumber;
			private readonly Item[] _items;
			private readonly JsonEventType _sourceType;
			private readonly JsonEventType _resultType;

			internal JsonInsertProcessorExpressions(
				int underlyingStreamNumber,
				Item[] items,
				JsonEventType sourceType,
				JsonEventType resultType)
			{
				_underlyingStreamNumber = underlyingStreamNumber;
				_items = items;
				_sourceType = sourceType;
				_resultType = resultType;
			}

			public EventType ResultEventType => _resultType;

			public CodegenMethod ProcessCodegen(
				CodegenExpression resultEventType,
				CodegenExpression eventBeanFactory,
				CodegenMethodScope codegenMethodScope,
				SelectExprProcessorCodegenSymbol selectSymbol,
				ExprForgeCodegenSymbol exprSymbol,
				CodegenClassScope codegenClassScope)
			{
				var methodNode = codegenMethodScope.MakeChild(
					typeof(EventBean),
					GetType(),
					codegenClassScope);
				var refEPS = exprSymbol.GetAddEps(methodNode);
				var block = methodNode.Block
					.DeclareVar(
						_sourceType.UnderlyingType,
						"src",
						CastUnderlying(
							_sourceType.UnderlyingType,
							ArrayAtIndex(refEPS, Constant(_underlyingStreamNumber))))
					.DeclareVar(_resultType.UnderlyingType, "und", NewInstance(_resultType.UnderlyingType));
				foreach (var item in _items) {
					if (item.OptionalFromField != null) {
						block.AssignRef(
							"und." + item.ToField.FieldName,
							Ref("src." + item.OptionalFromField.FieldName));
					}
					else {
						CodegenExpression value;
						if (item.OptionalWidener != null) {
							value = item.Forge.EvaluateCodegen(
								item.Forge.EvaluationType,
								methodNode,
								exprSymbol,
								codegenClassScope);
							value = item.OptionalWidener.WidenCodegen(value, methodNode, codegenClassScope);
						}
						else {
							value = item.Forge.EvaluateCodegen(
								typeof(object),
								methodNode,
								exprSymbol,
								codegenClassScope);
						}

						block.AssignRef("und." + item.ToField.FieldName, value);
					}
				}

				methodNode.Block.MethodReturn(
					ExprDotMethod(eventBeanFactory, "AdapterForTypedJson", Ref("und"), resultEventType));
				return methodNode;
			}
		}

		internal class Item
		{
			private readonly JsonUnderlyingField _toField;
			private readonly JsonUnderlyingField _optionalFromField;
			private readonly ExprForge _forge;
			private readonly TypeWidenerSPI _optionalWidener;

			private ExprEvaluator _evaluatorAssigned;

			internal Item(
				JsonUnderlyingField toField,
				JsonUnderlyingField optionalFromField,
				ExprForge forge,
				TypeWidenerSPI optionalWidener)
			{
				if (toField == null) {
					throw new ArgumentException("Null to-field");
				}

				_toField = toField;
				_optionalFromField = optionalFromField;
				_forge = forge;
				_optionalWidener = optionalWidener;
			}

			public JsonUnderlyingField ToField => _toField;

			public JsonUnderlyingField OptionalFromField => _optionalFromField;

			public ExprForge Forge => _forge;

			public TypeWidenerSPI OptionalWidener => _optionalWidener;

			public ExprEvaluator EvaluatorAssigned {
				get => _evaluatorAssigned;
				set => _evaluatorAssigned = value;
			}
		}
	}
} // end of namespace
