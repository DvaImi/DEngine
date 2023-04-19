// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-19 19:34:39
// 版 本：1.0
// ========================================================

#if !UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace Dvalmi
{
    [Preserve]
    public class SkipUnityLogo
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void BeforeSplashScreen()
        {
#if UNITY_WEBGL
            Application.focusChanged += Application_focusChanged;
#else
            System.Threading.Tasks.Task.Run(AsyncSkip);
#endif
        }

#if UNITY_WEBGL
        private static void Application_focusChanged(bool obj)
        {
            Application.focusChanged -= Application_focusChanged;
            SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
        }
#else
        private static void AsyncSkip()
        {
            SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
        }
#endif
    }

}
#endif
