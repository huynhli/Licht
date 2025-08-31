using UnityEngine;

public class FloorMap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int seed = GameManager.Instance.dungeonSeed;
        int level = GameManager.Instance.level;
        GenerateDungeon(seed, level);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GenerateDungeon(int seed, int level)
    {
        
    }
}
