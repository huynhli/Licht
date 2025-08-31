using UnityEngine;

public class GameManagerSingleton : MonoBehaviour
{
    public static GameManagerSingleton Instance;
    public int level = 1;
    public int dungeonSeed;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
