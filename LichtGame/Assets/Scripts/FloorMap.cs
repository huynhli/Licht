using UnityEngine;
using System;
using System.Collections.Generic;

public class Room {
    [SerializeField] private int type;
    [SerializeField] private int roomNumber;
    [SerializeField] private int already;
    [SerializeField] public List<Room> neighbours;

    public Room(int roomNumber){
        this.type = 0;
        this.roomNumber = roomNumber;
        this.already = 0;
        this.neighbours = new List<Room>();
    } 

    public void initType(bool nearingEnd, bool haveBossRoom, bool lastRoom){         // room gen (mandatory)
        if (lastRoom) {
            this.type = 8;
        } 
        if (nearingEnd && !haveBossRoom) {
            this.type = UnityEngine.Random.Range(4, 9);
        } else if(nearingEnd) {
            this.type = UnityEngine.Random.Range(4, 8);
        }   
        else {
            this.type = UnityEngine.Random.Range(1, 8);
        }
        // TODO skew this
    }   

    public int getTypeValue(){           // room load
        return this.type;
    }

    public int getRoomNumber(){     // room gen(surroundings), room load (bounds)
        return this.roomNumber;
    }

    public int getAlready() {
        return this.already;        // generate map for map key / on re-run
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
    // so we can call dungeon[room#Tens][room#Ones] for indexing

    // DS for rooms --> list of lists
    // types of rooms:
    // 0: null
    // 1: starter 
    // 2: combat
    // 3: puzzle+combat
    // 4: puzzle
    // 5: puzzle+lore
    // 6: lore
    // 7: cat shop
    // always at least one - 8: boss battle 

    // MAKE THESE LEFT SKEW
    List<List<Room>> dungeon;
    
    void PrintDungeon()
    {
        for (int row = 0; row < dungeon.Count; row++)
        {
            string line = "";
            for (int col = 0; col < dungeon[row].Count; col++)
            {
                Room r = dungeon[row][col];
                // prints type or 0 if uninitialized
                line += r.getTypeValue() + " ";
            }
            Debug.Log(line);
        }
    }
    
    // TODO: dont need rooms for every empty room
    List<List<Room>> makeFloorMap()
    {
        List<List<Room>> floorMap = new List<List<Room>>();

        for (int row = 0; row < 10; row++)
        {
            List<Room> tempListRoom = new List<Room>();
            for (int col = 0; col < 10; col++)
            {
                int roomNum = row * 10 + col;
                tempListRoom.Add(new Room(roomNum));
            }
            floorMap.Add(tempListRoom);
        }

        return floorMap;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dungeon = makeFloorMap();
        PrintDungeon();
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

        int numCurRooms = 0;

        // TODO: queue or stack? stack has chance for VERY linear, queue has chance for much more spread
        // update: queue or balanced avl?
        // or can have balanced counter like avl trees

        // numOfCurRooms = rolling num of rooms generated
        // curRoom = current room looking at
        // curRoomNum = nuber of current room number looking at

        // Binding of Isaac
        // For each cell in the queue, it loops over the 4 cardinal directions and does the following:
            // Determine the neighbour cell by adding +10/-10/+1/-1 to the currency cell.
            // If the neighbour cell is already occupied, give up
            // If the neighbour cell itself has more than one filled neighbour, give up.
            // If we already have enough rooms, give up
            // Random 50% chance, give up
            // Otherwise, mark the neighbour cell as having a room in it, and add it to the queue.

        var roomQueue = new Queue<Room>();
        Room startRoom = dungeon[5][6]; // example starting point
        roomQueue.Enqueue(startRoom);

        bool haveBossRoom = false;

        while (numCurRooms < numRooms && roomQueue.Count > 0) {
            Room curRoom = roomQueue.Dequeue();
            curRoom.initType(numCurRooms > numRooms/3*2, haveBossRoom, numCurRooms == numRooms-1);
            if (curRoom.getTypeValue == 9) haveBossRoom = true;
            numCurRooms++;

            int curRoomNum = curRoom.getRoomNumber();
            int row = curRoomNum / 10;
            int col = curRoomNum % 10;

            // directions: right, up, left, down
            var directions = new List<(int dr, int dc)> {
                (0, 1), (1, 0), (0, -1), (-1, 0)
            };

            int addingRooms = 0;
            foreach (var (dr, dc) in directions) {
                if (addingRooms > 1) break;
                int newRow = row + dr;
                int newCol = col + dc;

                // edge / break case
                if (newRow < 0 || newRow >= 10 || newCol < 0 || newCol >= 10)
                    continue;

                Room neighbour = dungeon[newRow][newCol];

                // skip if already filled, has too many neighbours, or random reject
                if (neighbour.getTypeValue() != 0 ||
                    neighbour.neighbours.Count > 1 ||
                    UnityEngine.Random.value < 0.5f) {
                    continue;
                }

                // otherwise, connect and add to queue
                curRoom.neighbours.Add(neighbour);
                neighbour.neighbours.Add(curRoom);
                roomQueue.Enqueue(neighbour);
                addingRooms++;
            }

        }
        
        
    }
}
