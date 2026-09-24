using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using LavaEngine.Core.ScriptManagerCore;
using LavaEngine.Core.InputSystem.MouseInputSystem;
using LavaEngine.Log;
using LavaEngine.Core.UICore.UIInitModule;
using LavaEngine.Core.UICore.UIWindowCore;

namespace LavaEngine.Core.Window.VulkanWindow;

public class VulkanWindow
{
    private IWindow _window;
    private ScriptManager _scriptManager;
    private MouseSystem _mouseSystem;
    private float _elapsedTime;
    private Logger _logger = new Logger();

    private double _fixedTimeStep = 0.02;
    private double _timeAccumulator = 0.0;

    private bool _isCursorInside;

    public int CreateWindow(ScriptManager manager,
        int window_width = 800,
        int window_height = 600,
        string title = "LavaEngine Vulkan Window",
        bool VSync = true)
    {
        _scriptManager = manager;

        var options = WindowOptions.DefaultVulkan;
        options.Size = new Vector2D<int>(window_width, window_height);
        options.Title = title;
        options.VSync = VSync;

        _window = Silk.NET.Windowing.Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Resize += OnResize;

        _window.Run();

        return 0;
    }

    private void OnLoad()
    {
        UiInit.Init();

        _mouseSystem = MouseSystem.Instance;
        _elapsedTime = 0f;
        _isCursorInside = true;

        _mouseSystem.WindowSize = new Vector2(_window.Size.X, _window.Size.Y);

        var input = _window.CreateInput();
        var mouse = input.Mice[0];

        mouse.MouseDown += (m, button) =>
        {
            var mapped = MapButton(button);
            var pos = m.Position;
            _mouseSystem.ProcessMouseDown(mapped, new Vector2((float)pos.X, (float)pos.Y), _elapsedTime);
        };

        mouse.MouseUp += (m, button) =>
        {
            var mapped = MapButton(button);
            var pos = m.Position;
            _mouseSystem.ProcessMouseUp(mapped, new Vector2((float)pos.X, (float)pos.Y), _elapsedTime);
        };

        mouse.MouseMove += (m, position) =>
        {
            var pos = new Vector2((float)position.X, (float)position.Y);
            var delta = pos - _mouseSystem.GetPosition();
            _mouseSystem.ProcessMouseMove(pos, delta, _elapsedTime);

            bool inside = pos.X >= 0 && pos.Y >= 0
                       && pos.X <= _mouseSystem.WindowSize.X
                       && pos.Y <= _mouseSystem.WindowSize.Y;

            if (inside && !_isCursorInside)
            {
                _isCursorInside = true;
                _mouseSystem.ProcessCursorEnter();
            }
            else if (!inside && _isCursorInside)
            {
                _isCursorInside = false;
                _mouseSystem.ProcessCursorLeave();
            }
        };

        mouse.Scroll += (m, wheel) =>
        {
            _mouseSystem.ProcessScroll(new Vector2((float)wheel.X, (float)wheel.Y), _elapsedTime);
        };

        _window.FocusChanged += (focused) => _mouseSystem.ProcessWindowFocusChanged(focused);

        _window.Resize += (size) =>
        {
            _mouseSystem.WindowSize = new Vector2(Math.Max(size.X, 0), Math.Max(size.Y, 0));
        };

        _logger.Trace("Vulkan window loaded, calling all Start functions...");
        _scriptManager.CallAllStartMethods();
        _logger.Info("All Start functions executed.");
    }

    private void OnUpdate(double delta)
    {
        _timeAccumulator += delta;

        while (_timeAccumulator >= _fixedTimeStep)
        {
            _scriptManager.CallAllFixedUpdates();
            _timeAccumulator -= _fixedTimeStep;
        }

        _scriptManager.CallAllUpdates();
        _scriptManager.CallAllLateUpdates();
    }

    private void OnRender(double delta)
    {
        if (_window.Size.X <= 0 || _window.Size.Y <= 0)
            return;

        UIWindow.Flash();
        UIWindow.Render();
    }

    private void OnResize(Vector2D<int> new_size)
    {
        _mouseSystem.WindowSize = new Vector2(Math.Max(new_size.X, 0), Math.Max(new_size.Y, 0));
    }

    public IWindow GetWindow()
    {
        return this._window;
    }

    private LavaEngine.Core.InputSystem.MouseInputSystem.MouseButton MapButton(Silk.NET.Input.MouseButton silkButton)
    {
        return silkButton switch
        {
            Silk.NET.Input.MouseButton.Left => LavaEngine.Core.InputSystem.MouseInputSystem.MouseButton.Left,
            Silk.NET.Input.MouseButton.Right => LavaEngine.Core.InputSystem.MouseInputSystem.MouseButton.Right,
            Silk.NET.Input.MouseButton.Middle => LavaEngine.Core.InputSystem.MouseInputSystem.MouseButton.Middle,
            Silk.NET.Input.MouseButton.Button4 => LavaEngine.Core.InputSystem.MouseInputSystem.MouseButton.Button4,
            Silk.NET.Input.MouseButton.Button5 => LavaEngine.Core.InputSystem.MouseInputSystem.MouseButton.Button5,
            _ => LavaEngine.Core.InputSystem.MouseInputSystem.MouseButton.Left
        };
    }
}