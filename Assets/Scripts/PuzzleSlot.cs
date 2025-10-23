using UnityEngine;

public class PuzzleSlot : MonoBehaviour
{
    [SerializeField] private float magnetDistance = 0.5f;

    [Header("VFX для слота")]
    [SerializeField] private ParticleSystem slotVFX1;
    [SerializeField] private ParticleSystem slotVFX2;

    public bool Occupied { get; private set; }
    public float MagnetDistance => magnetDistance;

    public void Occupy(bool isCorrect = false)
    {
        if (Occupied) return;
        Occupied = true;

        RestartVFX(slotVFX1);
        RestartVFX(slotVFX2);
    }

    public void Free()
    {
        Occupied = false;
        StopAllSlotVFX();
    }

    public void ControlChildVFX(string vfxName, bool active)
    {
        foreach (var ps in GetComponentsInChildren<ParticleSystem>())
        {
            if (ps.gameObject.name == vfxName)
            {
                if (active) ps.Play();
                else ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                return;
            }
        }
    }

    private void RestartVFX(ParticleSystem vfx)
    {
        if (vfx == null) return;
        vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        vfx.Play();
    }

    private void StopVFX(ParticleSystem vfx)
    {
        if (vfx == null) return;
        vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void StopAllSlotVFX()
    {
        StopVFX(slotVFX1);
        StopVFX(slotVFX2);
    }
}