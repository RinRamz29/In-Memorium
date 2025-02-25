using System;
using System.Collections.Generic;

namespace _Memoriam.Script.Enemies.BT
{
    public static class ListExtensions
    {
        private static Random random;
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            if (random == null) random = new Random();

            int count = list.Count;
            while (count > 1)
            {
                --count;
                int index = random.Next(count + 1);
                (list[index], list[count]) = (list[count], list[index]);
            }

            return list;
        }
    }
}