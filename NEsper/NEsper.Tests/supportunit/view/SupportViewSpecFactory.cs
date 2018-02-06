///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.view;

namespace com.espertech.esper.supportunit.view
{
    /// <summary>Convenience class for making view specifications from class and string arrays. </summary>
    public class SupportViewSpecFactory
    {
        public static IList<ViewSpec> MakeSpecListOne()
        {
            List<ViewSpec> specifications = new List<ViewSpec>();
    
            ViewSpec specOne = MakeSpec("win", "length",
                    new Type[] { typeof(int)}, new String[] { "1000" } );
            ViewSpec specTwo = MakeSpec("stat", "uni",
                    new Type[] { typeof(String)}, new String[] { "IntPrimitive" } );
            ViewSpec specThree = MakeSpec("std", "lastevent", null, null);
    
            specifications.Add(specOne);
            specifications.Add(specTwo);
            specifications.Add(specThree);
    
            return specifications;
        }

        public static IList<ViewFactory> MakeFactoryListOne(EventType parentEventType)
        {
            return MakeFactories(parentEventType, MakeSpecListOne());
        }

        public static IList<ViewSpec> MakeSpecListTwo()
        {
            List<ViewSpec> specifications = new List<ViewSpec>();
    
            ViewSpec specOne = MakeSpec("std", "groupwin",
                    new Type[] { typeof(String) }, new String[] { "TheString" } );
            ViewSpec specTwo = MakeSpec("win", "length",
                    new Type[] { typeof(int) }, new String[] { "100" } );
    
            specifications.Add(specOne);
            specifications.Add(specTwo);
    
            return specifications;
        }
    
        public static IList<ViewFactory> MakeFactoryListTwo(EventType parentEventType)
        {
            return MakeFactories(parentEventType, MakeSpecListTwo());
        }
    
        public static List<ViewSpec> MakeSpecListThree()
        {
            List<ViewSpec> specifications = new List<ViewSpec>();
    
            ViewSpec specOne = SupportViewSpecFactory.MakeSpec("win", "length",
                    new Type[] { typeof(int)}, new String[] { "1000" } );
            ViewSpec specTwo = SupportViewSpecFactory.MakeSpec("std", "unique",
                    new Type[] { typeof(String)}, new String[] { "TheString" } );
    
            specifications.Add(specOne);
            specifications.Add(specTwo);
    
            return specifications;
        }
    
        public static IList<ViewFactory> MakeFactoryListThree(EventType parentEventType)
        {
            return MakeFactories(parentEventType, MakeSpecListThree());
        }
    
        public static IList<ViewSpec> MakeSpecListFour()
        {
            List<ViewSpec> specifications = new List<ViewSpec>();
    
            ViewSpec specOne = SupportViewSpecFactory.MakeSpec("win", "length",
                    new Type[] { typeof(int)}, new String[] { "1000" } );
            ViewSpec specTwo = SupportViewSpecFactory.MakeSpec("stat", "uni",
                    new Type[] { typeof(String)}, new String[] { "IntPrimitive" } );
            ViewSpec specThree = SupportViewSpecFactory.MakeSpec("std", "size", null, null);
    
            specifications.Add(specOne);
            specifications.Add(specTwo);
            specifications.Add(specThree);
    
            return specifications;
        }

        public static IList<ViewFactory> MakeFactoryListFour(EventType parentEventType)
        {
            return MakeFactories(parentEventType, MakeSpecListFour());
        }

        public static IList<ViewSpec> MakeSpecListFive()
        {
            List<ViewSpec> specifications = new List<ViewSpec>();
    
            ViewSpec specOne = MakeSpec("win", "time",
                    new Type[] { typeof(int)}, new String[] { "10000" } );
            specifications.Add(specOne);
    
            return specifications;
        }
    
        public static IList<ViewFactory> MakeFactoryListFive(EventType parentEventType)
        {
            return MakeFactories(parentEventType, MakeSpecListFive());
        }
    
        public static ViewSpec MakeSpec(String @namespace, String viewName, Type[] paramTypes, String[] paramValues)
        {
            return new ViewSpec(@namespace, viewName, MakeParams(paramTypes, paramValues));
        }
    
        private static IList<ExprNode> MakeParams(Type[] clazz, String[] values)
        {
            var parameters = new List<ExprNode>();
            if (values == null)
            {
                return parameters;
            }
    
            for (int i = 0; i < values.Length; i++)
            {
                ExprNode node;
                String value = values[i];
                if (clazz[i] == typeof(string))
                {
                    if (value.StartsWith("\""))
                    {
                        value = value.Replace("\"", "");
                        node = new ExprConstantNodeImpl(value);
                    }
                    else
                    {
                        node = SupportExprNodeFactory.MakeIdentNodeBean(value);
                    }
                }
                else if (clazz[i] == typeof(bool?))
                {
                    node = new ExprConstantNodeImpl(bool.Parse(value));
                }
                else
                {
                    node = new ExprConstantNodeImpl(int.Parse(value));
                }
                parameters.Add(node);
            }
    
            return parameters;
        }
    
        private static IList<ViewFactory> MakeFactories(EventType parentEventType, IList<ViewSpec> viewSpecs)
        {
            ViewServiceImpl svc = new ViewServiceImpl();
            ViewFactoryChain viewFactories = svc.CreateFactories(
                1, parentEventType, 
                ViewSpec.ToArray(viewSpecs),
                StreamSpecOptions.DEFAULT, 
                SupportStatementContextFactory.MakeContext(SupportContainer.Instance), 
                false, -1);
            return viewFactories.FactoryChain;
        }
    }
}
