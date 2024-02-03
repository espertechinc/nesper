///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
	public class StmtClassForgeableJsonDelegate : StmtClassForgeable
	{
		private readonly CodegenClassType _classType;
		private readonly string _className;
		private readonly CodegenNamespaceScope _namespaceScope;
		private readonly string _underlyingClassName;
		private readonly StmtClassForgeableJsonDesc _desc;

		public StmtClassForgeableJsonDelegate(
			CodegenClassType classType,
			string className,
			CodegenNamespaceScope namespaceScope,
			string underlyingClassName,
			StmtClassForgeableJsonDesc desc)
		{
			_classType = classType;
			_className = className;
			_namespaceScope = namespaceScope;
			_underlyingClassName = underlyingClassName;
			_desc = desc;
		}

		public CodegenClass Forge(
			bool includeDebugSymbols,
			bool fireAndForget)
		{
			var classScope = new CodegenClassScope(includeDebugSymbols, _namespaceScope, _className);
			
			// make members
			var members = new List<CodegenTypedParam>();
			
			// --------------------------------------------------------------------------------
			// Constructor
			// --------------------------------------------------------------------------------

			var ctorParams = new CodegenTypedParam[] {};
			var ctor = new CodegenCtor(typeof(StmtClassForgeableRSPFactoryProvider), classScope, ctorParams);

			// --------------------------------------------------------------------------------
			// TryGetProperty(string name, out object value)
			// --------------------------------------------------------------------------------

			var tryGetPropertyMethod = CodegenMethod
				.MakeParentNode(typeof(bool), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(new CodegenNamedParam(typeof(int), "index"))
				.AddParam(new CodegenNamedParam(typeof(object), "underlying"))
				.AddParam(new CodegenNamedParam(typeof(object), "value").WithOutputModifier());
			tryGetPropertyMethod = MakeTryGetProperty(tryGetPropertyMethod);

			// --------------------------------------------------------------------------------
			// TrySetProperty(string name, object underlying, object value)
			// --------------------------------------------------------------------------------

			var trySetPropertyMethod = CodegenMethod
				.MakeParentNode(typeof(bool), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(new CodegenNamedParam(typeof(int), "index"))
				.AddParam(new CodegenNamedParam(typeof(object), "value"))
				.AddParam(new CodegenNamedParam(typeof(object), "underlying"));

			trySetPropertyMethod = MakeTrySetProperty(trySetPropertyMethod);

			// --------------------------------------------------------------------------------
			// object TryCopy(object source)
			// --------------------------------------------------------------------------------

			var tryCopyMethod = CodegenMethod
				.MakeParentNode(typeof(object), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
				.AddParam(new CodegenNamedParam(typeof(object), "source"));
			tryCopyMethod = MakeTryCopy(tryCopyMethod, classScope);

			// --------------------------------------------------------------------------------
			// Allocator (property)
			// --------------------------------------------------------------------------------

			var allocateMethod = CodegenMethod
				.MakeParentNode(typeof(object), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope);
			allocateMethod = MakeAllocate(allocateMethod);

			// --------------------------------------------------------------------------------
			
			var properties = new CodegenClassProperties();

			// walk methods
			var methods = new CodegenClassMethods();
			CodegenStackGenerator.RecursiveBuildStack(tryGetPropertyMethod, "TryGetProperty", methods, properties);
			CodegenStackGenerator.RecursiveBuildStack(trySetPropertyMethod, "TrySetProperty", methods, properties);
			CodegenStackGenerator.RecursiveBuildStack(tryCopyMethod, "TryCopy", methods, properties);
			CodegenStackGenerator.RecursiveBuildStack(allocateMethod, "Allocate", methods, properties);

			var clazz = new CodegenClass(
				_classType,
				_className,
				classScope,
				members,
				ctor,
				methods,
				properties,
				EmptyList<CodegenInnerClass>.Instance);
			
			clazz.BaseList.AssignType(typeof(IJsonDelegate));

			return clazz;
		}

		private CodegenMethod MakeAllocate(CodegenMethod allocateMethod)
		{
			// we know this underlying class has a default constructor otherwise it is not json and deep-class eligible
			allocateMethod.Block.MethodReturn(NewInstanceInner(_underlyingClassName));
			return allocateMethod;
		}

		private CodegenMethod MakeTrySetProperty(
			CodegenMethod trySetPropertyMethod)
		{
			trySetPropertyMethod.Block
				.IfRefNullReturnFalse("underlying")
				.DeclareVar(_underlyingClassName, "und", Cast(_underlyingClassName, Ref("underlying")));

			var cases = _desc.PropertiesThisType
				.Select(_ => _desc.FieldDescriptorsInclSupertype.Get(_.Key))
				.Select(_ => Constant(_.PropertyNumber))
				.ToArray();
				
			var switchStmt = trySetPropertyMethod.Block.SwitchBlockExpressions(Ref("index"), cases, true, false);
			var switchIndx = 0;
				
			foreach (var property in _desc.PropertiesThisType) {
				var field = _desc.FieldDescriptorsInclSupertype.Get(property.Key);
				var fieldName = field.FieldName;
				var propertyRef = ExprDotName(Ref("und"), fieldName);
				var propertyValue = Cast(field.PropertyType, Ref("value"));

				switchStmt.Blocks[switchIndx++]
					.AssignRef(propertyRef, propertyValue)
					.BlockReturn(ConstantTrue());
			}

			switchStmt.DefaultBlock.BlockReturn(ConstantFalse());
			return trySetPropertyMethod;
		}

		private CodegenMethod MakeTryGetProperty(
			CodegenMethod method)
		{
			method.Block
				.AssignRef(Ref("value"), DefaultValue())
				.IfRefNullReturnFalse("underlying")
				.DeclareVar(_underlyingClassName, "und", Cast(_underlyingClassName, Ref("underlying")));

			var cases = _desc.PropertiesThisType
				.Select(_ => _desc.FieldDescriptorsInclSupertype.Get(_.Key))
				.Select(_ => Constant(_.PropertyNumber))
				.ToArray();
			
			var switchStmt = method.Block.SwitchBlockExpressions(Ref("index"), cases, true, false);
			var switchIndx = 0;
			
			foreach (var property in _desc.PropertiesThisType) {
				var field = _desc.FieldDescriptorsInclSupertype.Get(property.Key);
				var fieldName = field.FieldName;
				var propertyValue = ExprDotName(Ref("und"), fieldName);

				switchStmt.Blocks[switchIndx++]
					.AssignRef("value", propertyValue)
					.BlockReturn(ConstantTrue());
			}

			switchStmt.DefaultBlock.BlockReturn(ConstantFalse());

			return method;
		}

		private CodegenMethod MakeTryCopy(
			CodegenMethod tryCopyMethod,
			CodegenScope classScope)
		{
			tryCopyMethod.Block
				.IfRefNullReturnNull("source")
				.DeclareVar(_underlyingClassName, "src", Cast(_underlyingClassName, Ref("source")))
				.DeclareVar(_underlyingClassName, "dst", NewInstanceInner(_underlyingClassName));

			foreach (var property in _desc.PropertiesThisType) {
				var field = _desc.FieldDescriptorsInclSupertype.Get(property.Key);
				var fieldName = field.FieldName;
				var fieldType = field.PropertyType;

				var dstRef = ExprDotName(Ref("dst"), fieldName);
				var srcValue = ExprDotName(Ref("src"), fieldName);

				if (fieldType.IsArray) {
					var arrayCopy = tryCopyMethod
						.MakeChild(fieldType, GetType(), classScope)
						.AddParam(fieldType, "src");
					arrayCopy.Block
						.IfRefNullReturnNull("src")
						.DeclareVar(fieldType, "copy", NewArrayByLength(fieldType.GetElementType(), ArrayLength(Ref("src"))))
						.StaticMethod(typeof(Array), "Copy", Ref("src"), Constant(0), Ref("copy"), Constant(0), Constant(0))
						.MethodReturn(Ref("copy"));

					tryCopyMethod.Block.AssignRef(dstRef, LocalMethod(arrayCopy, srcValue));
				} else if (fieldType.IsInterface &&
				           fieldType.IsGenericType &&
				           fieldType.GetGenericTypeDefinition() == typeof(IDictionary<,>).GetGenericTypeDefinition()) {
					var keyType = fieldType.GetDictionaryKeyType();
					var valType = fieldType.GetDictionaryValueType();
					tryCopyMethod.Block.AssignRef(
						dstRef,
						StaticMethod(typeof(DictionaryExtensions), "CopyDictionary", new[] {keyType, valType}, srcValue));
				} else {
					tryCopyMethod.Block.AssignRef(dstRef, srcValue);
				}
			}

			tryCopyMethod.Block.MethodReturn(Ref("dst"));
			return tryCopyMethod;
		}

		public string ClassName => _className;

		public StmtClassForgeableType ForgeableType => StmtClassForgeableType.JSON_DELEGATE;
	}
} // end of namespace
