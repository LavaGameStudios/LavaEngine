namespace LavaEngineScripts.InputSystem.MouseInputSystem;

using System;
using System.Collections.Generic;
using System.Numerics;
using LavaEngine.Core.InputSystem.MouseInputSystem;

public enum MouseButton
{
    Left,
    Right,
    Middle
}

internal static class MouseButtonConverter
{
    public static LavaEngine.Core.InputSystem.MouseInputSystem.MouseButton ToCore(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => LavaEngine.Core.InputSystem.MouseInputSystem.MouseButton.Left,
            MouseButton.Right => LavaEngine.Core.InputSystem.MouseInputSystem.MouseButton.Right,
            MouseButton.Middle => LavaEngine.Core.InputSystem.MouseInputSystem.MouseButton.Middle,
            _ => LavaEngine.Core.InputSystem.MouseInputSystem.MouseButton.Left
        };
    }
}

public class MouseInput
{
    public static bool IsDown(MouseButton button) => MouseSystem.Instance.IsDown(MouseButtonConverter.ToCore(button));
    public static bool IsUp(MouseButton button) => MouseSystem.Instance.IsUp(MouseButtonConverter.ToCore(button));
    public static bool JustPressed(MouseButton button) => MouseSystem.Instance.JustPressed(MouseButtonConverter.ToCore(button));
    public static bool JustReleased(MouseButton button) => MouseSystem.Instance.JustReleased(MouseButtonConverter.ToCore(button));
    public static bool Held(MouseButton button) => MouseSystem.Instance.Held(MouseButtonConverter.ToCore(button));
    public static bool IsDragging(MouseButton button) => MouseSystem.Instance.IsDragging(MouseButtonConverter.ToCore(button));

    public static Vector2 GetPosition() => MouseSystem.Instance.GetPosition();
    public static Vector2 GetDelta() => MouseSystem.Instance.GetDelta();
    public static Vector2 GetNormalizedPosition() => MouseSystem.Instance.GetNormalizedPosition();

    public static float GetScrollTotal() => MouseSystem.Instance.CurrentState.ScrollTotal;
    public static Vector2 GetScrollDelta() => MouseSystem.Instance.CurrentState.ScrollDelta;

    public static bool IsInsideWindow() => MouseSystem.Instance.CurrentState.IsInsideWindow;
    public static bool IsWindowFocused() => MouseSystem.Instance.CurrentState.IsWindowFocused;

    public static void SetCursorMode(CursorMode mode) => MouseSystem.Instance.SetCursorMode(mode);
    public static void SetCursorPosition(Vector2 position) => MouseSystem.Instance.SetCursorPosition(position);
    public static void ResetCursorPositionToCenter() => MouseSystem.Instance.ResetCursorPositionToCenter();
    public static void SetSensitivity(float x, float y) => MouseSystem.Instance.SetSensitivity(x, y);
    public static void SetRawInput(bool enabled) => MouseSystem.Instance.SetRawInput(enabled);

    public static Vector2 WindowToScreen(Vector2 windowPos) => MouseSystem.Instance.WindowToScreen(windowPos);
    public static Vector2 ScreenToWindow(Vector2 screenPos) => MouseSystem.Instance.ScreenToWindow(screenPos);
    public static Vector2 WindowToNormalized(Vector2 windowPos) => MouseSystem.Instance.WindowToNormalized(windowPos);
    public static Vector2 NormalizedToWindow(Vector2 normalizedPos) => MouseSystem.Instance.NormalizedToWindow(normalizedPos);

    public static IReadOnlyList<MouseEvent> GetEventBuffer() => MouseSystem.Instance.GetEventBuffer();
    public static IReadOnlyList<MouseDragEvent> GetDragBuffer() => MouseSystem.Instance.GetDragBuffer();
}