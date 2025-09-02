using UnityEngine;
using System;
using System.Collections.Generic;

public struct Room {
    [SerializeField] private int type;
    [SerializeField] private int roomNumber;
    [SerializeField] private int already;

    public Room(int type, int roomNumber){
        this.type = type;
        this.roomNumber = roomNumber;
        this.already = 0;
    } 
}

public class FloorMap : MonoBehaviour
{
    // dont explicitly use ints for number rooms. 
    // room is found by taking each individual digit of currentRoom# inputted,
    // so room 11 is row1, col1 for dungeon[][], etc.
    // then, each room stores a Room object, which have 
    // .type (int) for attacking, .roomNumber (int) to double check + convert to pixels for spawning,
    // and .already (int) for if already visited room, and what direction you left in --> for gen after-image
    // so we can call dungeon[room#Tens][room#Ones].var for values
    

    // DS for rooms --> list of lists
    List<List<int>> dungeon = new List<List<int>>
    { 
        // types of rooms:
        // 0: starter 
        // 1: combat
        // 2: puzzle+combat
        // 3: puzzle
        // 4: puzzle+lore
        // 5: lore
        // 6: cat shop
        // always at least one - 7: boss battle 

        // MAKE THESE LEFT SKEW
        new List<int> {new Room(UnityEngine.Random.Range(0f, 7), 11), 12, 13, 14, 15, 16, 17, 18, 19},
        new List<int> {21, 22, 23, 24, 25, 26, 27, 28, 29},
        new List<int> {31, 32, 33, 34, 35, 36, 37, 38, 39},
        new List<int> {41, 42, 43, 44, 45, 46, 47, 48, 49},
        new List<int> {51, 52, 53, 54, 55, 56, 57, 58, 59},
        new List<int> {61, 62, 63, 64, 65, 66, 67, 68, 69},
        new List<int> {71, 72, 73, 74, 75, 76, 77, 78, 79},
        new List<int> {81, 82, 83, 84, 85, 86, 87, 88, 89},
        new List<int> {91, 92, 93, 94, 95, 96, 97, 98, 99}
    };


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int seed = GameManagerSingleton.Instance.dungeonSeed;
        int level = GameManagerSingleton.Instance.level;
        GenerateDungeon(seed, level);
    }

    void GenerateDungeon(int seed, int level)
    {
        // we want a snake like pattern, so length > variety. 
        // this leads to streamlined traversals, which gets us:
        // more flow, less memorizing, and you FEEL faster.

        // THIS IS NOT AN EXPLORATION GAME! 
        // it has a story focus, so add lore rooms, 
        // but more importantly, we want players to want to play, not want to quit
        // --> streamlined maps, fast/powerful feeling gameplay, but varied experiences.

        // what does this mean? ...
        // it means:
        // - keep them thinking, but dont make them think about things they dont need to think abt
        
        // timer mechanic:
        // we reset if we dont reach the boss room fast enough.
        // keep a counter of how many resets, and change final boss dialogue
        // - /difficulty(very slightly) based on that.
        //      - then, speedrunners/better players have negative feedback loop for being fast ? [do we even want that?]
        // - or positive feedback loop with perma cosmetic :D or shop creds

        // IMPORTANT: THIS IS NOT A ROGUELIKE
        // shop items should be temporary one-time usage or perma cosmetic
        // - if ppl replay the game, i think dialogue should be different the second time, it would be a cool addition

        int numRooms = (int)Math.Floor(UnityEngine.Random.Range(0f, 3f) + 6f + (level * 2.4f));

        int curRooms = 0;
        List<int> roomsByNum = new List<int>();
        while (numRooms > curRooms){
            
            curRooms++;
            // if last room to generate, make it the boss one
        }
    }
}
