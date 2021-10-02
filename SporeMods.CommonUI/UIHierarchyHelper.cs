using SporeMods.Core;
using SporeMods.CommonUI.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SporeMods.CommonUI
{
	public static class UIHierarchyHelper
	{
		public static List<T> GetDescendantsOfType<T>(this Visual target) where T : Visual
        {
            List<T> found = new List<T>();
            _getDescendantsOfType<T>(target, ref found);
            return found;
        }

        static void _getDescendantsOfType<T>(this Visual target, ref List<T> found) where T : Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(target); i++)
            {
                var child = VisualTreeHelper.GetChild(target, i);
                
                if (child is T find)
                    found.Add(find);
                
                if (child is Visual next)
                    _getDescendantsOfType<T>(next, ref found);
            }
        }

        public static T Find<T>(this FrameworkElement target, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(target); i++)
            {
                var child = VisualTreeHelper.GetChild(target, i);
                
                if ((child is T typeMatch) && (typeMatch.Name.Equals(name)))
                    return typeMatch;
                
                if (child is FrameworkElement next)
                {
                    var el = next.Find<T>(name);

                    if (el != null)
                        return el;
                }
            }
            return null;
        }
	}
}
