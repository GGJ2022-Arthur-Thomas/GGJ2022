using System;
using System.Linq;

namespace ExtensionMethods
{
    public static class EnumExtensions
    {
        public static TEnum GetRandomValue<TEnum>(this TEnum enumValue) where TEnum : struct, IConvertible
        {
            return GetRandomValue<TEnum>();
        }

        public static TEnum GetRandomValue<TEnum>() where TEnum : struct, IConvertible
        {
            CheckIsEnumType<TEnum>();
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray().GetRandomElement();
        }

        public static int GetRandomIndex<TEnum>(this TEnum enumValue) where TEnum : struct, IConvertible
        {
            return GetRandomIndex<TEnum>();
        }

        public static int GetRandomIndex<TEnum>() where TEnum : struct, IConvertible
        {
            CheckIsEnumType<TEnum>();
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray().GetRandomIndex();
        }

        public static TEnum GetPreviousValueClamped<TEnum>(this TEnum enumValue) where TEnum : struct, IConvertible
        {
            CheckIsEnumType<TEnum>();
            return GetPreviousValue(enumValue, clamp: true, modulo: false);
        }

        public static TEnum GetPreviousValueModulo<TEnum>(this TEnum enumValue) where TEnum : struct, IConvertible
        {
            CheckIsEnumType<TEnum>();
            return GetPreviousValue(enumValue, clamp: false, modulo: true);
        }

        public static TEnum GetNextValueClamped<TEnum>(this TEnum enumValue) where TEnum : struct, IConvertible
        {
            CheckIsEnumType<TEnum>();
            return GetNextValue(enumValue, clamp: true, modulo: false);
        }

        public static TEnum GetNextValueModulo<TEnum>(this TEnum enumValue) where TEnum : struct, IConvertible
        {
            CheckIsEnumType<TEnum>();
            return GetNextValue(enumValue, clamp: false, modulo: true);
        }

        private static TEnum GetPreviousValue<TEnum>(TEnum enumValue, bool clamp, bool modulo) where TEnum : struct, IConvertible
        {
            CheckIsEnumType<TEnum>();

            var enumIdx = IndexOf(enumValue);
            int previousStateIdx = enumIdx - 1;
            int enumLength = Enum.GetValues(typeof(TEnum)).Length;

            if (previousStateIdx < 0)
            {
                if (clamp)
                {
                    previousStateIdx = 0;
                }
                else if (modulo)
                {
                    previousStateIdx = enumLength - 1;
                }
            }

            return GetValueAtIndex<TEnum>(previousStateIdx);
        }

        private static TEnum GetNextValue<TEnum>(TEnum enumValue, bool clamp, bool modulo) where TEnum : struct, IConvertible
        {
            CheckIsEnumType<TEnum>();

            var enumIdx = IndexOf(enumValue);
            int nextStateIdx = enumIdx + 1;
            int enumLength = Enum.GetValues(typeof(TEnum)).Length;

            if (nextStateIdx >= enumLength)
            {
                if (clamp)
                {
                    nextStateIdx = enumLength - 1;
                }
                else if (modulo)
                {
                    nextStateIdx %= enumLength;
                }
            }

            return GetValueAtIndex<TEnum>(nextStateIdx);
        }

        private static TEnum GetValueAtIndex<TEnum>(int enumIdx) where TEnum : struct, IConvertible
        {
            CheckIsEnumType<TEnum>();
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray()[enumIdx];
        }

        private static int IndexOf<TEnum>(TEnum enumValue) where TEnum : struct, IConvertible
        {
            CheckIsEnumType<TEnum>();
            return Array.FindIndex(Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray(), e => e.Equals(enumValue));
        }

        private static void CheckIsEnumType<TEnum>() where TEnum : struct, IConvertible
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException($"{typeof(TEnum)} must be an enumerated type");
            }
        }
    }
}