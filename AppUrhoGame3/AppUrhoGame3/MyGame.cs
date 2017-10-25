using System;
using System.Collections.Generic;
using System.Linq;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Maze;
using Urho.Audio;
using Kruskal;
using Urho.Urho2D;

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

            Urho.IO.Log.Write(LogLevel.Debug, String.Concat(Enumerable.Repeat("_", x * 2 + 1)));
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
                Urho.IO.Log.Write(LogLevel.Debug, line);
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
            Urho.IO.Log.Write(LogLevel.Debug, line);
            System.Diagnostics.Debug.WriteLine(line);
            Urho.IO.Log.Write(LogLevel.Debug, String.Concat(Enumerable.Repeat("‾", x * 2 + 1)));
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

        private Maze<DataCell, DataWall> createMaze(int x, int y)
        {
            var cells = new List<Cell<DataCell, DataWall>>(x * y)
            {
                new Cell<DataCell, DataWall>(/*0, 0, 0, */new List<Wall<DataCell, DataWall>>(), new DataCell())
            };
            var walls = new List<Wall<DataCell, DataWall>>(x * y * 4 - x * 2 - y * 2);

            for (int i = 1; i < x; i++)
            {
                cells.Add(new Cell<DataCell, DataWall>(/*i, i, 0, */new List<Wall<DataCell, DataWall>>(), new DataCell()));
                walls.Add(new Wall<DataCell, DataWall>(cells[i], cells[i - 1], false, new DataWall()));
            }
            for (int i = 1; i < y; i++)
            {
                cells.Add(new Cell<DataCell, DataWall>(/*i * x, 0, i, */new List<Wall<DataCell, DataWall>>(), new DataCell()));
                walls.Add(new Wall<DataCell, DataWall>(cells[i * x], cells[(i - 1) * x], false, new DataWall()));
                for (int j = 1; j < x; j++)
                {
                    cells.Add(new Cell<DataCell, DataWall>(/*i * x + j, j, i, */new List<Wall<DataCell, DataWall>>(), new DataCell()));
                    walls.Add(new Wall<DataCell, DataWall>(cells[i * x + j], cells[(i - 1) * x + j], false, new DataWall()));
                    walls.Add(new Wall<DataCell, DataWall>(cells[i * x + j], cells[i * x + j - 1], false, new DataWall()));
                }
            }

            return new Maze<DataCell, DataWall>(cells, walls);
        }

        protected override void Start()
        {
            CreateScene();

            int x = 10;
            int y = 10;

            var test1 = createMaze(x, y);
            Kruskal.Kruskal<DataCell, DataWall>.generate(test1.Cells, test1.Walls);
            PrintMaze(test1.Cells, x, y);

            var test2 = createMaze(x, y);
            GrowingTree.GrowingTree<DataCell, DataWall>.generate(test2.Cells);
            PrintMaze(test2.Cells, x, y);


            Input.KeyDown += (args) => {
                if (args.Key == Key.Esc) Exit();
            };
        }



        async void CreateScene()
        {
            Log.LogLevel = LogLevel.Debug;
            Urho.IO.Log.Write(LogLevel.Debug, "test");
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
            var debug = scene.CreateComponent<DebugRenderer>();

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

            // Terrain
            var terrainNode = scene.CreateChild();
            terrainNode.Position = new Vector3(x: 0, y: 0, z: - 5);      // and change its position
            var terrain = terrainNode.CreateComponent<Terrain>();
            terrain.Spacing = new Vector3(2, 0.5f, 2);
            // the spacing specifies the distance in world units between each heigtmap pixel (XZ)
            // or height value (Y)

            terrain.Smoothing = true;
            // the smoothing specifies if there should be interpolated vertices between heightmap
            // pixels

            terrain.SetHeightMap(ResourceCache.GetImage("Textures/xamarin.png"));
            terrain.Material = ResourceCache.GetMaterial("Materials/BoxMaterial.xml");
            terrain.CastShadows = true;
            terrain.Occluder = true;
            // the heightmap system can automatically mark objects as invisible that are
            // behind parts of the terrain (like behind a mountain). This improves
            // the rendering performance and is called occluding.

            // moves 10x10 pixel under the camera up by 10%
            IntVector2 v = terrain.WorldToHeightMap(cameraNode.WorldPosition);
            var i = terrain.HeightMap;
            for (int x = -10; x < 10; x++)
                for (int y = -10; y < 10; y++)
                    i.SetPixel(v.X + x, v.Y + y, i.GetPixel(v.X + x, v.Y + y) + new Color(0.1f, 0.1f, 0.1f));
            terrain.ApplyHeightMap();  // has to be called to apply the changes

            // Viewport
            Renderer.SetViewport(0, new Viewport(Context, scene, camera, null));
            Renderer.GetViewport(0).DrawDebug = true;
            scene.SaveJson(new Urho.IO.File(Context, "save.json", Urho.IO.FileMode.Write), "  ");
            // Do actions
            await boxNode.RunActionsAsync(new EaseBounceOut(new ScaleTo(duration: 5f, scale: 1)));
            await boxNode.RunActionsAsync(new RepeatForever(
            new RotateBy(duration: 0.1f, deltaAngleX: 90, deltaAngleY: 90, deltaAngleZ: 90)));

            
            
        }
    }
}
