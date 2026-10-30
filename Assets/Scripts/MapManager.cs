using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [Header("Cài đặt Map")]
    public GameObject mapPrefab;       // File gốc của đoạn đường (MapChunk_01)
    public float chunkLength = 20f;    // Chiều dài của 1 đoạn đường (Trục Y)
    public int chunksOnScreen = 3;     // Số đoạn đường tồn tại cùng lúc

    [Header("Mục tiêu theo dõi")]
    public Transform cameraTransform;  // Gắn Camera (hoặc Player) vào đây

    private float spawnY = 0f;
    private Queue<GameObject> activeChunks = new Queue<GameObject>();

    void Start()
    {
        // Vừa vào game, sinh ra sẵn 3 đoạn đường đầu tiên
        for (int i = 0; i < chunksOnScreen; i++)
        {
            SpawnChunk();
        }
    }

    void Update()
    {
        // Khi Camera tiến lên, nếu khoảng cách tới đầu đoạn map mới bị thu hẹp -> Sinh thêm map
        if (cameraTransform.position.y > spawnY - (chunksOnScreen * chunkLength))
        {
            SpawnChunk();
            DeleteOldChunk();
        }
    }

    void SpawnChunk()
    {
        // Tạo đoạn map mới nối tiếp vào tọa độ Y hiện tại
        GameObject newChunk = Instantiate(mapPrefab, new Vector3(0, spawnY, 0), Quaternion.identity);
        newChunk.transform.SetParent(transform); // Gom gọn vào trong MapManager cho đỡ rác Hierarchy
        activeChunks.Enqueue(newChunk);

        spawnY += chunkLength; // Tịnh tiến mốc tọa độ Y cho đoạn tiếp theo
    }

    void DeleteOldChunk()
    {
        // Xóa đoạn map cũ nhất ở phía sau lưng để giải phóng bộ nhớ
        if (activeChunks.Count > chunksOnScreen + 1)
        {
            Destroy(activeChunks.Dequeue());
        }
    }
}