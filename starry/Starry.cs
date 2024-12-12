﻿using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace starry;

public static class Starry {
    /// <summary>
    /// starry settings
    /// </summary>
    public static StarrySettings settings { get; set; }
    /// <summary>
    /// the engine version (semantic versioning)
    /// </summary>
    public static vec3i starryVersion => (2, 0, 5);

    /// <summary>
    /// sets up the engine
    /// </summary>
    public static async Task create(StarrySettings settings)
    {
        // funni
        Starry.settings = settings;
        Console.WriteLine("Use --verbose if the game is broken.");

        // opengl thread lmao
        Thread thread = new(Graphics.glLoop) {
            IsBackground = true,
        };
        thread.Start();

        string title = $"{settings.gameName}";
        if (settings.showVersion) title += " " + settings.gameVersion.asVersion();
        
        // the size doesn't matter once you make it fullscreen
        Window.create(title, settings.renderSize);
        Window.setFullscreen(settings.fullscreen);
        
        // fccking kmodules
        await Task.Run(Tilemap.create);
        await DebugMode.create();

        settings.startup();
        
        while (!await Window.isClosing()) {
            Graphics.clear(color.black);
            
            // stuff
            // this being async has a small but non-zero chance of collapsing the space time continuum
            Input.update();
            await Entities.update();
            await Task.Run(Tilemap.update);
            await DebugMode.update();

            Graphics.endDrawing();
        }

        Window.invokeTheInfamousCloseEventBecauseCeeHashtagIsStupid();

        // fccking kmodules
        Assets.cleanup();
        Window.cleanup();
    }

    /// <summary>
    /// loads the assets and then puts it in a handsome dictionary of stuff so its blazingly fast or smth idfk this is just Assets.load<T> lmao
    /// </summary>
    public static async Task<T> load<T>(string path) where T: IAsset, new() =>
        await Assets.load<T>(path);

    /// <summary>
    /// Console.WriteLine but cooler (it prints more types and has caller information)
    /// </summary>
    public static void log(params object[] x)
    {
        if (!settings.verbose) return;

        StringBuilder str = new();

        // show the class and member lmao
        StackTrace stackTrace = new();
        StackFrame? frame = stackTrace.GetFrame(1);
        var method = frame?.GetMethod();
        var className = method?.DeclaringType?.Name;
        var methodName = method?.Name;
        str.Append($"[{className ?? string.Empty}.{methodName ?? string.Empty}] ");

        foreach (var item in x) {
            // we optimize common types so the game doesn't explode
            switch (item) {
                case string:
                case sbyte:
                case byte:
                case short:
                case ushort:
                case int:
                case uint:
                case long:
                case ulong:
                case float:
                case double:
                case decimal:
                case bool:
                    str.Append(item.ToString());
                    break;
                
                case vec2 wec2: str.Append($"vec2({wec2.x}, {wec2.y})"); break;
                case vec2i wec2i: str.Append($"vec2i({wec2i.x}, {wec2i.y})"); break;
                case vec3 wec3: str.Append($"vec3({wec3.x}, {wec3.y}, {wec3.z})"); break;
                case vec3i wec3i: str.Append($"vec3i({wec3i.x}, {wec3i.y}, {wec3i.z})"); break;
                case color coughlour: str.Append($"rgba({coughlour.r}, {coughlour.g}, {coughlour.b}, {coughlour.a})"); break;
                case null: str.Append("null"); break;
                default: str.Append(JsonConvert.SerializeObject(item)); break;
            }

            if (x.Length > 1) str.Append(", ");
        }
        Console.WriteLine(str);
    }

    /// <summary>
    /// if true, the game is running in debug mode
    /// </summary>
    public static bool isDebug()
    {
        #if DEBUG
        return true;
        #else
        return false;
        #endif
    }
}