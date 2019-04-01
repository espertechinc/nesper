///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.codegen.core;
using com.espertech.esper.events;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.codegen.compile
{
    public static class CodegenEventPropertyGetter
    {
        public static EventPropertyGetter Compile(
            this ICodegenContext codegenContext,
            string engineURI, 
            ClassLoaderProvider classLoaderProvider, 
            EventPropertyGetterSPI getterSPI, 
            string propertyExpression)
        {
            var get = getterSPI.CodegenEventBeanGet(Ref("bean"), codegenContext);
            var exists = getterSPI.CodegenEventBeanExists(Ref("bean"), codegenContext);
            var fragment = getterSPI.CodegenEventBeanFragment(Ref("bean"), codegenContext);
    
            var singleBeanParam = CodegenNamedParam.From(typeof(EventBean), "bean");
    
            // For: public object Get(EventBean eventBean) ;
            // For: public bool IsExistsProperty(EventBean eventBean);
            // For: public object GetFragment(EventBean eventBean) ;
            var getMethod = new CodegenMethod(typeof(object), "Get", singleBeanParam, null);
            getMethod.Statements.MethodReturn(get);
            var isExistsPropertyMethod = new CodegenMethod(typeof(bool), "IsExistsProperty", singleBeanParam, null);
            isExistsPropertyMethod.Statements.MethodReturn(exists);
            var fragmentMethod = new CodegenMethod(typeof(object), "GetFragment", singleBeanParam, null);
            fragmentMethod.Statements.MethodReturn(fragment);
    
            var clazz = new CodegenClass(
                    "com.espertech.esper.codegen.uri_" + engineURI,
                    typeof(EventPropertyGetter).Name + "_" + CodeGenerationIDGenerator.GenerateClass(),
                    typeof(EventPropertyGetter),
                    codegenContext.Members,
                    new [] { getMethod, isExistsPropertyMethod, fragmentMethod },
                    codegenContext.Methods
            );
    
            string debugInfo = null;
            if (codegenContext.IsDebugEnabled) {
                debugInfo = getterSPI.GetType().FullName + " for property '" + propertyExpression + "'";
            }

            return codegenContext.Compiler.Compile(
                clazz, classLoaderProvider, typeof(EventPropertyGetter), debugInfo);
        }
    }
} // end of namespace
