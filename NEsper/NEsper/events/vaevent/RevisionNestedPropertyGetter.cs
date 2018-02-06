///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    ///     A getter that works on PONO events residing within a Map as an event property.
    /// </summary>
    public class RevisionNestedPropertyGetter : EventPropertyGetterSPI
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventPropertyGetterSPI _nestedGetter;
        private readonly EventPropertyGetterSPI _revisionGetter;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="revisionGetter">getter for revision value</param>
        /// <param name="nestedGetter">getter to apply to revision value</param>
        /// <param name="eventAdapterService">for handling object types</param>
        public RevisionNestedPropertyGetter(EventPropertyGetterSPI revisionGetter, EventPropertyGetterSPI nestedGetter,
            EventAdapterService eventAdapterService)
        {
            _revisionGetter = revisionGetter;
            _eventAdapterService = eventAdapterService;
            _nestedGetter = nestedGetter;
        }

        public object Get(EventBean obj)
        {
            var result = _revisionGetter.Get(obj);
            if (result == null) return result;

            // Object within the map
            var theEvent = _eventAdapterService.AdapterForObject(result);
            return _nestedGetter.Get(theEvent);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public object GetFragment(EventBean eventBean)
        {
            return null; // no fragments provided by revision events
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return LocalMethod(GetCodegen(context), beanExpression);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            throw VAERevisionEventPropertyGetterDeclaredGetVersioned.RevisionImplementationNotProvided();
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            throw VAERevisionEventPropertyGetterDeclaredGetVersioned.RevisionImplementationNotProvided();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            throw VAERevisionEventPropertyGetterDeclaredGetVersioned.RevisionImplementationNotProvided();
        }

        private string GetCodegen(ICodegenContext context)
        {
            var mgetter = context.MakeAddMember(typeof(EventPropertyGetter), _nestedGetter);
            var msvc = context.MakeAddMember(typeof(EventAdapterService), _eventAdapterService);
            return context.AddMethod(typeof(object), typeof(EventBean), "obj", GetType())
                .DeclareVar(typeof(object), "result",
                    _revisionGetter.CodegenEventBeanGet(Ref("obj"), context))
                .IfRefNullReturnNull("result")
                .DeclareVar(typeof(EventBean), "theEvent",
                    ExprDotMethod(Ref(msvc.MemberName), "adapterForBean",
                        Ref("result")))
                .MethodReturn(ExprDotMethod(Ref(mgetter.MemberName), "get",
                    Ref("theEvent")));
        }
    }
} // end of namespace