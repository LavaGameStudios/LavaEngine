namespace LavaEngine.Core.UICore.UIInitModule;

using ImGuiNET;

public class UiInit
{
    public static int Init()
    {
        ImGui.CreateContext();
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

        return 0;
    }
}