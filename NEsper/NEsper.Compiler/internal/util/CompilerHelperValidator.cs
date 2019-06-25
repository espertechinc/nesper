///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilerHelperValidator
    {
        public static void VerifySubstitutionParams(IList<ExprSubstitutionNode> substitutionParameters)
        {
            if (substitutionParameters.IsEmpty()) {
                return;
            }

            IDictionary<string, Type> named = new Dictionary<string, Type>();
            IList<Type> unnamed = new List<Type>();

            foreach (var node in substitutionParameters) {
                if (node.OptionalName != null) {
                    var name = node.OptionalName;
                    var existing = named.Get(name);
                    if (existing == null) {
                        named.Put(name, node.ResolvedType);
                    }
                    else {
                        if (!TypeHelper.IsSubclassOrImplementsInterface(node.ResolvedType, existing)) {
                            throw new ExprValidationException(
                                "Substitution parameter '" + name + "' incompatible type assignment between types '" + existing.Name + "' and '" +
                                node.ResolvedType.Name + "'");
                        }
                    }
                }
                else {
                    unnamed.Add(node.ResolvedType);
                }
            }

            if (!unnamed.IsEmpty() && !named.IsEmpty()) {
                throw new ExprValidationException(
                    "Inconsistent use of substitution parameters, expecting all substitutions to either all provide a name or provide no name");
            }
        }
    }
} // end of namespace