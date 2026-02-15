using System.Collections.Generic;

namespace SpektraGames.SpektraUtilities.Runtime
{
    [System.Serializable]
    public class JsonArrayWrapper<T>
    {
        public T[] array;
        public JsonArrayWrapper(T[] array) => this.array = array;
    }
}