using System;
using System.Collections.Generic;
using System.Linq;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Maze;
using Urho.Audio;
using Kruskal;

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

        void PrintMaze(List<Cell<DataCell, DataWall>> cells, int x, int y)
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

        class DataCell : GrowingTree.IDataCell, Kruskal.IDataCell
        {
            public bool Visited { get; set; }
            public UnionFindNode Node { get; set; }
        }

        class DataWall : Kruskal.IDataWall
        {
            public bool Visited { get; set; }
        }

        private List<Cell<DataCell, DataWall>> createMaze(int x, int y)
        {
            List<Cell<DataCell, DataWall>> cells;
            cells = new List<Cell<DataCell, DataWall>>(x * y)
            {
                new Cell<DataCell, DataWall>(/*0, 0, 0, */new List<Wall<DataCell, DataWall>>(), new DataCell())
            };
            for (int i = 1; i < x; i++)
            {
                cells.Add(new Cell<DataCell, DataWall>(/*i, i, 0, */new List<Wall<DataCell, DataWall>>(), new DataCell()));
                new Wall<DataCell, DataWall>(cells[i], cells[i - 1], false, new DataWall());
            }
            for (int i = 1; i < y; i++)
            {
                cells.Add(new Cell<DataCell, DataWall>(/*i * x, 0, i, */new List<Wall<DataCell, DataWall>>(), new DataCell()));
                new Wall<DataCell, DataWall>(cells[i * x], cells[(i - 1) * x], false, new DataWall());
                for (int j = 1; j < x; j++)
                {
                    cells.Add(new Cell<DataCell, DataWall>(/*i * x + j, j, i, */new List<Wall<DataCell, DataWall>>(), new DataCell()));
                    new Wall<DataCell, DataWall>(cells[i * x + j], cells[(i - 1) * x + j], false, new DataWall());
                    new Wall<DataCell, DataWall>(cells[i * x + j], cells[i * x + j - 1], false, new DataWall());
                }
            }

            return cells;
        }

        protected override void Start()
        {
            int x = 200;
            int y = 200;

            var test1 = createMaze(x, y);
            Kruskal.Kruskal<DataCell, DataWall>.generate(test1);
            PrintMaze(test1, x, y);

            var test2 = createMaze(x, y);
            GrowingTree.GrowingTree<DataCell, DataWall>.generate(test2);
            PrintMaze(test2, x, y);

            CreateScene();

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

            // Sound
            Node soundNode = scene.CreateChild(name: "Sound");
            var sound = soundNode.CreateComponent<SoundSource>();
            sound.Play(ResourceCache.GetSound("Sounds/Glorious_morning.wav"));


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
