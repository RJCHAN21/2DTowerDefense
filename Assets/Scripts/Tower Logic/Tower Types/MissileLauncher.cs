using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Tower Defense/Tower Types/Missile Launcher")]
public sealed class MissileLauncher : ScriptableObject, ITowerType
{
#region Unity Inspector Fields

    [Header("Fire Rate & Reload")]
    [Tooltip("Delay in seconds between firing the 1st and 2nd missile in the burst.")]
    [SerializeField, Min(0.01f)] private float burstInterval = 0.25f;

    [Tooltip("Seconds required to reload both missiles after the burst is complete.")]
    [SerializeField, Min(0.1f)] private float reloadDuration = 1f;
    [SerializeField, Min(0.01f)] private float waitFireAfterReload = 3f;

    [Header("Damage")]
    [Tooltip("The amount of damage each missile will deal to the target.")]
    [SerializeField, Min(0f)] private int damage = 25;

    [Header("Sight")]
    [Tooltip("Maximum detection distance of the missile launcher.")]
    [SerializeField, Min(0f)] private float range = 5f;

#endregion

#region Runtime State

    private bool _isFiringOrReloading;

#endregion

#region Firing Behavior

    public void Fire(PooledGun gun)
    {
        if (_isFiringOrReloading || gun == null) return;

        gun.StartCoroutine(FireAndReloadSequence(gun));
    }

    private IEnumerator FireAndReloadSequence(PooledGun gun)
    {
        _isFiringOrReloading = true;

        Transform launcherRoot = gun.transform.root;
        Transform slot1 = launcherRoot.Find("Missile1");
        Transform slot2 = launcherRoot.Find("Missile2");

        SpriteRenderer rend1 = slot1 != null ? slot1.GetComponent<SpriteRenderer>() : null;
        SpriteRenderer rend2 = slot2 != null ? slot2.GetComponent<SpriteRenderer>() : null;

        if (rend1 != null) rend1.enabled = false;
        FireSingleMissile(gun, slot1 != null ? slot1 : gun.transform);

        yield return new WaitForSeconds(burstInterval);

        if (rend2 != null) rend2.enabled = false;
        FireSingleMissile(gun, slot2 != null ? slot2 : gun.transform);

        yield return new WaitForSeconds(reloadDuration);

        if (rend1 != null) rend1.enabled = true;
        if (rend2 != null) rend2.enabled = true;

        yield return new WaitForSeconds(waitFireAfterReload);

        _isFiringOrReloading = false;
    }

    private void FireSingleMissile(PooledGun gun, Transform slotTransform)
    {
        Vector3 originalPos = gun.transform.position;
        gun.transform.position = slotTransform.position;
        gun.FirePattern(0f, damage, 0f);
        gun.transform.position = originalPos;
    }

    public void ResetFiring()
    {
        _isFiringOrReloading = false;
    }

#endregion

#region Sight Behavior

    public void DrawSightVisual(LineRenderer renderer)
    {
        LineRendShape.DrawCircle(renderer, range);
    }

    public bool IsInSight(Transform origin, Transform target)
    {
        return SightDetector.IsInRadius(origin, target, range);
    }

#endregion

#region Upgrade Logic

    public void ApplyUpgrade(TowerUpgradeEffects effects)
    {
        if (effects.OverrideDamage)
        {
            damage = effects.Damage;
        }

        if (effects.OverrideShotInterval)
        {
            reloadDuration = effects.ShotInterval;
        }

        if (effects.OverrideRange)
        {
            range = effects.Range;
        }
    }

#endregion
}