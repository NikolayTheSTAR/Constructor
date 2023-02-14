using UnityEngine;

namespace TheSTAR.Console
{
    public sealed partial class Console
    {
        private void LoadCommands()
        {
            _commandsTree = new CommandBranch("/", new CommandBranch[]
            {
                new ("help", () =>
                {
                    PrintMessage("Welcome to Console!");
                    PrintMessage("Write one of this parts to start your command:");

                    foreach (var rootCommand in _commandsTree.NextElements) PrintMessage($"/{rootCommand.KeyWord}");
                }),
                new ("get", new CommandBranch[]
                {
                    new ("info", new CommandBranch[]
                    {
                        new ("application", () =>
                        {
                            PrintMessage($"Walls & Ways | Developed by The STAR | v{Application.version}");
                        }),
                        new ("grid", () =>
                        {
                            PrintMessage($"grid size: {_constructor.GetWidth.ToString()}x{_constructor.GetHeight.ToString()} (cells {(_constructor.GetWidth * _constructor.GetHeight).ToString()})");
                            PrintMessage($"pool size: {_constructor.GetPoolWidth.ToString()}x{_constructor.GetPoolHeight.ToString()} (cells {(_constructor.GetPoolWidth * _constructor.GetPoolHeight).ToString()})");
                        }),
                        new ("camera", () =>
                        {
                            PrintMessage($"camera size: {_camera.CurrentSizeScale.ToString()}");
                            PrintMessage($"camera speed: {_camera.CurrentSpeedScale.ToString()}");
                        })
                    })
                }),
                new ("set", new CommandBranch[]
                {
                    new ("grid", new CommandBranch[]
                    {
                        new ("size", new CommandBranch[]
                        {
                            new IntBranch ("x", (x) => _constructor.SetGridSize(x)),
                            new ("min", () => _constructor.SetGridSize(CreativeSpace.CreativeSpace.MinSize)),
                            new ("max", () => _constructor.SetGridSize(CreativeSpace.CreativeSpace.MaxSize))
                        }),
                        new ("width", new CommandBranch[]
                        {
                            new IntBranch ("x", (x) => _constructor.SetGridSize(x, _constructor.GetHeight)),
                            new ("min", () => _constructor.SetGridSize(CreativeSpace.CreativeSpace.MinSize, _constructor.GetHeight)),
                            new ("max", () => _constructor.SetGridSize(CreativeSpace.CreativeSpace.MaxSize, _constructor.GetHeight))
                        }),
                        new ("height", new CommandBranch[]
                        {
                            new IntBranch ("x", (x) => _constructor.SetGridSize(_constructor.GetWidth, x)),
                            new ("min", () => _constructor.SetGridSize(_constructor.GetWidth, CreativeSpace.CreativeSpace.MinSize)),
                            new ("max", () => _constructor.SetGridSize(_constructor.GetWidth, CreativeSpace.CreativeSpace.MaxSize))
                        })
                    }),
                    new ("camera", new CommandBranch[]
                    {
                        new ("size", new CommandBranch[]
                        {
                            new ("default", () => _camera.SetSizeScale(1)),
                            new ("min", () => _camera.SetSizeScale(Constructor.ConstructorCamera.MIN_SIZE_SCALE)),
                            new ("small", () => _camera.SetSizeScale(0.5f)),
                            new ("big", () => _camera.SetSizeScale(2)),
                            new ("max", () => _camera.SetSizeScale(Constructor.ConstructorCamera.MAX_SIZE_SCALE))
                        }),
                        new ("speed", new CommandBranch[]
                        {
                            new ("default", () => _camera.SetSpeedScale(1)),
                            new ("min", () => _camera.SetSpeedScale(Constructor.ConstructorCamera.MIN_SPEED_SCALE)),
                            new ("slow", () => _camera.SetSpeedScale(0.5f)),
                            new ("fast", () => _camera.SetSpeedScale(2)),
                            new ("max", () => _camera.SetSpeedScale(Constructor.ConstructorCamera.MAX_SPEED_SCALE))
                        }),
                        new ("position", new CommandBranch[]
                        {
                            //new ("center", () => _constructor.CameraCentralization()),
                            new EnumBranch<WorldDirections>("direction", (worldDirection) => _constructor.SetCameraPosition(worldDirection))
                        })
                    }),
                    new ("fon", new CommandBranch[]
                    {
                        new ("type", new CommandBranch[]
                        {
                            new EnumBranch<FonType> ("t", (t) => _constructor.SetFonType(t))
                        })
                    })
                }),
                new ("add", new CommandBranch[]
                {
                    new ("grid", new CommandBranch[]
                    {
                        new ("size", new CommandBranch[]
                        {
                            new IntBranch ("x", (x) => _constructor.AddGridSize(x))
                        }),
                        new ("width", new CommandBranch[]
                        {
                            new IntBranch ("x", (x) => _constructor.AddGridSize(x, 0))
                        }),
                        new ("height", new CommandBranch[]
                        {
                            new IntBranch("x", (x) => _constructor.AddGridSize(0, x))
                        })
                    })
                }),
                new ("reset", new CommandBranch[]
                {
                    new ("camera", () =>
                    {
                        _camera.Reset();
                        _constructor.CameraCentralization();
                    })
                }),
                new ("clear", new CommandBranch[]
                {
                    new ("console", ClearMessages),
                    new ("grid", new CommandBranch[]
                    {
                        new ("pool", () =>
                        {
                            _constructor.ClearPool(out var deletedCells);
                            PrintMessage($"Deleted blocks from pool: {deletedCells.ToString()}");
                        })
                    })
                }),
                new ("exit", Application.Quit),
            });
        }
    }
}