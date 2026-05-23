using UnityEngine;

[System.Serializable]
public class BossRewardSpawner
{
    public RewardTarget[] rewardTargets;

    public void ResetRewards()
    {
        foreach (RewardTarget r in rewardTargets)
        {
            if (r == null) continue;

            r.ResetReward();
        }
    }
}