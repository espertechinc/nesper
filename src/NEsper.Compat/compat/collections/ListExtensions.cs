///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.magic;

namespace com.espertech.esper.compat.collections
{
    public static class ListExtensions
    {
        public static IList<object> AsObjectList(this object value, MagicMarker magicMarker)
        {
            if (value == null) {
                return null;
            }
            else if (value is IList<object> asList) {
                return asList;
            }
            else if (value.GetType().IsGenericList()) {
                return magicMarker
                    .GetListFactory(value.GetType())
                    .Invoke(value);
            }

            throw new ArgumentException("invalid value for object list");
        }

        public static bool AreEqual<T>(IList<T> listThis, IList<T> listThat)
        {
            var listThisCount = listThis.Count;
            var listThatCount = listThat.Count;
            if (listThisCount != listThatCount)
            {
                return false;
            }

            for (var ii = 0; ii < listThisCount; ii++)
            {
                if (!Equals(listThis[ii], listThat[ii]))
                {
                    return false;
                }
            }

            return true;
        }
        
        public static IList<T> CopyList<T>(IList<T> sourceList)
        {
            return new List<T>(sourceList);
        }

        public static object TryCopy(object sourceList)
        {
            if (sourceList == null) {
                return null;
            }
            var sourceListType = sourceList.GetType();
            if (sourceListType.IsGenericList()) {
                var valType = sourceListType.GetListType();
                var method = typeof(ListExtensions).GetMethod("CopyList")?.MakeGenericMethod(valType);
                return method.Invoke(null, new[] { sourceList });
            }

            throw new ArgumentException("argument is not a dictionary", nameof(sourceList));
        }
    }
}
