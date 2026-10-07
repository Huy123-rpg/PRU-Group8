using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MonsterLevel
{
    public string levelName;
    public float spawnAtY;
    public GameObject monsterPrefab;
}

public class ObstacleSpawner : MonoBehaviour
{
    [Header("1. Chướng ngại vật thường (Rải liên tục)")]
    public GameObject[] normalObstacles;
    public int minNormal = 1;
    public int maxNormal = 3;

    [Tooltip("Khoảng cách tối thiểu giữa 2 chướng ngại vật để không bị đè lên nhau")]
    public float khoangCachAnToan = 2.5f; // ĐÃ THÊM: Biến chỉnh khoảng cách

    [Header("2. Quái vật theo cột mốc (Chỉ xuất hiện 1 lần)")]
    public MonsterLevel[] levels;
    [Tooltip("Phải nhập ĐÚNG chiều dài của 1 đoạn Map (giống bên MapManager)")]
    public float chunkLength = 20f;

    [Header("3. Giới hạn khu vực đường vàng")]
    public float minX = -3f;
    public float maxX = 3f;
    public float minY = -4f;
    public float maxY = 4f;

    // ĐÃ THÊM: Danh sách lưu trữ các vị trí đã được sinh ra trên đoạn đường này
    private List<Vector2> cacViTriDaCo = new List<Vector2>();

    void Start()
    {
        float chunkY = transform.position.y;
        bool isMonsterLevel = false;

        // Reset danh sách mỗi khi một chunk mới được tạo ra
        cacViTriDaCo.Clear();

        // 1. Quét xem đoạn đường này có rơi trúng mốc tọa độ cố định của quái vật nào không
        if (levels != null && levels.Length > 0)
        {
            foreach (var level in levels)
            {
                if (level.spawnAtY >= chunkY && level.spawnAtY < chunkY + chunkLength)
                {
                    SpawnMonster(level.monsterPrefab);
                    isMonsterLevel = true;
                    break;
                }
            }

            // 2. Chế độ Endless Loop: Sau khi vượt qua các mốc cố định, tiếp tục lặp lại Boss vô tận
            float maxYLevel = 0f;
            foreach (var level in levels)
            {
                if (level.spawnAtY > maxYLevel) maxYLevel = level.spawnAtY;
            }

            if (!isMonsterLevel && chunkY > maxYLevel)
            {
                float cycleInterval = 40f; // Cách mỗi 2 đoạn map (40m) xuất hiện 1 Boss tiếp theo
                float nextBossK = Mathf.Ceil((chunkY - maxYLevel) / cycleInterval);
                if (nextBossK < 1) nextBossK = 1;

                float nextBossY = maxYLevel + nextBossK * cycleInterval;
                if (nextBossY >= chunkY && nextBossY < chunkY + chunkLength)
                {
                    int monsterIndex = ((int)nextBossK - 1) % levels.Length;
                    if (levels[monsterIndex].monsterPrefab != null)
                    {
                        SpawnMonster(levels[monsterIndex].monsterPrefab);
                        isMonsterLevel = true;
                    }
                }
            }
        }

        if (!isMonsterLevel)
        {
            SpawnNormalObstacles();
        }
    }

    void SpawnMonster(GameObject monster)
    {
        if (monster == null) return;

        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(transform.position.x + randomX, transform.position.y, 0);
        Instantiate(monster, spawnPosition, Quaternion.identity, transform);

        // Đưa vị trí Boss vào danh sách để các vật thể khác (nếu có) sẽ né Boss ra
        cacViTriDaCo.Add(spawnPosition);
    }

    void SpawnNormalObstacles()
    {
        if (normalObstacles.Length == 0) return;

        int count = Random.Range(minNormal, maxNormal + 1);
        for (int i = 0; i < count; i++)
        {
            // Gọi hàm tìm vị trí an toàn thay vì random bừa
            Vector3 spawnPos = TimViTriAnToan();

            // Nếu tìm được vị trí hợp lệ (khác với tọa độ rác -9999)
            if (spawnPos != new Vector3(-9999, -9999, 0))
            {
                int randomIndex = Random.Range(0, normalObstacles.Length);
                GameObject obs = normalObstacles[randomIndex];

                Instantiate(obs, spawnPos, Quaternion.identity, transform);

                // Đánh dấu vị trí này đã có vật thể chiếm dụng
                cacViTriDaCo.Add(spawnPos);
            }
        }
    }

    // ĐÃ THÊM: Vòng lặp tìm kiếm tọa độ trống
    private Vector3 TimViTriAnToan()
    {
        int soLanThu = 0;
        int maxSoLanThu = 30; // Thử tối đa 30 lần để chống treo máy

        while (soLanThu < maxSoLanThu)
        {
            // 1. Lấy tọa độ random ban đầu của bạn
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            Vector3 viTriThuNghiem = new Vector3(transform.position.x + randomX, transform.position.y + randomY, 0);

            // 2. Đối chiếu với các vật đã có
            bool anToan = true;
            foreach (Vector2 viTriCu in cacViTriDaCo)
            {
                if (Vector2.Distance(viTriThuNghiem, viTriCu) < khoangCachAnToan)
                {
                    anToan = false; // Bị đụng chạm, đánh dấu không an toàn
                    break;
                }
            }

            // 3. Nếu an toàn thật sự, chốt vị trí này
            if (anToan)
            {
                return viTriThuNghiem;
            }

            soLanThu++;
        }

        // Báo lỗi ngầm định nếu map quá chật không còn chỗ nhét
        return new Vector3(-9999, -9999, 0);
    }
}