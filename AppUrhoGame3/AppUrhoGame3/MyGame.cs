using System;
using System.Collections.Generic;
using System.Linq;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Maze;

namespace AppUrhoGame3
{
    public class Game : Application
    {
        [Preserve]
        public Game(ApplicationOptions opts) : base(opts) { }

        static Game()
        {
            UnhandledException += (s, e) =>
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
                e.Handled = true;
            };
        }

        void PrintMaze(List<Cell<Data, Object>> cells, int x, int y)
        {
            string line = "";

            System.Diagnostics.Debug.WriteLine(String.Concat(Enumerable.Repeat("_", x * 2 + 1)));

            for (int i = 0; i < y - 1; i++)
            {
                line += '|';
                for (int j = 0; j < x - 1; j++)
                {
                    if (cells[i * x + j].IsOpenWith(cells[(i + 1) * x + j]) == false)
                    {
                        line += '_';
                    }
                    else
                    {
                        line += ' ';
                    }
                    if (cells[i * x + j].IsOpenWith(cells[i * x + j + 1]) == false)
                    {
                        line += '|';
                    }
                    else
                    {
                        line += ' ';
                    }
                }
                if (cells[i * x + x - 1].IsOpenWith(cells[(i + 1) * x + x - 1]) == false)
                {
                    line += '_';
                }
                else
                {
                    line += ' ';
                }
                line += '|';
                System.Diagnostics.Debug.WriteLine(line);
                line = "";
            }
            line += '|';
            for (int i = 0; i < x - 1; i++)
            {
                line += ' ';
                if (cells[(x - 1) * x + i].IsOpenWith(cells[(x - 1) * x + i + 1]) == false)
                {
                    line += '|';
                }
                else
                {
                    line += ' ';
                }
            }
            line += ' ';
            line += '|';
            System.Diagnostics.Debug.WriteLine(line);
            System.Diagnostics.Debug.WriteLine(String.Concat(Enumerable.Repeat("‾", x * 2 + 1)));
        }

        class Data : IData
        {
            public bool Visited { get; set; }
            public Data()
            {
            }
        }

        protected override void Start()
        {
            CreateScene();
            List<Cell<Data, Object>> cells;
            int x = 20;
            int y = 20;
            cells = new List<Cell<Data, Object>>(x * y)
            {
                new Cell<Data, Object>(/*0, 0, 0, */new List<Wall<Data, Object>>(), new Data())
            };
            for (int i = 1; i < x; i++)
            {
                cells.Add(new Cell<Data, Object>(/*i, i, 0, */new List<Wall<Data, Object>>(), new Data()));
                new Wall<Data, Object>(cells[i], cells[i - 1], false, null);
            }
            for (int i = 1; i < y; i++)
            {
                cells.Add(new Cell<Data, Object>(/*i * x, 0, i, */new List<Wall<Data, Object>>(), new Data()));
                new Wall<Data, Object>(cells[i * x], cells[(i - 1) * x], false, null);
                for (int j = 1; j < x; j++)
                {
                    cells.Add(new Cell<Data, Object>(/*i * x + j, j, i, */new List<Wall<Data, Object>>(), new Data()));
                    new Wall<Data, Object>(cells[i * x + j], cells[(i - 1) * x + j], false, null);
                    new Wall<Data, Object>(cells[i * x + j], cells[i * x + j - 1], false, null);
                }
            }
            PrintMaze(cells, x, y);
            MazeGenerator<Data, Object>.generate(cells);
            PrintMaze(cells, x, y);
            Input.KeyDown += (args) => {
                if (args.Key == Key.Esc) Exit();
            };
        }



        async void CreateScene()
        {
            // UI text 
            var helloText = new Text()
            {
                Value = "Hello World",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
            };
            helloText.SetColor(new Color(r: 0f, g: 1f, b: 1f));
            helloText.SetFont(font: ResourceCache.GetFont("Fonts/Font.ttf"), size: 30);
            UI.Root.AddChild(helloText);

            // 3D scene with Octree
            var scene = new Scene(Context);
            scene.CreateComponent<Octree>();

            // Box
            Node boxNode = scene.CreateChild(name: "Box node");
            boxNode.Position = new Vector3(x: 0, y: 0, z: 5);
            boxNode.SetScale(0f);
            boxNode.Rotation = new Quaternion(x: 60, y: 0, z: 30);

            StaticModel boxModel = boxNode.CreateComponent<StaticModel>();
            boxModel.Model = ResourceCache.GetModel("Models/Box.mdl");
            boxModel.SetMaterial(ResourceCache.GetMaterial("Materials/BoxMaterial.xml"));

            // Light
            Node lightNode = scene.CreateChild(name: "light");
            var light = lightNode.CreateComponent<Light>();
            light.Range = 10;
            light.Brightness = 10.5f;

            // Camera
            Node cameraNode = scene.CreateChild(name: "camera");
            Camera camera = cameraNode.CreateComponent<Camera>();

            // Viewport
            Renderer.SetViewport(0, new Viewport(Context, scene, camera, null));

            // Do actions
            await boxNode.RunActionsAsync(new EaseBounceOut(new ScaleTo(duration: 5f, scale: 1)));
            await boxNode.RunActionsAsync(new RepeatForever(
            new RotateBy(duration: 0.1f, deltaAngleX: 90, deltaAngleY: 90, deltaAngleZ: 90)));
        }
    }
}
