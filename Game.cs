using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Egc;

/// <summary>
/// Fereastra aplicației și bucla principală (main game loop).
/// GameWindow apelează automat metodele On... de mai jos, în ordinea descrisă în laborator.
/// </summary>
public class Game : GameWindow
{
    // Obiecte care NU depind de OpenGL pot fi create imediat.
    private readonly Camera _camera = new();
 
    // Obiectele care folosesc OpenGL se creează în OnLoad (abia atunci există contextul OpenGL).
    private Shader _shader = null!;
    private Quad _Quad = null!;

    // Starea aplicației (se modifică în OnUpdateFrame, se folosește în OnRenderFrame).
    private Color4 _clearColor = new(0.0f, 0.0f, 1.0f, 1.0f);   // culoarea de fundal
    private Color4 _tint = new(1.0f, 0.3f, 0.3f, 1.0f);                          // nuanța triunghiului (alb = neschimbat)
    private Vector3 _offset = Vector3.Zero;                       // deplasarea triunghiului
    private bool _wireframe;   // false = Fill, true = Line

    // Contoare pentru exemplul cu bucla principală (afișate în titlul ferestrei).
    private readonly string _baseTitle;
    private double _elapsed;     // timpul total scurs (secunde)
    private double _secondTimer; // cronometru pentru actualizarea titlului o dată pe secundă
    private int _updates;        // câte OnUpdateFrame în ultima secundă
    private int _frames;         // câte OnRenderFrame în ultima secundă

    public Game(int width, int height, string title)
        : base(GameWindowSettings.Default,
               new NativeWindowSettings
               {
                   ClientSize = new Vector2i(width, height),
                   Title = title,
                   APIVersion = new Version(3, 3),          // cerem OpenGL 3.3 ...
                   Profile = ContextProfile.Core,           // ... profil Core (fără modul imediat)
                   Flags = ContextFlags.ForwardCompatible
               })
    {
        _baseTitle = title;
        VSync = VSyncMode.Off;   // swap-ul bufferelor așteaptă reîmprospătarea monitorului
    }

    /// <summary>Apelată O SINGURĂ DATĂ, după crearea ferestrei și a contextului OpenGL.</summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        // Informații despre driverul și placa grafică folosite (vezi Problema 1 din Laboratorul 01).
        Console.WriteLine($"OpenGL  : {GL.GetString(StringName.Version)}");
        Console.WriteLine($"Vendor  : {GL.GetString(StringName.Vendor)}");
        Console.WriteLine($"Renderer: {GL.GetString(StringName.Renderer)}");

        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

        _shader = Shader.FromFiles("Shaders/shader.vert", "Shaders/shader.frag");
        _Quad = new Quad();

    }

    /// <summary>Apelată când se schimbă dimensiunea ferestrei.</summary>
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);   // zona ferestrei în care se desenează
        _camera.Resize(e.Width, e.Height);
    }

    /// <summary>Apelată la fiecare iterație a buclei: LOGICA aplicației (intrări, mișcare, timp).</summary>
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        // args.Time = secundele scurse de la apelul anterior (delta time).
        _elapsed += args.Time;
        _updates++;

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }
        if (KeyboardState.IsKeyPressed(Keys.F))
        {
            _wireframe = !_wireframe;
        }
        if (KeyboardState.IsKeyDown(Keys.D0))
        {
            _tint = Color4.White;
        }
        if (KeyboardState.IsKeyDown(Keys.D1))
        {
            _tint = new Color4(1f, 0f, 0f, 1f);
        }
        if (KeyboardState.IsKeyDown(Keys.D2))
        {
            _tint = new Color4(0f, 1f, 0f, 1f);
        }
        if (KeyboardState.IsKeyDown(Keys.D3))
        {
            _tint = new Color4(0f, 0f, 1f, 1f);
        }
        //changing the size of the triangle with time
       // _triangle.SetPosition(2, 0.0f, 0.3f + 0.3f * MathF.Sin((float)_elapsed));
        //_triangle.SetPosition(0, -0.3f + -0.3f * MathF.Cos((float)_elapsed), 0.0f );
        //_triangle.SetPosition(1, 0.3f + 0.3f * MathF.Sin((float)_elapsed), 0.0f );
       // _triangle.UpdateVertices();

        // --- TODO (P1–P5): codul vostru de logică se adaugă aici ---
        float t = (float)_elapsed;
        float dt = (float)args.Time;
        _clearColor.R = 0.5f + 0.5f * MathF.Sin(t);
        _clearColor.G = 0.5f + 0.5f * MathF.Sin(t + 2.094f);
        _clearColor.B = 0.5f + 0.5f * MathF.Sin(t + 4.189f);

        //_offset = new Vector3(0.7f, 0.2f, 0.0f);
        _offset.X = 0.5f * MathF.Sin((float)_elapsed);
        
        //_offset.Y = 0.5f * MathF.Cos((float)_elapsed);
        //_offset.X -= 0.01f;

        //4 (c) if (KeyboardState.IsKeyDown(Keys.Left)) _offset.X -= 5 * dt;

        // Exemplul bucla principală: o dată pe secundă afișăm în titlu câte
        // actualizări și câte cadre desenate au avut loc.
        _secondTimer += args.Time;
        if (_secondTimer >= 1.0)
        {
            Title = $"{_baseTitle} | update/s: {_updates} | render/s: {_frames}";
            _updates = 0;
            _frames = 0;
            _secondTimer -= 1.0;
        }
    }

    /// <summary>Apelată la fiecare iterație a buclei: DESENAREA unui cadru.</summary>
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        _frames++;

        // 1. Ștergem imaginea veche din back buffer. GL.ClearColor SETEAZĂ starea (culoarea de ștergere),
        //    GL.Clear o FOLOSEȘTE. Aici o setăm la fiecare cadru pentru că vom putea s-o schimbăm (P1).
        GL.ClearColor(_clearColor.R, _clearColor.G, _clearColor.B, _clearColor.A);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        // 2. Activăm programul shader și îi trimitem uniformele.
        _shader.Use();
        _shader.SetMatrix4("uView", _camera.ViewMatrix);
        _shader.SetMatrix4("uProjection", _camera.ProjectionMatrix);
        _shader.SetVector3("uOffset", _offset);
        _shader.SetColor("uTint", _tint);


        //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);   // doar contur

        GL.PolygonMode(MaterialFace.FrontAndBack,
    _wireframe ? PolygonMode.Line : PolygonMode.Fill);


        // 3. Desenăm triunghiul.
        _Quad.Draw();

        // 4. Afișăm cadrul terminat: back buffer <-> front buffer.
        SwapBuffers();
    }

    /// <summary>Apelată O SINGURĂ DATĂ, la închiderea ferestrei: eliberăm resursele GPU.</summary>
    protected override void OnUnload()
{
    _Quad.Dispose();
    _shader.Dispose();

    base.OnUnload();
}
}

