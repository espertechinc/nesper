///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client
{
    [Serializable]
    public class AutoImportDesc
    {
        /// <summary>
        /// Gets or sets the type of the namespace or.
        /// </summary>
        /// <value>The type of the namespace or.</value>
        public string TypeOrNamespace { get; set; }

        /// <summary>
        /// Gets or sets an optional assembly name or file.
        /// </summary>
        /// <value>The assembly name or file.</value>
        public string AssemblyNameOrFile { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoImportDesc"/> class.
        /// </summary>
        public AutoImportDesc()
        {
            TypeOrNamespace = null;
            AssemblyNameOrFile = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoImportDesc"/> class.
        /// </summary>
        /// <param name="namespaceOrType">Type of the namespace or.</param>
        public AutoImportDesc(string namespaceOrType)
        {
            TypeOrNamespace = namespaceOrType;
            AssemblyNameOrFile = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoImportDesc"/> class.
        /// </summary>
        /// <param name="namespaceOrType">Type of the namespace or.</param>
        /// <param name="assemblyNameOrFile">The assembly name or file.</param>
        public AutoImportDesc(string namespaceOrType, string assemblyNameOrFile)
        {
            TypeOrNamespace = namespaceOrType;
            AssemblyNameOrFile = assemblyNameOrFile;
        }

        /// <summary>
        /// Compares the equality of two <see cref="AutoImportDesc"/> instances.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(AutoImportDesc other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.TypeOrNamespace, TypeOrNamespace) && Equals(other.AssemblyNameOrFile, AssemblyNameOrFile);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (AutoImportDesc)) return false;
            return Equals((AutoImportDesc) obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked {
                return ((TypeOrNamespace != null ? TypeOrNamespace.GetHashCode() : 0)*397) ^ (AssemblyNameOrFile != null ? AssemblyNameOrFile.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("TypeOrNamespace: {0}, AssemblyNameOrFile: {1}", TypeOrNamespace, AssemblyNameOrFile);
        }
    }
}
