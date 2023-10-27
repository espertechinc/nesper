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
using System.Linq;
using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.expression.ops
{
	/// <summary>
	/// Represents an array in a filter expressiun tree.
	/// </summary>
	public class ExprArrayNode : ExprNodeBase
	{

		[NonSerialized] [JsonIgnore] private ExprArrayNodeForge forge;
		private Type optionalRequiredType;

		/// <summary>
		/// Ctor.
		/// </summary>
		public ExprArrayNode()
		{
		}

		public ExprEvaluator ExprEvaluator {
			get {
				CheckValidated(forge);
				return forge.ExprEvaluator;
			}
		}

		public bool IsConstantResult {
			get {
				CheckValidated(forge);
				return forge.ConstantResult != null;
			}
		}

		public override ExprForge Forge {
			get {
				CheckValidated(forge);
				return forge;
			}
		}

		public Type ComponentTypeCollection {
			get {
				CheckValidated(forge);
				return forge.ArrayReturnType;
			}
		}

		public override ExprNode Validate(ExprValidationContext validationContext)
		{
			var length = this.ChildNodes.Length;

			// Can be an empty array with no content
			if (this.ChildNodes.Length == 0) {
				if (optionalRequiredType == null) {
					forge = new ExprArrayNodeForge(this, typeof(object), CollectionUtil.OBJECTARRAY_EMPTY);
				}
				else {
					forge = new ExprArrayNodeForge(
						this,
						optionalRequiredType,
						Arrays.CreateInstanceChecked(optionalRequiredType, 0));
				}

				return null;
			}

			IList<Type> comparedTypes = new List<Type>();
			for (var i = 0; i < length; i++) {
				var evalType = this.ChildNodes[i].Forge.EvaluationType;
				comparedTypes.Add(evalType);
			}

			// Determine common denominator type
			Type arrayReturnType = null;
			var mustCoerce = false;
			Coercer coercer = null;
			try {
				if (optionalRequiredType == null) {
					var coercionType = TypeHelper.GetCommonCoercionType(comparedTypes.ToArray());
					arrayReturnType = coercionType;

					// Determine if we need to coerce numbers when one type doesn't match any other type
					if (arrayReturnType.IsTypeNumeric()) {
						foreach (var comparedType in comparedTypes) {
							if (!(comparedType.Equals(arrayReturnType))) {
								mustCoerce = true;
								break;
							}
						}

						if (mustCoerce) {
							coercer = SimpleNumberCoercerFactory.GetCoercer(null, arrayReturnType);
						}
					}
				}
				else {
					arrayReturnType = optionalRequiredType;
					var arrayBoxedType = optionalRequiredType.GetBoxedType();
					foreach (var comparedType in comparedTypes) {
						if (!comparedType.GetBoxedType().IsAssignmentCompatible(arrayBoxedType)) {
							throw new ExprValidationException(
								"Array element type mismatch: Expecting type " +
								arrayReturnType.CleanName() +
								" but received type " +
								comparedType.CleanName());
						}
					}
				}

			}
			catch (CoercionException) {
				// expected, such as mixing String and int values, or Java classes (not boxed) and primitives
				// use Object[] in such cases
			}

			if (arrayReturnType == null) {
				arrayReturnType = typeof(object);
			}

			// Determine if we are dealing with constants only
			var results = new object[length];
			var index = 0;
			foreach (var child in this.ChildNodes) {
				if (!child.Forge.ForgeConstantType.IsCompileTimeConstant) {
					results = null; // not using a constant result
					break;
				}

				results[index] = ChildNodes[index].Forge.ExprEvaluator.Evaluate(null, false, null);
				index++;
			}

			// Copy constants into array and coerce, if required
			object constantResult = null;
			if (results != null) {
				var asArray = Arrays.CreateInstanceChecked(arrayReturnType, length);
				constantResult = asArray;
				for (var i = 0; i < length; i++) {
					if (mustCoerce) {
						var boxed = results[i];
						if (boxed != null) {
							var coercedResult = coercer.CoerceBoxed(boxed);
							asArray.SetValue(coercedResult, i);
						}
					}
					else {
						if (arrayReturnType.IsPrimitive && results[i] == null) {
							throw new ExprValidationException(
								"Array element type mismatch: Expecting type " +
								arrayReturnType.CleanName() +
								" but received null");
						}

						try {
							asArray.SetValue(results[i], i);
						}
						catch (ArgumentException) {
							throw new ExprValidationException(
								"Array element type mismatch: Expecting type " +
								arrayReturnType.CleanName() +
								" but received type " +
								results[i].GetType().CleanName());
						}
					}
				}
			}

			forge = new ExprArrayNodeForge(this, arrayReturnType, mustCoerce, coercer, constantResult);
			return null;
		}

		public override void ToPrecedenceFreeEPL(
			TextWriter writer,
			ExprNodeRenderableFlags flags)
		{
			var delimiter = "";
			writer.Write("{");
			foreach (var expr in this.ChildNodes) {
				writer.Write(delimiter);
				expr.ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
				delimiter = ",";
			}

			writer.Write('}');
		}

		public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

		public override bool EqualsNode(
			ExprNode node,
			bool ignoreStreamPrefix)
		{
			if (!(node is ExprArrayNode)) {
				return false;
			}

			return true;
		}

		public Type OptionalRequiredType {
			get => optionalRequiredType;
			set => this.optionalRequiredType = value;
		}
	}
} // end of namespace
