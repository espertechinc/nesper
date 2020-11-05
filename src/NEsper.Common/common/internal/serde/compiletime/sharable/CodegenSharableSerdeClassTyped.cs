///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.serdeset.additional;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // newInstance;

namespace com.espertech.esper.common.@internal.serde.compiletime.sharable
{
	public class CodegenSharableSerdeClassTyped : CodegenFieldSharable
	{
		private readonly CodegenSharableSerdeName _name;
		private readonly Type _valueType;
		private readonly DataInputOutputSerdeForge _forge;
		private readonly CodegenClassScope _classScope;

		public CodegenSharableSerdeClassTyped(
			CodegenSharableSerdeName name,
			Type valueType,
			DataInputOutputSerdeForge forge,
			CodegenClassScope classScope)
		{
			this._name = name;
			this._valueType = valueType;
			this._forge = forge;
			this._classScope = classScope;
		}

		public Type Type()
		{
			return typeof(DataInputOutputSerde);
		}

		public CodegenExpression InitCtorScoped()
		{
			CodegenExpression serde = _forge.Codegen(_classScope.NamespaceScope.InitMethod, _classScope, null);
			if (_name == CodegenSharableSerdeName.VALUE_NULLABLE) {
				return serde;
			}
			else if (_name == CodegenSharableSerdeName.REFCOUNTEDSET) {
				return NewInstance(typeof(DIORefCountedSet), serde);
			}
			else if (_name == CodegenSharableSerdeName.SORTEDREFCOUNTEDSET) {
				return NewInstance(typeof(DIOSortedRefCountedSet), serde);
			}
			else {
				throw new ArgumentException("Unrecognized name " + _name);
			}
		}

		public override bool Equals(object o)
		{
			if (this == o) return true;
			if (o == null || GetType() != o.GetType()) return false;

			CodegenSharableSerdeClassTyped that = (CodegenSharableSerdeClassTyped) o;

			if (_name != that._name) return false;
			return _valueType.Equals(that._valueType);
		}

		public override int GetHashCode()
		{
			int result = _name.GetHashCode();
			result = 31 * result + _valueType.GetHashCode();
			return result;
		}

		public class CodegenSharableSerdeName
		{
			public static readonly CodegenSharableSerdeName VALUE_NULLABLE =
				new CodegenSharableSerdeName("valueNullable");

			public static readonly CodegenSharableSerdeName REFCOUNTEDSET =
				new CodegenSharableSerdeName("refCountedSet");

			public static readonly CodegenSharableSerdeName SORTEDREFCOUNTEDSET =
				new CodegenSharableSerdeName("sortedRefCountedSet");

			public string MethodName { get; }

			CodegenSharableSerdeName(string methodName)
			{
				this.MethodName = methodName;
			}
		}
	}
} // end of namespace
