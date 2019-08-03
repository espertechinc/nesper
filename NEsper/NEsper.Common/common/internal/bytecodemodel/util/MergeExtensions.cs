using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.espertech.esper.common.@internal.bytecodemodel.util
{
    public static class MergeExtensions
    {
        /// <summary>
        /// Adds the type to set - and any constraints and/or generic types.
        /// </summary>
        /// <param name="typeSet">The type set.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static ISet<Type> AddToSet(this ISet<Type> typeSet, Type type)
        {
            if (type != null) {
                typeSet.Add(type);
                if (type.IsArray) {
                    AddToSet(typeSet, type.GetElementType());
                }
                else if (type.IsGenericType) {
                    foreach (var genericArgument in type.GetGenericArguments()) {
                        AddToSet(typeSet, genericArgument);
                    }

                    try {
                        foreach (var genericParameterConstraint in type.GetGenericParameterConstraints()) {
                            AddToSet(typeSet, genericParameterConstraint);
                        }
                    }
                    catch (InvalidOperationException) {
                    }
                }
            }

            return typeSet;
        }
    }
}
