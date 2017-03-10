namespace WhiteRabbit
{
    using System.Collections.Generic;
    using System.Linq;

    internal class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            if (x?.Length != y?.Length)
            {
                return false;
            }

            return Enumerable.Range(0, x.Length).All(i => x[i] == y[i]);
        }

        public int GetHashCode(byte[] obj)
        {
            if (obj == null)
            {
                return 0;
            }

            int result = 0;
            for (var i = 0; i < obj.Length; i++)
            {
                result = unchecked(result + (i * obj[i]));
            }

            return result;
        }
    }
}
