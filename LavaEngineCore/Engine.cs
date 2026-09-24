using LavaEngine.Log;
using LavaEngine.Core;
using System.Diagnostics;
using System.Xml;

namespace LavaEngine
{
    public class Engine
    {
        public int Compile(String source)
        {
            return this.InitEngine(source);
        }

        public int InitEngine(String source)
        {
            // compile
            Core.Core compiler = new Core.Core();
            int result = compiler.CompleProject(source);
            return result;
        }
    }
}