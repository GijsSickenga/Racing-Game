namespace CinematicCameraSystem {
    [System.Flags]
    public enum RelativeDirection {
        Left = 1,
        Front = 1 << 1,
        Right = 1 << 2,
        Back = 1 << 3
    }
}
