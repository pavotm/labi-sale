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

        Camera cam;
        Scene scene;
        Node maze;

        [Preserve]
        public Game(ApplicationOptions opts) : base(opts)
        {

        }

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
            public Node DrawNode { get; set; }

            public DataCell(Node node)
            {
                DrawNode = node;
            }
        }

        class DataWall : Kruskal.IDataWall
        {
            public bool Visited { get; set; }
        }

        private Maze<DataCell, DataWall> createMaze(int x, int y)
        {
            maze = scene.CreateChild(name: "maze");
            var cells = new List<Cell<DataCell, DataWall>>(x * y)
            {
                new Cell<DataCell, DataWall>(/*0, 0, 0, */new List<Wall<DataCell, DataWall>>(), new DataCell(maze.CreateChild()))
            };

            Cell<DataCell, DataWall> fcell = cells.First<Cell<DataCell, DataWall>>();
            fcell.Data.DrawNode.Position = new Vector3(0, 0, 0);

            var walls = new List<Wall<DataCell, DataWall>>(x * y * 4 - x * 2 - y * 2);

            for (int i = 1; i < x; i++)
            {
                var node = maze.CreateChild();
                node.Position = new Vector3(i, 0, 0);
                cells.Add(new Cell<DataCell, DataWall>(/*i, i, 0, */new List<Wall<DataCell, DataWall>>(), new DataCell(node)));
                walls.Add(new Wall<DataCell, DataWall>(cells[i], cells[i - 1], false, new DataWall()));
            }
            for (int i = 1; i < y; i++)
            {
                var node = maze.CreateChild();

                node.Position = new Vector3(0, i, 0);
                cells.Add(new Cell<DataCell, DataWall>(/*i * x, 0, i, */new List<Wall<DataCell, DataWall>>(), new DataCell(node)));
                walls.Add(new Wall<DataCell, DataWall>(cells[i * x], cells[(i - 1) * x], false, new DataWall()));
                for (int j = 1; j < x; j++)
                {
                    var node1 = maze.CreateChild();

                    node1.Position = new Vector3(j, i, 0);
                    cells.Add(new Cell<DataCell, DataWall>(/*i * x + j, j, i, */new List<Wall<DataCell, DataWall>>(), new DataCell(node1)));
                    walls.Add(new Wall<DataCell, DataWall>(cells[i * x + j], cells[(i - 1) * x + j], false, new DataWall()));
                    walls.Add(new Wall<DataCell, DataWall>(cells[i * x + j], cells[i * x + j - 1], false, new DataWall()));
                }
            }

            return new Maze<DataCell, DataWall>(cells, walls);
        }

        protected override void Start()
        {
            scene = CreateScene();

            int x = 10;
            int y = 10;

            var test2 = createMaze(x, y);
            GrowingTree.GrowingTree<DataCell, DataWall>.generate(test2.Cells);
            PrintMaze(test2.Cells, x, y);
            Urho.IO.Log.Write(LogLevel.Debug, cam.Node.Direction.ToString());

            foreach (var cell in test2.Cells)
            {
                

                StaticModel boxModel = cell.Data.DrawNode.CreateComponent<StaticModel>();
                boxModel.Model = ResourceCache.GetModel("Models/Box.mdl");
                boxModel.SetMaterial(ResourceCache.GetMaterial("Materials/BoxMaterial.xml"));
            }

            Input.MouseMoved += (args) =>
            {
                var v = cam.Node.Rotation;
//                cam.Node.Rotate(new Quaternion(new Vector3(args.DX / 10, args.DY / 10, 0), 20), TransformSpace.Local);

                cam.Node.Rotate(new Quaternion(args.DY , args.DX, 0), TransformSpace.Local);
                Urho.IO.Log.Write(LogLevel.Debug, cam.Node.Direction.ToString());
            };

            float speed = 1.5f;


            Input.KeyDown += (args) => {
                if (args.Key == Key.M)
                {
                    maze.Scale *= 1.1f;
                }

                if (args.Key == Key.T)
                {
                    var v = cam.Node.Position;
                    cam.Node.Position += cam.Node.Direction * speed;
                }
                if (args.Key == Key.Z)
                {
                    var v = cam.Node.Position;
                    cam.Node.Position = new Vector3(v.X, v.Y, v.Z + 4);
                }
                if (args.Key == Key.S)
                {
                    var v = cam.Node.Position;
                    cam.Node.Position = new Vector3(v.X, v.Y, v.Z - 4);
                }
                if (args.Key == Key.Q)
                {
                    var v = cam.Node.Position;
                    cam.Node.Position = new Vector3(v.X, v.Y - 4, v.Z);
                }
                if (args.Key == Key.D)
                {
                    var v = cam.Node.Position;
                    cam.Node.Position = new Vector3(v.X, v.Y + 4, v.Z);
                }
                if (args.Key == Key.A)
                {
                    var v = cam.Node.Position;
                    cam.Node.Position = new Vector3(v.X + 4, v.Y, v.Z);
                }
                if (args.Key == Key.E)
                {
                    var v = cam.Node.Position;
                    cam.Node.Position = new Vector3(v.X - 4, v.Y, v.Z);
                }
                if (args.Key == Key.Esc) Exit();
            };
        }

        Scene CreateScene()
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
            light.Range = 1000;
            light.Brightness = 2;

            // Camera
            Node cameraNode = scene.CreateChild(name: "camera");
            cam = cameraNode.CreateComponent<Camera>();

            // Terrain
            var terrainNode = scene.CreateChild();
            terrainNode.Position = new Vector3(x: 0, y: 0, z: -5);      // and change its position
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

            // Viewport
            Renderer.SetViewport(0, new Viewport(Context, scene, cam, null));
            Renderer.GetViewport(0).DrawDebug = true;
            scene.SaveJson(new Urho.IO.File(Context, "save.json", Urho.IO.FileMode.Write), "  ");

            return scene;

        }




        //async void CreateScene()
        //{
        //    Log.LogLevel = LogLevel.Debug;
        //    Urho.IO.Log.Write(LogLevel.Debug, "test");
        //    // UI text 
        //    var helloText = new Text()
        //    {
        //        Value = "Hello World",
        //        HorizontalAlignment = HorizontalAlignment.Center,
        //        VerticalAlignment = VerticalAlignment.Top,
        //    };
        //    helloText.SetColor(new Color(r: 0f, g: 1f, b: 1f));
        //    helloText.SetFont(font: ResourceCache.GetFont("Fonts/Font.ttf"), size: 30);
        //    UI.Root.AddChild(helloText);

        //    // 3D scene with Octree
        //    var scene = new Scene(Context);
        //    scene.CreateComponent<Octree>();
        //    var debug = scene.CreateComponent<DebugRenderer>();

        //    // Sound


        //    // Box
        //    Node boxNode = scene.CreateChild(name: "Box node");
        //    boxNode.Position = new Vector3(x: 0, y: 0, z: 5);
        //    boxNode.SetScale(0f);
        //    boxNode.Rotation = new Quaternion(x: 60, y: 0, z: 30);

        //    StaticModel boxModel = boxNode.CreateComponent<StaticModel>();
        //    boxModel.Model = ResourceCache.GetModel("Models/Box.mdl");
        //    boxModel.SetMaterial(ResourceCache.GetMaterial("Materials/BoxMaterial.xml"));

        //    // Light
        //    Node lightNode = scene.CreateChild(name: "light");
        //    var light = lightNode.CreateComponent<Light>();
        //    light.Range = 1000;
        //    light.Brightness = 2;

        //    // Camera
        //    Node cameraNode = scene.CreateChild(name: "camera");
        //    cam = cameraNode.CreateComponent<Camera>();

        //    // Terrain
        //    var terrainNode = scene.CreateChild();
        //    terrainNode.Position = new Vector3(x: 0, y: 0, z: - 5);      // and change its position
        //    var terrain = terrainNode.CreateComponent<Terrain>();
        //    terrain.Spacing = new Vector3(2, 0.5f, 2);
        //    // the spacing specifies the distance in world units between each heigtmap pixel (XZ)
        //    // or height value (Y)

        //    terrain.Smoothing = true;
        //    // the smoothing specifies if there should be interpolated vertices between heightmap
        //    // pixels

        //    terrain.SetHeightMap(ResourceCache.GetImage("Textures/xamarin.png"));
        //    terrain.Material = ResourceCache.GetMaterial("Materials/BoxMaterial.xml");
        //    terrain.CastShadows = true;
        //    terrain.Occluder = true;

        //    // Viewport
        //    Renderer.SetViewport(0, new Viewport(Context, scene, cam, null));
        //    Renderer.GetViewport(0).DrawDebug = true;
        //    scene.SaveJson(new Urho.IO.File(Context, "save.json", Urho.IO.FileMode.Write), "  ");
        //    // Do actions
        //    await boxNode.RunActionsAsync(new EaseBounceOut(new ScaleTo(duration: 5f, scale: 1)));
        //    await boxNode.RunActionsAsync(new RepeatForever(
        //    new RotateBy(duration: 0.1f, deltaAngleX: 90, deltaAngleY: 90, deltaAngleZ: 90)));



        //}
    }
}
