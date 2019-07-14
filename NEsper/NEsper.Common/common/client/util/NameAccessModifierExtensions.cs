///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.util
{
    public static class NameAccessModifierExtensions
    {
        /// <summary>
        /// Returns indicator whether the object is visible.
        /// <para>Always false if the object is private or transient.</para>
        /// <para>Always true if the object is public or preconfigured.</para>
        /// <para>For protected the module name must match</para>
        /// </summary>
        /// <param name="objectVisibility">object visibility</param>
        /// <param name="objectModuleName">object module name</param>
        /// <param name="importModuleName">my module name</param>
        /// <returns>indicator</returns>
        public static bool Visible(
            NameAccessModifier objectVisibility,
            string objectModuleName,
            string importModuleName)
        {
            if (objectVisibility.IsPrivateOrTransient()) {
                return false;
            }

            if (objectVisibility == NameAccessModifier.PROTECTED) {
                return CompareModuleName(objectModuleName, importModuleName);
            }

            return true;
        }

        internal static bool CompareModuleName(
            string objectModuleName,
            string importModuleName)
        {
            if (objectModuleName == null && importModuleName == null)
            {
                return true;
            }

            if (objectModuleName != null && importModuleName != null)
            {
                return objectModuleName.Equals(importModuleName);
            }

            return false;
        }

        /// <summary>
        /// Returns true if the modifier can be used by modules i.e. returns true for private, protected and public.
        /// Returns false for preconfigured since preconfigured is reserved for configured objects.
        /// Returns false for transient as transient is reserved for internal use
        /// </summary>
        /// <returns>indicator</returns>
        public static bool IsModuleProvidedAccessModifier(this NameAccessModifier value)
        {
            switch (value)
            {
                case NameAccessModifier.PRIVATE:
                case NameAccessModifier.PROTECTED:
                case NameAccessModifier.PUBLIC:
                    return true;
                case NameAccessModifier.TRANSIENT:
                case NameAccessModifier.PRECONFIGURED:
                    return false;
            }

            throw new ArgumentException("invalid value", nameof(value));
        }

        /// <summary>
        /// Returns true for a public and protected and false for all others
        /// </summary>
        /// <returns>indicator</returns>
        public static bool IsNonPrivateNonTransient(this NameAccessModifier value)
        {
            switch (value)
            {
                case NameAccessModifier.TRANSIENT:
                case NameAccessModifier.PRIVATE:
                case NameAccessModifier.PRECONFIGURED:
                    return false;
                case NameAccessModifier.PROTECTED:
                case NameAccessModifier.PUBLIC:
                    return true;
            }

            throw new ArgumentException("invalid value", nameof(value));
        }

        /// <summary>
        /// Returns true for a private and transient and false for all others
        /// </summary>
        /// <returns>indicator</returns>
        public static bool IsPrivateOrTransient(this NameAccessModifier value)
        {
            switch (value)
            {
                case NameAccessModifier.TRANSIENT:
                case NameAccessModifier.PRIVATE:
                    return true;
                case NameAccessModifier.PROTECTED:
                case NameAccessModifier.PUBLIC:
                case NameAccessModifier.PRECONFIGURED:
                    return false;
            }

            throw new ArgumentException("invalid value", nameof(value));
        }
    }
}