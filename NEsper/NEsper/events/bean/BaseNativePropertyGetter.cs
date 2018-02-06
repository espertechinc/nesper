///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.bean
{
    /// <summary>Base getter for native fragments.</summary>
    public abstract class BaseNativePropertyGetter : EventPropertyGetterSPI
    {
        private readonly EventAdapterService _eventAdapterService;
        private volatile BeanEventType _fragmentEventType;
        private readonly Type _fragmentClassType;
        private bool _isFragmentable;
        private readonly bool _isArray;
        private readonly bool _isIterable;

        public abstract Type TargetType { get; }

        public abstract Type BeanPropType { get; }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="array">array</param>
        /// <param name="fragmentEventType">fragment type</param>
        /// <param name="eventAdapterService">event adapters</param>
        /// <returns>array</returns>
        public static Object ToFragmentArray(Array array, BeanEventType fragmentEventType, EventAdapterService eventAdapterService)
        {
            var events = new EventBean[array.Length];
            int countFilled = 0;

            for (int i = 0; i < array.Length; i++)
            {
                var element = array.GetValue(i);
                if (element == null)
                {
                    continue;
                }

                events[countFilled] = eventAdapterService.AdapterForTypedObject(element, fragmentEventType);
                countFilled++;
            }

            if (countFilled == array.Length)
            {
                return events;
            }

            if (countFilled == 0)
            {
                return new EventBean[0];
            }

            var returnVal = new EventBean[countFilled];
            Array.Copy(events, 0, returnVal, 0, countFilled);
            return returnVal;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// Returns the fragment for dynamic properties.
        /// </summary>
        /// <param name="object">to inspect</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <returns>fragment</returns>
        public static Object GetFragmentDynamic(Object @object, EventAdapterService eventAdapterService)
        {
            if (@object == null)
            {
                return null;
            }

            BeanEventType fragmentEventType = null;
            bool isArray = false;
            if (@object.GetType().IsArray)
            {
                if (@object.GetType().GetElementType().IsFragmentableType())
                {
                    isArray = true;
                    fragmentEventType = eventAdapterService.BeanEventTypeFactory.CreateBeanTypeDefaultName(@object.GetType().GetElementType());
                }
            }
            else
            {
                if (@object.GetType().IsFragmentableType())
                {
                    fragmentEventType = eventAdapterService.BeanEventTypeFactory.CreateBeanTypeDefaultName(@object.GetType());
                }
            }

            if (fragmentEventType == null)
            {
                return null;
            }

            if (isArray)
            {
                var array = @object as Array;
                int len = array.Length;
                var events = new EventBean[len];
                int countFilled = 0;

                for (int i = 0; i < len; i++)
                {
                    var element = array.GetValue(i);
                    if (element == null)
                    {
                        continue;
                    }

                    events[countFilled] = eventAdapterService.AdapterForTypedObject(element, fragmentEventType);
                    countFilled++;
                }

                if (countFilled == len)
                {
                    return events;
                }

                if (countFilled == 0)
                {
                    return new EventBean[0];
                }

                var returnVal = new EventBean[countFilled];
                Array.Copy(events, 0, returnVal, 0, countFilled);
                return returnVal;
            }
            else
            {
                return eventAdapterService.AdapterForTypedObject(@object, fragmentEventType);
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// Returns the fragment for dynamic properties.
        /// </summary>
        /// <param name="object">to inspect</param>
        /// <param name="fragmentEventType">type</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <returns>fragment</returns>
        public static Object ToFragmentIterable(Object @object, BeanEventType fragmentEventType, EventAdapterService eventAdapterService)
        {
            if (!(@object is IEnumerable))
            {
                return null;
            }
            var enumerator = ((IEnumerable) @object).GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return new EventBean[0];
            }

            var events = new ArrayDeque<EventBean>();
            do
            {
                var next = enumerator.Current;
                if (next == null)
                {
                    continue;
                }

                events.Add(eventAdapterService.AdapterForTypedObject(next, fragmentEventType));
            }
            while (enumerator.MoveNext());

            return events.ToArray();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="returnType">type of the entry returned</param>
        /// <param name="genericType">type generic parameter, if any</param>
        protected BaseNativePropertyGetter(EventAdapterService eventAdapterService, Type returnType, Type genericType)
        {
            _eventAdapterService = eventAdapterService;

            if (returnType.IsArray)
            {
                _fragmentClassType = returnType.GetElementType();
                _isArray = true;
                _isIterable = false;
            }
            else if (returnType.IsImplementsInterface(typeof(IEnumerable)))
            {
                _fragmentClassType = genericType;
                _isArray = false;
                _isIterable = true;
            }
            else
            {
                _fragmentClassType = returnType;
                _isArray = false;
                _isIterable = false;
            }
            _isFragmentable = true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            DetermineFragmentable();
            if (!_isFragmentable)
            {
                return null;
            }

            var @object = Get(eventBean);
            if (@object == null)
            {
                return null;
            }

            if (_isArray)
            {
                return ToFragmentArray((Object[])@object, _fragmentEventType, _eventAdapterService);
            }
            else if (_isIterable)
            {
                return ToFragmentIterable(@object, _fragmentEventType, _eventAdapterService);
            }
            else
            {
                return _eventAdapterService.AdapterForTypedObject(@object, _fragmentEventType);
            }
        }

        private string GetFragmentCodegen(ICodegenContext context)
        {
            var msvc = context.MakeAddMember(typeof(EventAdapterService), _eventAdapterService);
            var mtype = context.MakeAddMember(typeof(BeanEventType), _fragmentEventType);

            var block = context.AddMethod(typeof(Object), TargetType, "underlying", this.GetType())
                    .DeclareVar(BeanPropType, "object", CodegenUnderlyingGet(Ref("underlying"), context))
                    .IfRefNullReturnNull("object");

            if (_isArray)
            {
                return block.MethodReturn(StaticMethod(
                    typeof(BaseNativePropertyGetter), "ToFragmentArray",
                    Cast(typeof(Object[]), Ref("object")),
                    Ref(mtype.MemberName),
                    Ref(msvc.MemberName)));
            }
            if (_isIterable)
            {
                return block.MethodReturn(StaticMethod(
                    typeof(BaseNativePropertyGetter), "ToFragmentIterable",
                    Ref("object"),
                    Ref(mtype.MemberName),
                    Ref(msvc.MemberName)));
            }
            return block.MethodReturn(ExprDotMethod(
                Ref(msvc.MemberName), "AdapterForTypedBean",
                Ref("object"),
                Ref(mtype.MemberName)));
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            DetermineFragmentable();
            if (!_isFragmentable)
            {
                return ConstantNull();
            }
            return CodegenUnderlyingFragment(CastUnderlying(TargetType, beanExpression), context);
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            DetermineFragmentable();
            if (!_isFragmentable)
            {
                return ConstantNull();
            }
            return LocalMethod(GetFragmentCodegen(context), underlyingExpression);
        }

        private void DetermineFragmentable()
        {
            if (_fragmentEventType == null)
            {
                if (_fragmentClassType.IsFragmentableType())
                {
                    _fragmentEventType = _eventAdapterService.BeanEventTypeFactory.CreateBeanTypeDefaultName(_fragmentClassType);
                }
                else
                {
                    _isFragmentable = false;
                }
            }
        }

        public abstract ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context);
        public abstract ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context);
        public abstract ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context);
        public abstract ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression, ICodegenContext context);
        public abstract object Get(EventBean eventBean);
        public abstract bool IsExistsProperty(EventBean eventBean);
    }
} // end of namespace