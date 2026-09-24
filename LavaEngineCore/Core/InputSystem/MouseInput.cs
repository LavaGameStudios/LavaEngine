namespace LavaEngine.Core.InputSystem.MouseInputSystem;

using System;
using System.Collections.Generic;
using System.Numerics;

public enum CursorMode
{
    Normal,
    Hidden,
    Locked
}

public enum MouseButton
{
    Left,
    Right,
    Middle,
    Button4,
    Button5
}

public struct MouseState
{
    public Vector2 Position;
    public Vector2 Delta;
    public Vector2 ScrollDelta;
    public float ScrollTotal;
    public bool IsInsideWindow;
    public bool IsWindowFocused;
}

public struct MouseEvent
{
    public MouseButton Button;
    public Vector2 Position;
    public float Time;
}

public struct MouseDragEvent
{
    public MouseButton Button;
    public Vector2 StartPosition;
    public Vector2 CurrentPosition;
    public Vector2 Delta;
}

public class MouseSystem
{
    private static MouseSystem _instance;
    public static MouseSystem Instance => _instance ??= new MouseSystem();

    private readonly Dictionary<MouseButton, bool> _currentDown = new();
    private readonly Dictionary<MouseButton, bool> _previousDown = new();
    private readonly Dictionary<MouseButton, float> _pressTime = new();
    private readonly Dictionary<MouseButton, float> _lastClickTime = new();
    private readonly Dictionary<MouseButton, Vector2> _dragStartPos = new();
    private readonly Dictionary<MouseButton, bool> _dragging = new();
    private readonly List<MouseEvent> _eventBuffer = new();
    private readonly List<MouseDragEvent> _dragBuffer = new();

    public MouseState CurrentState;
    public CursorMode CursorMode { get; set; } = CursorMode.Normal;
    public bool CursorVisible { get; set; } = true;
    public bool ClipToWindow { get; set; } = false;
    public Vector2 Sensitivity { get; set; } = Vector2.One;
    public float ScrollSensitivity { get; set; } = 1.0f;
    public float DoubleClickThreshold { get; set; } = 0.3f;
    public float HoldThreshold { get; set; } = 0.5f;
    public bool UseRawInput { get; set; } = false;
    public Vector2 WindowSize { get; set; } = new(1920, 1080);

    public event Action<MouseEvent>? OnMouseDown;
    public event Action<MouseEvent>? OnMouseUp;
    public event Action<MouseEvent>? OnMouseClicked;
    public event Action<MouseEvent>? OnMouseDoubleClicked;
    public event Action<MouseDragEvent>? OnMouseDrag;
    public event Action<Vector2, Vector2>? OnMouseMove;
    public event Action<Vector2>? OnScroll;
    public event Action? OnCursorEnter;
    public event Action? OnCursorLeave;
    public event Action<bool>? OnWindowFocusChanged;

    private MouseSystem()
    {
        foreach (MouseButton btn in Enum.GetValues(typeof(MouseButton)))
        {
            _currentDown[btn] = false;
            _previousDown[btn] = false;
            _pressTime[btn] = -1f;
            _lastClickTime[btn] = -999f;
            _dragStartPos[btn] = Vector2.Zero;
            _dragging[btn] = false;
        }
    }

    public void ProcessMouseDown(MouseButton button, Vector2 position, float time)
    {
        _currentDown[button] = true;
        _pressTime[button] = time;
        _dragStartPos[button] = position;
        _dragging[button] = false;

        var evt = new MouseEvent { Button = button, Position = position, Time = time };
        _eventBuffer.Add(evt);
        OnMouseDown?.Invoke(evt);
    }

    public void ProcessMouseUp(MouseButton button, Vector2 position, float time)
    {
        _currentDown[button] = false;

        var evt = new MouseEvent { Button = button, Position = position, Time = time };
        _eventBuffer.Add(evt);
        OnMouseUp?.Invoke(evt);

        if (_pressTime[button] >= 0f && (time - _pressTime[button]) < DoubleClickThreshold)
        {
            if ((time - _lastClickTime[button]) < DoubleClickThreshold)
            {
                OnMouseDoubleClicked?.Invoke(evt);
                _lastClickTime[button] = -999f;
            }
            else
            {
                _lastClickTime[button] = time;
                OnMouseClicked?.Invoke(evt);
            }
        }

        _pressTime[button] = -1f;
        _dragging[button] = false;
    }

