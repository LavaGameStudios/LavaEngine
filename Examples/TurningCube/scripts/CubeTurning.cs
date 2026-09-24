using LavaEngineScripts.Log;
using LavaEngineScripts.InputSystem.MouseInputSystem;
using LavaEngineScripts.Ui;

public class Cube
{
    private LavaEngineScripts.Log.Logger l = new LavaEngineScripts.Log.Logger();
    private int time=0;

    public int Start()
    {
        return 0;
    }

    public int FixedUpdate()
    {
        return 0;
    }

    public int Update()
    {
        Ui.Create("Hello LavaEngine");
        Ui.Destroy();
        return 0;
    }

    public int LateUpdate()
    {
        return 0;
    }
}