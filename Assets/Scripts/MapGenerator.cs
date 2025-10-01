using UnityEngine;
using System.Collections.Generic;

public class WarehouseGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject outerWallPrefab;
    public GameObject[] obstaclePrefabs;
    public GameObject[] packagePrefabs;
    public GameObject deliveryPrefab;
    public GameObject playerPrefab;

    [Header("Map Settings")]
    public int mapWidth = 50;
    public int mapHeight = 50;
    public float tileSize = 2f;

    [Header("Room Settings")]
    public int roomAttempts = 70;
    public int minRoomWidth = 5;
    public int maxRoomWidth = 12;
    public int minRoomHeight = 5;
    public int maxRoomHeight = 12;

    [Header("Objects Settings")]
    public int obstaclesPerRoom = 3;
    public int packagesPerRoom = 3;

    private int[,] map;
    private List<RectInt> rooms = new List<RectInt>();
    private Vector3 deliveryPos;
    private List<Vector3> availableFloorPositions = new List<Vector3>();
    private List<Vector3> obstaclePositions = new List<Vector3>();
    private List<Vector3> packagePositions = new List<Vector3>();

    void Start()
    {
        map = new int[mapWidth, mapHeight];
        InitializeMap();
        GenerateRooms();
        ConnectRoomsWithHallways();
        InstantiateFloor();
        InstantiateWalls();
        PlaceObjects();
        PlaceDeliveryTile();
        SpawnPlayerNextToDelivery();
        PlaceOuterWalls();
    }

    void InitializeMap()
    {
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                map[x, y] = 0;
    }

    void GenerateRooms()
    {
        int minX = 1;
        int maxX = mapWidth - 2;
        int minY = 1;
        int maxY = mapHeight - 2;

        for (int i = 0; i < roomAttempts; i++)
        {
            int w = Random.Range(minRoomWidth, maxRoomWidth + 1);
            int h = Random.Range(minRoomHeight, maxRoomHeight + 1);
            int x = Random.Range(minX, maxX - w + 1);
            int y = Random.Range(minY, maxY - h + 1);

            RectInt newRoom = new RectInt(x, y, w, h);

            bool overlaps = false;
            foreach (var room in rooms)
                if (newRoom.Overlaps(room)) { overlaps = true; break; }

            if (!overlaps)
            {
                rooms.Add(newRoom);
                CarveRoom(newRoom);
            }
        }
    }

    void CarveRoom(RectInt room)
    {
        for (int x = room.xMin; x < room.xMax; x++)
            for (int y = room.yMin; y < room.yMax; y++)
                map[x, y] = 1;
    }

    void ConnectRoomsWithHallways()
    {
        rooms.Sort((a, b) => a.center.x.CompareTo(b.center.x));

        for (int i = 0; i < rooms.Count - 1; i++)
        {
            Vector2Int centerA = new Vector2Int(Mathf.RoundToInt(rooms[i].center.x), Mathf.RoundToInt(rooms[i].center.y));
            Vector2Int centerB = new Vector2Int(Mathf.RoundToInt(rooms[i + 1].center.x), Mathf.RoundToInt(rooms[i + 1].center.y));

            int minX = Mathf.Min(centerA.x, centerB.x);
            int maxX = Mathf.Max(centerA.x, centerB.x);
            for (int x = minX; x <= maxX; x++)
            {
                map[x, centerA.y] = 1;
                if (centerA.y + 1 < mapHeight) map[x, centerA.y + 1] = 1;
            }

            int minY = Mathf.Min(centerA.y, centerB.y);
            int maxY = Mathf.Max(centerA.y, centerB.y);
            for (int y = minY; y <= maxY; y++)
            {
                map[centerB.x, y] = 1;
                if (centerB.x + 1 < mapWidth) map[centerB.x + 1, y] = 1;
            }
        }
    }

    void InstantiateFloor()
    {
        Vector3 floorPos = new Vector3(mapWidth * tileSize / 2f - tileSize / 2f, 0, mapHeight * tileSize / 2f - tileSize / 2f);
        GameObject floor = Instantiate(floorPrefab, floorPos, Quaternion.identity, transform);
        floor.transform.localScale = new Vector3(mapWidth * tileSize / 10f, 1f, mapHeight * tileSize / 10f);

        availableFloorPositions.Clear();
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                if (map[x, y] == 1)
                    availableFloorPositions.Add(new Vector3((x + 0.5f) * tileSize, 0.25f, (y + 0.5f) * tileSize));
    }

    void InstantiateWalls()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (map[x, y] == 0 && HasAdjacentFloor(x, y))
                {
                    Vector3 pos = new Vector3(x * tileSize, 1f, y * tileSize);
                    Quaternion rot = Quaternion.identity;
                    if ((x > 0 && map[x - 1, y] == 1) || (x < mapWidth - 1 && map[x + 1, y] == 1))
                        rot = Quaternion.Euler(0, 90f, 0);
                    Instantiate(wallPrefab, pos, rot, transform);
                }
            }
        }
    }

    bool HasAdjacentFloor(int x, int y)
    {
        if (x > 0 && map[x - 1, y] == 1) return true;
        if (x < mapWidth - 1 && map[x + 1, y] == 1) return true;
        if (y > 0 && map[x, y - 1] == 1) return true;
        if (y < mapHeight - 1 && map[x, y + 1] == 1) return true;
        return false;
    }

    void PlaceObjects()
    {
        obstaclePositions.Clear();
        packagePositions.Clear();

        foreach (var room in rooms)
        {
            List<Vector3> occupied = new List<Vector3>();

            // Place obstacles
            for (int i = 0; i < obstaclesPerRoom; i++)
            {
                Vector3 pos = GetRandomPosInRoom(room);
                GameObject obs = Instantiate(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)], pos, Quaternion.identity, transform);

                // Add PushableObstacle component automatically
                PushableObstacle pushable = obs.GetComponent<PushableObstacle>();
                if (pushable == null)
                    pushable = obs.AddComponent<PushableObstacle>();

                // Scale mass based on size
                Vector3 size = obs.transform.localScale;
                Rigidbody rb = obs.GetComponent<Rigidbody>();
                if (rb == null)
                    rb = obs.AddComponent<Rigidbody>();
                rb.mass = Mathf.Max(1f, size.x * size.y * size.z);
                rb.linearDamping = 1f;
                rb.angularDamping = 1f;

                occupied.Add(pos);
                obstaclePositions.Add(pos);
            }

            // Place packages, avoiding obstacles and delivery tile
            int packagesSpawned = 0;
            int attempts = 0;
            while (packagesSpawned < packagesPerRoom && attempts < 50)
            {
                Vector3 pos = GetRandomPosInRoom(room);
                bool blocked = false;

                foreach (var o in occupied)
                    if (Vector3.Distance(pos, o) < 1f) { blocked = true; break; }

                if (!blocked && deliveryPrefab != null && deliveryPos != Vector3.zero)
                    if (Vector3.Distance(pos, deliveryPos) < 1.5f) blocked = true;

                if (!blocked)
                {
                    Instantiate(packagePrefabs[Random.Range(0, packagePrefabs.Length)], pos, Quaternion.identity, transform);
                    packagePositions.Add(pos);
                    packagesSpawned++;
                }

                attempts++;
            }
        }
    }

    Vector3 GetRandomPosInRoom(RectInt room)
    {
        int randX = Random.Range(room.xMin, room.xMax);
        int randY = Random.Range(room.yMin, room.yMax);

        float x = (randX + 0.5f) * tileSize;
        float z = (randY + 0.5f) * tileSize;

        return new Vector3(x, 0.25f, z);
    }

    void PlaceDeliveryTile()
    {
        List<Vector3> validPositions = new List<Vector3>();

        foreach (var pos in availableFloorPositions)
        {
            bool blocked = false;
            foreach (var obs in obstaclePositions)
                if (Vector3.Distance(pos, obs) < 1f)
                    blocked = true;

            if (!blocked)
                validPositions.Add(pos);
        }

        if (validPositions.Count == 0) return;

        deliveryPos = validPositions[Random.Range(0, validPositions.Count)];
        Instantiate(deliveryPrefab, deliveryPos, Quaternion.identity, transform);
    }

    void SpawnPlayerNextToDelivery()
    {
        if (playerPrefab == null) return;

        Vector3 playerPos = deliveryPos + new Vector3(tileSize * 1.5f, 0.5f, 0);
        Instantiate(playerPrefab, playerPos, Quaternion.identity);
    }

    void PlaceOuterWalls()
    {
        if (outerWallPrefab == null) return;

        Vector3 topPos = new Vector3(mapWidth * tileSize / 2f - tileSize / 2f, 1f, -tileSize / 2f);
        GameObject top = Instantiate(outerWallPrefab, topPos, Quaternion.identity, transform);
        top.transform.localScale = new Vector3(mapWidth * tileSize, 2f, 1f);

        Vector3 bottomPos = new Vector3(mapWidth * tileSize / 2f - tileSize / 2f, 1f, mapHeight * tileSize - tileSize / 2f);
        GameObject bottom = Instantiate(outerWallPrefab, bottomPos, Quaternion.identity, transform);
        bottom.transform.localScale = new Vector3(mapWidth * tileSize, 2f, 1f);

        Vector3 leftPos = new Vector3(-tileSize / 2f, 1f, mapHeight * tileSize / 2f - tileSize / 2f);
        GameObject left = Instantiate(outerWallPrefab, leftPos, Quaternion.Euler(0, 90f, 0), transform);
        left.transform.localScale = new Vector3(mapHeight * tileSize, 2f, 1f);

        Vector3 rightPos = new Vector3(mapWidth * tileSize - tileSize / 2f, 1f, mapHeight * tileSize / 2f - tileSize / 2f);
        GameObject right = Instantiate(outerWallPrefab, rightPos, Quaternion.Euler(0, 90f, 0), transform);
        right.transform.localScale = new Vector3(mapHeight * tileSize, 2f, 1f);
    }
}
