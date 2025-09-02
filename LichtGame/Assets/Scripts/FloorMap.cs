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
    List<List<Room>> dungeon = makeFloorMap();

    private List<List<Room>> makeFloorMap()
    {
        List<List<Room>> floorMap = new List<List<Room>>();
        for (int j = 10; i < 100; i += 10)
        {
            List<Room> tempListRoom = new List<Room>();
            for (int i = 1; i < 10; i++)
            {
                tempListRoom.append(new Room(UnityEngine.Random.Range(0, 7), i + j));
            }
            floorMap.append(tempListRoom);
        }
    }

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

        List<Room> roomStack = new List<Room>();
        while (numRooms > curRooms)
        {

            curRooms++;
            // if last room to generate, make it the boss one
        }
    }
}
