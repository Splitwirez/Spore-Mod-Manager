using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SporeMods.CommonUI
{
    public static class ResourceDictionaryHelper
    {
        const int _INFINITE_RECURSION = -1337;
        public static void Flatten(ref ResourceDictionary flatten, int recursionDepth = _INFINITE_RECURSION)
        {
            Dictionary<object, object> resDest = new Dictionary<object, object>();
            FlattenInternal(ref resDest, flatten, recursionDepth);
            flatten.MergedDictionaries.Clear();

            var sourceKeys = resDest.Keys;
            foreach (var key in sourceKeys)
            {
                //if (!flatten.Contains(key))
                    flatten[key] = resDest[key];
            }
        }

        static void FlattenInternal(ref Dictionary<object, object> resDest, ResourceDictionary resSource, int recursionDepth)
        {
            int nextDepth = Math.Max(recursionDepth - 1, 0);
            bool recurse = nextDepth > 0;
            if (recursionDepth == _INFINITE_RECURSION)
            {
                nextDepth = _INFINITE_RECURSION;
                recurse = true;
            }
                
            if (recurse)
            {
                foreach (var merged in resSource.MergedDictionaries)
                {
                    FlattenInternal(ref resDest, merged, nextDepth);
                }
            }

            //var destKeys = resDest.Keys;
            var sourceKeys = resSource.Keys
                //.Cast<object>().Where(x => !destKeys.Contains(x))
            ;
            foreach (var key in sourceKeys)
            {
                //if (!resDest.ContainsKey(key))
                    resDest[key] = resSource[key];
            }
        }
    }
}