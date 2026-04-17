using UnityEditor;
using UnityEngine;

public static class AllEnemiesGenerator
{
    [MenuItem("Tools/MegamanX4/Generate All Enemies")]
    public static void Generate()
    {
        SkyLagoonEnemyGenerator.Generate();
        RecurringEnemyGenerator.Generate();
        JungleEnemyGenerator.Generate();
        Debug.Log("All enemies generated.");
    }
}
