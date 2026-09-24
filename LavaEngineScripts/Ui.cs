namespace LavaEngineScripts.Ui;

using System.Runtime.InteropServices;
using LavaEngine.Core.UICore.UIWindowCore;

public class Ui
{
    public static void Create(string name) => UIWindow.CreateUIWindow(name);

    public static void Destroy() => UIWindow.EndWindow();
}