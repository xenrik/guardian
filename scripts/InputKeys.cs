public static class InputKeys {
    /*
    public const string MoveUp = "move_up";
    public const string MoveDown = "move_down";
    public const string MoveLeft = "move_left";
    public const string MoveRight = "move_right";
    */

    public static class Debug {
        public const string ExitGame = "Debug.ExitGame";
        public const string PauseGame = "Debug.PauseGame";
        public const string CycleCameraPos = "Debug.CycleCameraPos";
    }

    public static class Editor {
        public const string SelectModule = "Editor.SelectModule";

        public static class MoveCamera {
            public const string Forward = "Editor.MoveCamera.Forward";
            public const string Back = "Editor.MoveCamera.Back";
            public const string Left = "Editor.MoveCamera.Left";
            public const string Right = "Editor.MoveCamera.Right";
            public const string In = "Editor.MoveCamera.In";
            public const string Out = "Editor.MoveCamera.Out";
            public const string Rotate = "Editor.MoveCamera.Rotate";
        }

        public static class Selector {
            public const string ScrollUp = "Editor.Selector.ScrollUp";
            public const string ScrollDown = "Editor.Selector.ScrollDown";
        }
    }
}