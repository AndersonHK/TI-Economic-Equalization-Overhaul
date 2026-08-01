using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TIEconomyMod
{
    public sealed class ReferenceIdentityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        public static readonly ReferenceIdentityComparer<T> Instance =
            new ReferenceIdentityComparer<T>();

        private ReferenceIdentityComparer()
        {
        }

        public bool Equals(T left, T right)
        {
            return ReferenceEquals(left, right);
        }

        public int GetHashCode(T value)
        {
            return value == null ? 0 : RuntimeHelpers.GetHashCode(value);
        }
    }
}
