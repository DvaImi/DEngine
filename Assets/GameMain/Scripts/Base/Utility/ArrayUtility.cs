// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2022-04-16 13:09:06
// 版 本：1.0
// ========================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameFramework;

namespace Dvalmi
{
    public static class ArrayUtility
    {
        /// <summary>
        /// 升序排序
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <typeparam name="TKey">数据类型字段的类型</typeparam>
        /// <param name="array">数据类型对象的数组</param>
        /// <param name="handler">委托对象</param>
        public static void OrderBy<T, TKey>(T[] array, GameFrameworkFunc<T, TKey> handler)
            where TKey : IComparable<TKey>
        {
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (handler(array[i]).CompareTo(handler(array[j])) > 0)
                    {
                        (array[i], array[j]) = (array[j], array[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 降序排序
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <typeparam name="TKey">数据类型字段的类型</typeparam>
        /// <param name="array">数据类型对象的数组</param>
        /// <param name="handler">委托对象</param>
        public static void OrderByDescending<T, TKey>(T[] array, GameFrameworkFunc<T, TKey> handler)
            where TKey : IComparable<TKey>
        {
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (handler(array[i]).CompareTo(handler(array[j])) < 0)
                    {
                        (array[i], array[j]) = (array[j], array[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 返回最大的
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <typeparam name="TKey">数据类型字段的类型</typeparam>
        /// <param name="array">数据类型对象的数组</param>
        /// <param name="handler">委托对象</param>
        public static T Max<T, TKey>(T[] array, GameFrameworkFunc<T, TKey> handler)
            where TKey : IComparable<TKey>
        {
            T max = default(T);
            max = array[0];
            for (int i = 1; i < array.Length; i++)
            {
                if (handler(max).CompareTo(handler(array[i])) < 0)
                {
                    max = array[i];
                }
            }

            return max;
        }

        /// <summary>
        /// 返回最小的
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <typeparam name="TKey">数据类型字段的类型</typeparam>
        /// <param name="array">数据类型对象的数组</param>
        /// <param name="handler">委托对象</param>
        public static T Min<T, TKey>(T[] array, GameFrameworkFunc<T, TKey> handler)
            where TKey : IComparable<TKey>
        {
            T min = default(T);
            min = array[0];
            for (int i = 1; i < array.Length; i++)
            {
                if (handler(min).CompareTo(handler(array[i])) > 0)
                {
                    min = array[i];
                }
            }

            return min;
        }


        /// <summary>
        /// 返回随机的
        /// </summary>
        /// <param name="array"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Random<T>(T[] array)
        {
            Random rand = new Random();
            // 生成随机索引值
            var index = rand.Next(array.Length);
            return array[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T[] Randoms<T>(T[] array, int count)
        {
            if (count > array.Length)
            {
                throw new ArgumentException("选择的数量不能超过数组的长度.");
            }

            Random rng = new Random();
            T[] selectedItems = new T[count];
            int[] indices = new int[count];

            for (int i = 0; i < count; i++)
            {
                // 随机生成一个索引
                int index = rng.Next(i, array.Length);

                // 二分查找下一个未使用的索引
                int low = 0;
                int high = i - 1;
                while (low <= high)
                {
                    int mid = (low + high) / 2;
                    if (indices[mid] < index)
                    {
                        low = mid + 1;
                    }
                    else
                    {
                        high = mid - 1;
                    }
                }

                // 将新的索引插入到正确的位置
                for (int j = i - 1; j >= low; j--)
                {
                    indices[j + 1] = indices[j];
                }

                indices[low] = index;
                selectedItems[i] = array[index];
            }

            return selectedItems;
        }


        /// <summary>
        /// 查找满足handler条件的一个
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="handler">查找条件委托</param>
        /// <returns></returns>
        public static T Find<T>(T[] array, GameFrameworkFunc<T, bool> handler)
        {
            T temp = default(T);
            for (int i = 0; i < array.Length; i++)
            {
                if (handler(array[i]))
                {
                    return array[i];
                }
            }

            return temp;
        }

        /// <summary>
        /// 查找所有满足条件的
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static T[] FindAll<T>(T[] array, GameFrameworkFunc<T, bool> handler)
        {
            List<T> list = new List<T>();
            for (int i = 0; i < array.Length; i++)
            {
                if (handler(array[i]))
                {
                    list.Add(array[i]);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// 选取数组中对象的某些成员形成一个独立数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="array"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static TKey[] Select<T, TKey>(T[] array, GameFrameworkFunc<T, TKey> handler)
        {
            TKey[] keys = new TKey[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                keys[i] = handler(array[i]);
            }

            return keys;
        }

        /// <summary>
        /// 选取数组中满足条件的执行callback
        /// </summary>
        /// <param name="array"></param>
        /// <param name="predicate"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        public static void PickAndExecute<T>(IEnumerable<T> array, GameFrameworkFunc<T, bool> predicate, Action<T> callback)
        {
            foreach (T item in array)
            {
                if (predicate(item))
                {
                    callback(item);
                }
            }
        }
    }
}