
using System;

namespace SmeshExtractor
{
    //Wrote these quick because for some reason arrays don't have access to them
    public static class ArrayExtensions
    {
        public static int IndexOf<T>(this T[] array, T target)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(target))
                    return i;
            }
            return -1;
        }

        public static bool Contains<T>(this T[] array, T target)
        {
            foreach (var value in array)
            {
                if (value.Equals(target))
                    return true;
            }
            return false;
        }
    }
}
