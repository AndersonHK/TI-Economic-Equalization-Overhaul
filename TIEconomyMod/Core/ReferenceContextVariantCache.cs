using System;

namespace TIEconomyMod
{
    public sealed class ReferenceContextVariantCache<TValue>
        where TValue : class
    {
        private object primarySource;
        private object secondarySource;
        private int primaryCount;
        private int secondaryCount;
        private string localizationMarker;
        private readonly TValue[] values = new TValue[2];
        private readonly bool[] populated = new bool[2];
        private bool initialized;

        public TValue GetOrCreate(
            object currentPrimarySource,
            int currentPrimaryCount,
            object currentSecondarySource,
            int currentSecondaryCount,
            string currentLocalizationMarker,
            bool alternateVariant,
            Func<TValue> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            if (!ContextMatches(
                currentPrimarySource,
                currentPrimaryCount,
                currentSecondarySource,
                currentSecondaryCount,
                currentLocalizationMarker))
            {
                ResetContext(
                    currentPrimarySource,
                    currentPrimaryCount,
                    currentSecondarySource,
                    currentSecondaryCount,
                    currentLocalizationMarker);
            }

            int index = alternateVariant ? 1 : 0;
            if (!populated[index])
            {
                values[index] = factory();
                populated[index] = true;
            }

            return values[index];
        }

        public void Invalidate()
        {
            initialized = false;
            values[0] = null;
            values[1] = null;
            populated[0] = false;
            populated[1] = false;
        }

        private bool ContextMatches(
            object currentPrimarySource,
            int currentPrimaryCount,
            object currentSecondarySource,
            int currentSecondaryCount,
            string currentLocalizationMarker)
        {
            return initialized &&
                ReferenceEquals(primarySource, currentPrimarySource) &&
                primaryCount == currentPrimaryCount &&
                ReferenceEquals(secondarySource, currentSecondarySource) &&
                secondaryCount == currentSecondaryCount &&
                string.Equals(
                    localizationMarker,
                    currentLocalizationMarker,
                    StringComparison.Ordinal);
        }

        private void ResetContext(
            object currentPrimarySource,
            int currentPrimaryCount,
            object currentSecondarySource,
            int currentSecondaryCount,
            string currentLocalizationMarker)
        {
            primarySource = currentPrimarySource;
            primaryCount = currentPrimaryCount;
            secondarySource = currentSecondarySource;
            secondaryCount = currentSecondaryCount;
            localizationMarker = currentLocalizationMarker;
            values[0] = null;
            values[1] = null;
            populated[0] = false;
            populated[1] = false;
            initialized = true;
        }
    }
}
