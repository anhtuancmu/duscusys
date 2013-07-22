using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Discussions
{
    class SkinManager
    {
        public static void ChangeSkin(String skinPath, ResourceDictionary resDict)
        {
            var skinRD = new ResourceDictionary();
            skinRD.Source = new Uri("Metro\\" + skinPath, UriKind.Relative);
            resDict.MergedDictionaries.Clear();
            resDict.MergedDictionaries.Add(skinRD);
        }
    }
}
