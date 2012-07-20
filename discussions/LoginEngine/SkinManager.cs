using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Discussions
{
    public class SkinManager
    {
        public static void ChangeSkin(String skinPath, ResourceDictionary resDict)
        {
            var skinRD = new ResourceDictionary();
            skinRD.Source = new Uri("pack://application:,,,/LoginEngine;component/Metro/" + skinPath, UriKind.Absolute);
            resDict.MergedDictionaries.Clear();
            resDict.MergedDictionaries.Add(skinRD);
        }
    }
}
