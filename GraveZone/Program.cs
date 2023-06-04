using System;

namespace GraveZone;

internal static class Program
{
    [STAThread]
    public static void Main()
    {
        using var game = new GraveZone();
        game.Run();
    }
}