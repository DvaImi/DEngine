// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2022-04-16 13:09:06
// 版 本：1.0
// ========================================================

using System;
using System.Collections.Generic;
using DEngine;

namespace Game
{
    public static partial class GameUtility
    {
        public static class Array
        {
            /// <summary>
            /// 升序排序
            /// </summary>
            /// <typeparam name="T">数据类型</typeparam>
            /// <typeparam name="TKey">数据类型字段的类型</typeparam>
            /// <param name="array">数据类型对象的数组</param>
            /// <param name="handler">委托对象</param>
            public static void OrderBy<T, TKey>(T[] array, DEngineFunc<T, TKey> handler) where TKey : IComparable<TKey>
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
            public static void OrderByDescending<T, TKey>(T[] array, DEngineFunc<T, TKey> handler) where TKey : IComparable<TKey>
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
            public static T Max<T, TKey>(T[] array, DEngineFunc<T, TKey> handler) where TKey : IComparable<TKey>
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
            public static T Min<T, TKey>(T[] array, DEngineFunc<T, TKey> handler) where TKey : IComparable<TKey>
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
                Random rand = new();
                // 生成随机索引值
                var index = rand.Next(array.Length);
                return array[index];
            }


            /// <summary>
            /// 返回随机的
            /// </summary>
            /// <param name="array"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static T Random<T>(List<T> array)
            {
                Random rand = new();
                // 生成随机索引值
                var index = rand.Next(array.Count);
                return array[index];
            }

            /// <summary>
            /// 按指定长度获取数组中得一些元素
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="array"></param>
            /// <param name="count"></param>
            /// <param name="probability">表示选择每个项的概率。如果 probability 等于 1，那么就相当于原来的方法，每个项都会被选择；如果 probability 等于 0，那么就不会选择任何项。如果 probability 大于 0 且小于 1，那么就会按照该概率来选择每个项。</param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            public static T[] Randoms<T>(T[] array, int count, double probability = 1)
            {
                if (count > array.Length)
                {
                    throw new ArgumentException("选择的数量不能超过数组的长度.");
                }

                Random rng = new Random();
                T[] selectedItems = new T[count];
                int[] indices = new int[count];
                int selectedCount = 0;

                while (selectedCount < count)
                {
                    int index = rng.Next(0, array.Length);
                    double rand = rng.NextDouble();

                    if (rand < probability)
                    {
                        int low = 0;
                        int high = selectedCount - 1;

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

                        for (int j = selectedCount - 1; j >= low; j--)
                        {
                            indices[j + 1] = indices[j];
                        }

                        indices[low] = index;
                        selectedItems[selectedCount++] = array[index];
                    }
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
            public static T Find<T>(T[] array, DEngineFunc<T, bool> handler)
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
            public static T[] FindAll<T>(T[] array, DEngineFunc<T, bool> handler)
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
            public static TKey[] Select<T, TKey>(T[] array, DEngineFunc<T, TKey> handler)
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
            public static void PickAndExecute<T>(IEnumerable<T> array, DEngineFunc<T, bool> predicate, Action<T> callback)
            {
                foreach (T item in array)
                {
                    if (predicate(item))
                    {
                        callback(item);
                    }
                }
            }

            /// <summary>
            /// 判定数组是否包含给定值
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="array"></param>
            /// <param name="t"></param>
            /// <returns></returns>
            public static bool Contains<T>(T[] array, T t)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (EqualityComparer<T>.Default.Equals(array[i], t))
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// 交换两个变量
            /// </summary>
            /// <param name="a">变量1</param>
            /// <param name="b">变量2</param>
            /// 
            public static void Swap<T>(ref T a, ref T b)
            {
                (a, b) = (b, a);
            }

            /// <summary>
            ///     递归算法求数组的组合(私有成员)
            /// </summary>
            /// <param name="list">返回的范型</param>
            /// <param name="t">所求数组</param>
            /// <param name="n">辅助变量</param>
            /// <param name="m">辅助变量</param>
            /// <param name="b">辅助数组</param>
            /// <param name="M">辅助变量M</param>
            public static void GetCombination<T>(ref List<T[]> list, T[] t, int n, int m, int[] b, int M)
            {
                for (var i = n; i >= m; i--)
                {
                    b[m - 1] = i - 1;
                    if (m > 1)
                    {
                        GetCombination(ref list, t, i - 1, m - 1, b, M);
                    }
                    else
                    {
                        list ??= new List<T[]>();

                        T[] temp = new T[M];
                        for (var j = 0; j < b.Length; j++)
                        {
                            temp[j] = t[b[j]];
                        }

                        list.Add(temp);
                    }
                }
            }

            /// <summary>
            ///     递归算法求排列(私有成员)
            /// </summary>
            /// <param name="list">返回的列表</param>
            /// <param name="t">所求数组</param>
            /// <param name="startIndex">起始标号</param>
            /// <param name="endIndex">结束标号</param>
            public static void GetPermutation<T>(ref List<T[]> list, T[] t, int startIndex, int endIndex)
            {
                if (startIndex == endIndex)
                {
                    list ??= new List<T[]>();
                    T[] temp = new T[t.Length];
                    t.CopyTo(temp, 0);
                    list.Add(temp);
                }
                else
                {
                    for (var i = startIndex; i <= endIndex; i++)
                    {
                        Swap(ref t[startIndex], ref t[i]);
                        GetPermutation(ref list, t, startIndex + 1, endIndex);
                        Swap(ref t[startIndex], ref t[i]);
                    }
                }
            }

            /// <summary>
            ///     求从起始标号到结束标号的排列，其余元素不变
            /// </summary>
            /// <param name="t">所求数组</param>
            /// <param name="startIndex">起始标号</param>
            /// <param name="endIndex">结束标号</param>
            /// <returns>从起始标号到结束标号排列的范型</returns>
            public static List<T[]> GetPermutation<T>(T[] t, int startIndex, int endIndex)
            {
                if (startIndex < 0 || endIndex > t.Length - 1)
                {
                    return null;
                }

                List<T[]> list = new List<T[]>();
                GetPermutation(ref list, t, startIndex, endIndex);
                return list;
            }

            /// <summary>
            ///     返回数组所有元素的全排列
            /// </summary>
            /// <param name="t">所求数组</param>
            /// <returns>全排列的范型</returns>
            public static List<T[]> GetPermutation<T>(T[] t)
            {
                return GetPermutation(t, 0, t.Length - 1);
            }

            /// <summary>
            ///     求数组中n个元素的排列
            /// </summary>
            /// <param name="t">所求数组</param>
            /// <param name="n">元素个数</param>
            /// <returns>数组中n个元素的排列</returns>
            public static List<T[]> GetPermutation<T>(T[] t, int n)
            {
                if (n > t.Length) return null;
                var list = new List<T[]>();
                var c = GetCombination(t, n);
                for (var i = 0; i < c.Count; i++)
                {
                    var l = new List<T[]>();
                    GetPermutation(ref l, c[i], 0, n - 1);
                    list.AddRange(l);
                }

                return list;
            }

            /// <summary>
            ///     求数组中n个元素的组合
            /// </summary>
            /// <param name="t">所求数组</param>
            /// <param name="n">元素个数</param>
            /// <returns>数组中n个元素的组合的范型</returns>
            public static List<T[]> GetCombination<T>(T[] t, int n)
            {
                if (t.Length < n) return null;
                var temp = new int[n];
                var list = new List<T[]>();
                GetCombination(ref list, t, t.Length, n, temp, n);
                return list;
            }
        }
    }
}