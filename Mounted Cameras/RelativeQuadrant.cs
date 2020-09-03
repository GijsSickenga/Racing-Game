namespace CinematicCameraSystem {
    public enum RelativeQuadrant {
        FrontLeft = RelativeDirection.Front | RelativeDirection.Left,
        FrontRight = RelativeDirection.Front | RelativeDirection.Right,
        BackLeft = RelativeDirection.Back | RelativeDirection.Left,
        BackRight = RelativeDirection.Back | RelativeDirection.Right
    }
}
