namespace LavaEngine.Core.UICore.UIWindowCore;

using ImGuiNET;
using Silk.NET.Vulkan;

public class UIWindow
{
    public static void Flash()
    {
        ImGui.NewFrame();
    }

    public static void Render() => ImGui.Render(); 

    public static void CreateUIWindow(string name) => ImGui.Begin(name);

    public static void EndWindow() => ImGui.End();
    
}