using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace uplink.NET.Sample.Skia.Tizen
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new uplink.NET.Sample.App(), args);
            host.Run();
        }
    }
}