    public void ProcessMouseMove(Vector2 position, Vector2 rawDelta, float time)
    {
        var scaledDelta = rawDelta * Sensitivity;
        var previousPosition = CurrentState.Position;

        CurrentState.Position = position;
        CurrentState.Delta = scaledDelta;

        OnMouseMove?.Invoke(position, scaledDelta);

        foreach (MouseButton btn in Enum.GetValues(typeof(MouseButton)))
        {
            if (_currentDown[btn])
            {
                _dragging[btn] = true;
                var dragEvt = new MouseDragEvent
                {
                    Button = btn,
                    StartPosition = _dragStartPos[btn],
                    CurrentPosition = position,
                    Delta = scaledDelta
                };
                _dragBuffer.Add(dragEvt);
                OnMouseDrag?.Invoke(dragEvt);
            }
        }
    }

    public void ProcessScroll(Vector2 scrollDelta, float time)
    {
        var scaled = scrollDelta * ScrollSensitivity;
        CurrentState.ScrollDelta = scaled;
        CurrentState.ScrollTotal += scaled.Y;
        OnScroll?.Invoke(scaled);
    }

    public void ProcessCursorEnter()
    {
        CurrentState.IsInsideWindow = true;
        OnCursorEnter?.Invoke();
    }

    public void ProcessCursorLeave()
    {
        CurrentState.IsInsideWindow = false;
        OnCursorLeave?.Invoke();
    }

    public void ProcessWindowFocusChanged(bool focused)
    {
        CurrentState.IsWindowFocused = focused;
        OnWindowFocusChanged?.Invoke(focused);

        if (!focused && CursorMode == CursorMode.Normal)
        {
            CursorVisible = false;
        }
        else if (focused && CursorMode == CursorMode.Normal)
        {
            CursorVisible = true;
        }
    }

    public void Update(float deltaTime, float currentTime)
    {
        foreach (MouseButton btn in Enum.GetValues(typeof(MouseButton)))
        {
            _previousDown[btn] = _currentDown[btn];
        }

        _eventBuffer.Clear();
        _dragBuffer.Clear();
    }

    public bool IsDown(MouseButton button) => _currentDown.ContainsKey(button) && _currentDown[button];

    public bool IsUp(MouseButton button) => !IsDown(button);

    public bool JustPressed(MouseButton button) =>
        _currentDown.ContainsKey(button) && _currentDown[button] && !_previousDown[button];

    public bool JustReleased(MouseButton button) =>
        _previousDown.ContainsKey(button) && !_currentDown[button] && _previousDown[button];

    public bool Held(MouseButton button) =>
        IsDown(button) && _pressTime.ContainsKey(button) && _pressTime[button] >= 0f;

    public bool IsDragging(MouseButton button) =>
        _dragging.ContainsKey(button) && _dragging[button];

    public Vector2 GetPosition() => CurrentState.Position;

    public Vector2 GetDelta() => CurrentState.Delta;

    public Vector2 GetNormalizedPosition() =>
        new(CurrentState.Position.X / WindowSize.X, CurrentState.Position.Y / WindowSize.Y);

    public Vector2 WindowToScreen(Vector2 windowPos) => windowPos;

    public Vector2 ScreenToWindow(Vector2 screenPos) => screenPos;

    public Vector2 WindowToNormalized(Vector2 windowPos) =>
        new(windowPos.X / WindowSize.X, windowPos.Y / WindowSize.Y);

    public Vector2 NormalizedToWindow(Vector2 normalizedPos) =>
        new(normalizedPos.X * WindowSize.X, normalizedPos.Y * WindowSize.Y);

    public void SetCursorPosition(Vector2 position)
    {
        CurrentState.Position = position;
    }

    public void ResetCursorPositionToCenter()
    {
        CurrentState.Position = new Vector2(WindowSize.X / 2f, WindowSize.Y / 2f);
    }

    public void SetCursorMode(CursorMode mode)
    {
        CursorMode = mode;
        switch (mode)
        {
            case CursorMode.Normal:
                CursorVisible = true;
                ClipToWindow = false;
                break;
            case CursorMode.Hidden:
                CursorVisible = false;
                ClipToWindow = false;
                break;
            case CursorMode.Locked:
                CursorVisible = false;
                ClipToWindow = true;
                ResetCursorPositionToCenter();
                break;
        }
    }

    public void SetSensitivity(float x, float y)
    {
        Sensitivity = new Vector2(x, y);
    }

    public void SetRawInput(bool enabled)
    {
        UseRawInput = enabled;
    }

    public IReadOnlyList<MouseEvent> GetEventBuffer() => _eventBuffer;
    public IReadOnlyList<MouseDragEvent> GetDragBuffer() => _dragBuffer;
}