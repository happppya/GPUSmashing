using UnityEngine;

[RequireComponent(typeof(VoxelDestructionManager))]
public class DestructionStagesController : MonoBehaviour
{
    [SerializeField] private GraphicsCardSO config;
    [SerializeField] private GameObject damagedEffect;
    [SerializeField] private GameObject flamesEffect;
    [SerializeField] private GameObject explosionEffect;

    private VoxelDestructionManager voxelDestructionManager;
    private ParticleSystem currentFlamesEffect;

    private bool isJackpot;
    private bool isSuperJackpot;
    
    private float lightEarnings;
    private float criticalEarnings;
    private float explodeEarnings;

    void DamagedLight()
    {
        GameObject newDamagedEffect = Instantiate(damagedEffect, transform, false);
        Destroy(newDamagedEffect, 5);
        CashManager.Instance.AddCash(lightEarnings);
    }

    void DamagedCritical()
    {
        currentFlamesEffect = Instantiate(flamesEffect, transform).GetComponent<ParticleSystem>();
        GameObject newDamagedEffect = Instantiate(damagedEffect, transform, false);
        Destroy(newDamagedEffect, 5);
        CashManager.Instance.AddCash(criticalEarnings);
    }

    void Exploded()
    {
        if (currentFlamesEffect != null)
        {
            currentFlamesEffect.Stop();
        }
        GameObject newExplosionEffect = Instantiate(explosionEffect, transform, false);
        Destroy(newExplosionEffect, 5);
        CashManager.Instance.AddCash(explodeEarnings);
    }

    void Start()
    {
        voxelDestructionManager = GetComponent<VoxelDestructionManager>();
        voxelDestructionManager.OnDamagedLight += DamagedLight;
        voxelDestructionManager.OnDamagedCritical += DamagedCritical;
        voxelDestructionManager.OnExploded += Exploded;

        float totalEarnings =
            UpgradeManager.Instance.GetStat(UpgradeType.GPUBaseEarningAdd)
            + (
                config.BaseEarnings.GetRandomValue() 
                * (1 + UpgradeManager.Instance.GetStat(UpgradeType.GPUBaseEarningMultiplier))
            );

        float random = UnityEngine.Random.Range(0.0f, 1.0f);

        lightEarnings = 0.1f * totalEarnings;
        criticalEarnings = 0.2f * totalEarnings;
        explodeEarnings = 0.7f * totalEarnings;

        if (random < UpgradeManager.Instance.GetStat(UpgradeType.SuperJackpotChance))
        {
            isSuperJackpot = true;
            explodeEarnings += totalEarnings * (UpgradeManager.Instance.GetStat(UpgradeType.SuperJackpotEarningMultiplier) - 1);
        }
        else if (random < UpgradeManager.Instance.GetStat(UpgradeType.JackpotChance))
        {
            isJackpot = true;
            explodeEarnings += totalEarnings * (UpgradeManager.Instance.GetStat(UpgradeType.JackpotEarningMultiplier) - 1);
        }

        Debug.Log("GOT RANDOM NUMBER " + random.ToString());

        Debug.Log($"GOT EARNINGS {lightEarnings} {criticalEarnings} {explodeEarnings} JACKPOT {isJackpot} {isSuperJackpot}");
    }
}
