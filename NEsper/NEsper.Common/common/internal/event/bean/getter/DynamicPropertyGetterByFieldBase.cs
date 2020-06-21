///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
	/// <summary>
	/// Base class for getters for a dynamic property (syntax field.inner?), caches methods to use for classes.
	/// </summary>
	public abstract class DynamicPropertyGetterByFieldBase : BeanEventPropertyGetter
	{
		private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
		private readonly BeanEventTypeFactory _beanEventTypeFactory;
		private readonly CopyOnWriteList<DynamicPropertyDescriptorByField> _cache;

		private readonly CodegenFieldSharable _sharableCode = new ProxyCodegenFieldSharable() {
			ProcType = () => typeof(CopyOnWriteList<DynamicPropertyDescriptorByField>),
			ProcInitCtorScoped = () => NewInstance(typeof(CopyOnWriteList<DynamicPropertyDescriptorByField>)),
		};

		public EventBeanTypedEventFactory EventBeanTypedEventFactory => _eventBeanTypedEventFactory;

		public BeanEventTypeFactory BeanEventTypeFactory => _beanEventTypeFactory;

		public CopyOnWriteList<DynamicPropertyDescriptorByField> Cache => _cache;

		public CodegenFieldSharable SharableCode => _sharableCode;

		/// <summary>
		/// To be implemented to return the field or property required, or null to indicate an
		/// appropriate field or property could not be found.
		/// </summary>
		/// <param name="clazz">to search for a matching field</param>
		/// <returns>field or property if found, or null if no matching exists</returns>
		protected abstract FieldInfo DetermineField(Type clazz);

		protected abstract CodegenExpression DetermineFieldCodegen(
			CodegenExpressionRef clazz,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope);

		/// <summary>
		/// Call the getter to obtains the return result object, or null if no such field exists.
		/// </summary>
		/// <param name="descriptor">provides field information for the class</param>
		/// <param name="underlying">is the underlying object to ask for the property value</param>
		/// <returns>underlying</returns>
		protected abstract object Call(
			DynamicPropertyDescriptorByField descriptor,
			object underlying);

		protected abstract CodegenExpression CallCodegen(
			CodegenExpressionRef desc,
			CodegenExpressionRef @object,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope);

		public abstract bool IsExistsProperty(EventBean eventBean);

		public abstract CodegenExpression UnderlyingExistsCodegen(
			CodegenExpression underlyingExpression,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope);

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="cache">cache</param>
		/// <param name="getter">getter</param>
		/// <param name="object">object</param>
		/// <param name="eventBeanTypedEventFactory">event server</param>
		/// <param name="beanEventTypeFactory">bean factory</param>
		/// <returns>property</returns>
		public static object CacheAndCall(
			CopyOnWriteList<DynamicPropertyDescriptorByField> cache,
			DynamicPropertyGetterByFieldBase getter,
			object @object,
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory)
		{
			var desc = GetPopulateCache(cache, getter, @object, eventBeanTypedEventFactory);
			if (desc.Field == null) {
				return null;
			}

			return getter.Call(desc, @object);
		}

		private CodegenExpression CacheAndCallCodegen(
			CodegenExpression underlyingExpression,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope)
		{
			CodegenExpression memberCache = codegenClassScope.AddOrGetDefaultFieldSharable(_sharableCode);
			var method = parent.MakeChild(typeof(object), typeof(DynamicPropertyGetterByFieldBase), codegenClassScope)
				.AddParam(typeof(object), "@object");
			method.Block
				.DeclareVar<DynamicPropertyDescriptorByField>("desc", GetPopulateCacheCodegen(memberCache, Ref("@object"), method, codegenClassScope))
				.IfCondition(EqualsNull(ExprDotName(Ref("desc"), "Field")))
				.BlockReturn(ConstantNull())
				.MethodReturn(CallCodegen(Ref("desc"), Ref("@object"), method, codegenClassScope));
			return LocalMethod(method, underlyingExpression);
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="cache">cache</param>
		/// <param name="getter">getter</param>
		/// <param name="object">object</param>
		/// <param name="eventBeanTypedEventFactory">event server</param>
		/// <returns>exists-flag</returns>
		public static bool CacheAndExists(
			CopyOnWriteList<DynamicPropertyDescriptorByField> cache,
			DynamicPropertyGetterByFieldBase getter,
			object @object,
			EventBeanTypedEventFactory eventBeanTypedEventFactory)
		{
			var desc = GetPopulateCache(cache, getter, @object, eventBeanTypedEventFactory);
			if (desc.Field == null) {
				return false;
			}

			return true;
		}

		protected CodegenExpression CacheAndExistsCodegen(
			CodegenExpression underlyingExpression,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope)
		{
			CodegenExpression memberCache = codegenClassScope.AddOrGetDefaultFieldSharable(_sharableCode);
			var method = parent.MakeChild(typeof(bool), typeof(DynamicPropertyGetterByFieldBase), codegenClassScope)
				.AddParam(typeof(object), "@object");
			method.Block
				.DeclareVar<DynamicPropertyDescriptorByField>("desc", GetPopulateCacheCodegen(memberCache, Ref("@object"), method, codegenClassScope))
				.IfCondition(EqualsNull(ExprDotName(Ref("desc"), "Field")))
				.BlockReturn(ConstantFalse())
				.MethodReturn(Constant(true));
			return LocalMethod(method, underlyingExpression);
		}

		public DynamicPropertyGetterByFieldBase(
			EventBeanTypedEventFactory eventBeanTypedEventFactory,
			BeanEventTypeFactory beanEventTypeFactory)
		{
			this._beanEventTypeFactory = beanEventTypeFactory;
			_cache = new CopyOnWriteList<DynamicPropertyDescriptorByField>();
			this._eventBeanTypedEventFactory = eventBeanTypedEventFactory;
		}

		public object GetBeanProp(object @object) {
			return CacheAndCall(_cache, this, @object, _eventBeanTypedEventFactory, _beanEventTypeFactory);
		}

		public Type TargetType => typeof(object);

		public bool IsBeanExistsProperty(object @object)
		{
			return CacheAndExists(_cache, this, @object, _eventBeanTypedEventFactory);
		}

		public object Get(EventBean @event)
		{
			return CacheAndCall(_cache, this, @event.Underlying, _eventBeanTypedEventFactory, _beanEventTypeFactory);
		}

		public Type BeanPropType => typeof(object);

		public CodegenExpression EventBeanGetCodegen(
			CodegenExpression beanExpression,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			return UnderlyingGetCodegen(ExprDotUnderlying(beanExpression), codegenMethodScope, codegenClassScope);
		}

		public CodegenExpression EventBeanExistsCodegen(
			CodegenExpression beanExpression,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			return UnderlyingExistsCodegen(ExprDotUnderlying(beanExpression), codegenMethodScope, codegenClassScope);
		}

		public CodegenExpression EventBeanFragmentCodegen(
			CodegenExpression beanExpression,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			return UnderlyingFragmentCodegen(ExprDotUnderlying(beanExpression), codegenMethodScope, codegenClassScope);
		}

		public CodegenExpression UnderlyingGetCodegen(
			CodegenExpression underlyingExpression,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			return CacheAndCallCodegen(underlyingExpression, codegenMethodScope, codegenClassScope);
		}

		public CodegenExpression UnderlyingFragmentCodegen(
			CodegenExpression underlyingExpression,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			return ConstantNull();
		}

		public object GetFragment(EventBean eventBean)
		{
			var result = Get(eventBean);
			return BaseNativePropertyGetter.GetFragmentDynamic(result, _eventBeanTypedEventFactory, _beanEventTypeFactory);
		}

		internal static DynamicPropertyDescriptorByField GetPopulateCache(
			CopyOnWriteList<DynamicPropertyDescriptorByField> cache,
			DynamicPropertyGetterByFieldBase dynamicPropertyGetterBase,
			object obj,
			EventBeanTypedEventFactory eventBeanTypedEventFactory)
		{
			var desc = DynamicPropertyCacheCheck(cache, obj);
			if (desc != null) {
				return desc;
			}

			// need to add it
			lock (dynamicPropertyGetterBase) {
				desc = DynamicPropertyCacheCheck(cache, obj);
				if (desc != null) {
					return desc;
				}

				// Lookup method to use
				var field = dynamicPropertyGetterBase.DetermineField(obj.GetType());

				// Cache descriptor and create field
				desc = DynamicPropertyCacheAdd(obj.GetType(), field, cache);
				return desc;
			}
		}

		protected CodegenExpression GetPopulateCacheCodegen(
			CodegenExpression memberCache,
			CodegenExpressionRef @object,
			CodegenMethodScope parent,
			CodegenClassScope codegenClassScope)
		{
			var method = parent
				.MakeChild(typeof(DynamicPropertyDescriptorByField), typeof(DynamicPropertyGetterByFieldBase), codegenClassScope)
				.AddParam(typeof(CopyOnWriteList<DynamicPropertyDescriptorByField>), "cache")
				.AddParam(typeof(object), "obj");
			method.Block
				.DeclareVar(
					typeof(DynamicPropertyDescriptorByField),
					"desc",
					StaticMethod(typeof(DynamicPropertyGetterByFieldBase), "DynamicPropertyCacheCheck", Ref("cache"), Ref("obj")))
				.IfRefNotNull("desc")
				.BlockReturn(Ref("desc"))
				.DeclareVar<Type>("clazz", ExprDotMethod(Ref("obj"), "GetType"))
				.DeclareVar<FieldInfo>("field", DetermineFieldCodegen(Ref("clazz"), method, codegenClassScope))
				.AssignRef(
					"desc",
					StaticMethod(typeof(DynamicPropertyGetterByFieldBase), "DynamicPropertyCacheAdd", Ref("clazz"), Ref("field"), Ref("cache")))
				.MethodReturn(Ref("desc"));
			return LocalMethod(method, memberCache, @object);
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="obj">target</param>
		/// <param name="cache">cache</param>
		/// <returns>descriptor</returns>
		public static DynamicPropertyDescriptorByField DynamicPropertyCacheCheck(
			CopyOnWriteList<DynamicPropertyDescriptorByField> cache,
			object obj)
		{
			// Check if the method is already there
			var target = obj.GetType();
			foreach (var desc in cache) {
				if (desc.Clazz == target) {
					return desc;
				}
			}

			return null;
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="clazz">class</param>
		/// <param name="field">field</param>
		/// <param name="cache">cache</param>
		/// <returns>descriptor</returns>
		public static DynamicPropertyDescriptorByField DynamicPropertyCacheAdd(
			Type clazz,
			FieldInfo field,
			CopyOnWriteList<DynamicPropertyDescriptorByField> cache)
		{
			var propertyDescriptor = new DynamicPropertyDescriptorByField(clazz, field);
			cache.Add(propertyDescriptor);
			return propertyDescriptor;
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="descriptor">descriptor</param>
		/// <param name="underlying">underlying</param>
		/// <param name="ex">throwable</param>
		/// <returns>exception</returns>
		public static PropertyAccessException HandleException(
			DynamicPropertyDescriptorByField descriptor,
			object underlying,
			Exception ex)
		{
			if (ex is InvalidCastException invalidCastException) {
				throw PropertyUtility.GetMismatchException(descriptor.Field, underlying, invalidCastException);
			}

			if (ex is ArgumentException argumentException) {
				throw PropertyUtility.GetArgumentException(descriptor.Field, argumentException);
			}

			if (ex is MemberAccessException memberAccessException) {
				throw PropertyUtility.GetMemberAccessException(descriptor.Field, memberAccessException);
			}

			throw PropertyUtility.GetGeneralException(descriptor.Field, ex);
		}
	}
} // end of namespace
