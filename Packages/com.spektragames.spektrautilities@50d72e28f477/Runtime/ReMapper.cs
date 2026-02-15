using System;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    [System.Serializable]
    public static class ReMapper
    {
        public static float ReMap(
            this float value,
            float valueRangeMin, // Original value range
            float valueRangeMax, // Original value range
            float mappingRangeMin, // Map value range
            float mappingRangeMax // Map value range
        )
        {

            if (valueRangeMin > valueRangeMax)
            {
                Debug.LogError("ReMapper:: valueRangeMin > valueRangeMax");
                return mappingRangeMin;
            }
        
            // Clamps
        
            if ((valueRangeMin < valueRangeMax && mappingRangeMin < mappingRangeMax) ||
                (valueRangeMin < valueRangeMax && mappingRangeMin > mappingRangeMax))
            {
                if (value <= valueRangeMin)
                {
                    return mappingRangeMin;
                }
                else if (value >= valueRangeMax)
                {
                    return mappingRangeMax;
                }
            }
            else if ((valueRangeMin > valueRangeMax && mappingRangeMin < mappingRangeMax) ||
                     (valueRangeMin > valueRangeMax && mappingRangeMin > mappingRangeMax))
            {
                if (value <= valueRangeMax)
                {
                    return mappingRangeMax;
                }
                else if (value >= valueRangeMin)
                {
                    return mappingRangeMin;
                }
            }
        
            // Mapping
        
            if (valueRangeMin <= valueRangeMax && mappingRangeMin <= mappingRangeMax ||
                valueRangeMin <= valueRangeMax && mappingRangeMin > mappingRangeMax)
            {
                float remappedValue = ((value - valueRangeMin) / (valueRangeMax - valueRangeMin)) *
                    (mappingRangeMax - mappingRangeMin) + mappingRangeMin;
                return Clamp(remappedValue, mappingRangeMin, mappingRangeMax);
            }
            else if (valueRangeMin >= valueRangeMax && mappingRangeMin <= mappingRangeMax ||
                     valueRangeMin >= valueRangeMax && mappingRangeMin > mappingRangeMax)
            {
                float normalizedValue = (value - valueRangeMin) / (valueRangeMax - valueRangeMin);
                float remappedValue = ((value - valueRangeMin) / (valueRangeMax - valueRangeMin)) +
                                      normalizedValue * (mappingRangeMax - mappingRangeMin);
                return Clamp(remappedValue, mappingRangeMin, mappingRangeMax);
            }
            else
            {
                UnityEngine.Debug.LogError("Error!");
                return -1;
            }
        }

        static internal float Clamp(float value, float first, float second)
        {
            if (first > second)
            {
                return Math.Clamp(value, second, first);
            }
            else  if (second > first)
            {
                return Math.Clamp(value, first, second);
            }
            else
            {
                return first;
            }
        }
    }
}