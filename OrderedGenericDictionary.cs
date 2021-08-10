using System.Collections.Specialized;

namespace JankShade
{
    public class OrderedGenericDictionary<T, U> : OrderedDictionary
    {
        public OrderedGenericDictionary() : base()
        {

        }

        public OrderedGenericDictionary(OrderedGenericDictionary<T, U> otherToCopy)
        {
            foreach (var item in otherToCopy)
            {
                Add(item, base[item]);
            }
        }

        public void Insert(int index , T key, U value)
        {
            Insert(index, (object)key, (object)value);
        }

        public bool ContainsKey(T key)
        {
            return Contains(key);
        }
    }
}
