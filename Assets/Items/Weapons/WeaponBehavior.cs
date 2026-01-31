using Unity.VisualScripting;
using UnityEngine;

public abstract class WeaponBehavior : MonoBehaviour
{
    // return false if cannot fire (e.g., in cooldown)
    public abstract bool TryFire();
}
