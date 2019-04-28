using System;
using System.Collections.Generic;

namespace EGUI
{
    public sealed class Database
    {
        private static List<Object> objMap = new List<Object>();

        public static int AddObject(Object obj)
        {
            if (ReferenceEquals(obj, null))
                throw new NullReferenceException();
            var count = objMap.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(objMap[i], obj))
                    return i;
                if (ReferenceEquals(objMap[i], null))
                {
                    objMap[i] = obj;
                    return i;
                }
            }

            objMap.Add(obj);
            return count;
        }

        public static int RemoveObject(Object obj)
        {
            if (ReferenceEquals(obj, null))
                throw new NullReferenceException();
            var index = objMap.IndexOf(obj);
            objMap[index] = null;
            return index;
        }

        public static int FetchObject(Object obj)
        {
            if (ReferenceEquals(obj, null))
                throw new NullReferenceException();
            return objMap.IndexOf(obj);
        }

        public static Object FetchObject(int id)
        {
            return id >= 0 && id < objMap.Count ? objMap[id] : null;
        }

        public static bool ExistObject(Object obj)
        {
            if (ReferenceEquals(obj, null))
                throw new NullReferenceException();
            return objMap.IndexOf(obj) >= 0;
        }

        public static bool ExistObject(int id)
        {
            return !ReferenceEquals(objMap[id], null);
        }

        public static void Clear()
        {
            objMap.Clear();
        }
    }
}