namespace com.espertech.esper.common.client.artifact
{
    public interface IStore
    {
        /// <summary>
        /// Retrieves the image.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        byte[] Load(string id);
        
        /// <summary>
        /// Stores the image.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        string Store(byte[] image);
    }
}