using System;
using System.Collections.Generic;


namespace LouvainCommunityPL
{
#if EVO3
    static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }
    }
#endif

#if EVO3
    public class Tuple<T1, T2> : IFormattable
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }

        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        #region Optional - If you need to use in dictionaries or check equality
        private static readonly IEqualityComparer<T1> Item1Comparer = EqualityComparer<T1>.Default;
        private static readonly IEqualityComparer<T2> Item2Comparer = EqualityComparer<T2>.Default;

        public override int GetHashCode()
        {
            var hc = 0;
            if (!object.ReferenceEquals(Item1, null))
                hc = Item1Comparer.GetHashCode(Item1);
            if (!object.ReferenceEquals(Item2, null))
                hc = (hc << 3) ^ Item2Comparer.GetHashCode(Item2);
            return hc;
        }
        public override bool Equals(object obj)
        {
            var other = obj as Tuple<T1, T2>;
            if (object.ReferenceEquals(other, null))
                return false;
            else
                return Item1Comparer.Equals(Item1, other.Item1) && Item2Comparer.Equals(Item2, other.Item2);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
       

   #endif
    
}