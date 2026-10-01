public sealed class Shotgun : ITowerType
{
    private static readonly float[] pelletAngles =
        { -20f, -10f, 0f, 10f, 20f };

    public void Fire(PooledGun gun)
    {
        gun.FirePattern(1f, pelletAngles);
    }
}