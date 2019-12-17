using System;
using System.Collections;
using System.IO;

namespace com.espertech.esper.compat.collections
{
    public interface IValueRenderer
    {
        /// <summary>
        /// Render any value.
        /// </summary>
        /// <param name="value">a value</param>
        /// <param name="textWriter">the text writer to write to</param>
        void RenderAny(
            object value,
            TextWriter textWriter);

         /// <summary>
         /// Render any value.
         /// </summary>
         /// <param name="value">a value</param>
         string RenderAny(object value);

        /// <summary>
        ///     Renders the array as a string.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        string Render(Array array);

        /// <summary>
        ///     Renders the array as a string.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="textWriter">the destination to write to.</param>
        /// <returns></returns>
        void Render(
            Array array,
            TextWriter textWriter);

        /// <summary>
        ///     Renders the array as a string
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="textWriter">Destination for the content.</param>
        /// <param name="itemSeparator">The item separator.</param>
        /// <param name="firstAndLast">The first and last.</param>
        /// <returns></returns>
        void Render(
            Array array,
            TextWriter textWriter,
            string itemSeparator,
            string firstAndLast);

        /// <summary>
        ///     Renders the array as a string
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="itemSeparator">The item separator.</param>
        /// <param name="firstAndLast">The first and last.</param>
        /// <returns></returns>
        string Render(
            Array array,
            string itemSeparator,
            string firstAndLast);

        /// <summary>
        ///     Renders an enumerable source
        /// </summary>
        /// <param name="source">the object to render.</param>
        /// <param name="textWriter">the destination to write to.</param>
        /// <returns></returns>
        void Render(
            IEnumerable source,
            TextWriter textWriter);

        /// <summary>
        ///     Renders an enumerable source
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="renderEngine">The render engine.</param>
        /// <returns></returns>
        string Render(
            IEnumerable source,
            Func<object, string> renderEngine);
    }
}