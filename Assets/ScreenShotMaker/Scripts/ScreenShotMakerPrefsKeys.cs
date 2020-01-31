using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScreenShotMaker
{
    /// <summary>
    /// Хранилище ключей префсов для интсрумента ScreenShotMaker
    /// </summary>
    public static class ScreenShotMakerPrefsKeys
    {
        public static string PathScene
        {
            get
            {
                return "ScreenShotMake" + Application.productName + "PathScene";
            }
        }

        public static string NumberScenes
        {
            get
            {
                return "ScreenShotMake" + Application.productName + "NumberScenes";
            }
        }
    }
}