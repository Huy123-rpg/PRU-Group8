using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Cài đặt Map")]
    public GameObject mapChunkPrefab; // Chứa Prefab MapChunk_01
    public float chunkLength = 20f;   // Chiều dài của 1 đoạn map (thay đổi số này cho khớp để map không bị hở)
    public int numberOfChunks = 3;    // Số đoạn map nối đuôi nhau xuất hiện cùng lúc

    private List<GameObject> activeChunks = new List<GameObject>();
    private float spawnPositionX = 0f;
    private Transform playerTransform;

    void Start()
    {
        // Tạm thời lấy Main Camera làm mốc di chuyển (sau này thay bằng nhân vật)
        playerTransform = Camera.main.transform;

        // Tạo ra vài đoạn đường đầu tiên lúc mới vào game
        for (int i = 0; i < numberOfChunks; i++)
        {
            SpawnChunk();
        }
    }

    void Update()
    {
        // Nếu Camera đi qua khỏi đoạn đường cũ, tạo đoạn mới và xóa đoạn cũ ở phía sau
        if (playerTransform.position.x - chunkLength > activeChunks[0].transform.position.x)
        {
            SpawnChunk();
            DeleteOldChunk();
        }
    }

    void SpawnChunk()
    {
        // Sinh ra một đoạn map mới nối tiếp vào tọa độ X
        GameObject newChunk = Instantiate(mapChunkPrefab, new Vector3(spawnPositionX, 0, 0), Quaternion.identity);
        activeChunks.Add(newChunk);
        spawnPositionX += chunkLength;
    }

    void DeleteOldChunk()
    {
        // Xóa đoạn map cũ để tránh nặng máy
        Destroy(activeChunks[0]);
        activeChunks.RemoveAt(0);
    }
}