using System;

namespace com.espertech.esper.compat.attributes
{
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property)]
    public class RequiredAttribute : Attribute
    {
    }
}