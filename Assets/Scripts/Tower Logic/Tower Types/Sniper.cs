public sealed class Sniper : ITowerType
{
    public void Fire(PooledGun gun)
    {
        gun.FirePattern(1.5f, 0f);
    }
}