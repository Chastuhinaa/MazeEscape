using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Генерує лабіринт із простих квадратних блоків — prefab стіни НЕ потрібен.
/// Просто додай компонент на будь-який GameObject і натисни Play.
/// </summary>
public class MazeGenerator : MonoBehaviour
{
    [Header("Контейнер стін (WallsContainer)")]
    [Tooltip("Перетягни WallsContainer з Hierarchy. Старі стіни будуть сховані.")]
    public Transform wallsParent;

    [Header("Вигляд стін")]
    public Color wallColor  = new(0.33f, 0.56f, 0.73f);   // блакитний
    public Color floorColor = new(0.18f, 0.24f, 0.32f);   // темно-сірий

    [Tooltip("Розмір одного блоку у юнітах. Залиши 1.")]
    public float blockSize = 1f;

    [Header("Для другого привида на Важко (опціонально)")]
    public GameObject ghostPrefab;

    // ── Розміри (клітинок) ────────────────────────────────────────────────
    // Реальна сітка тайлів = cells*2+1
    private static readonly (int w, int h) SizeEasy   = ( 7,  7);  // 15×15
    private static readonly (int w, int h) SizeMedium = (11, 11);  // 23×23
    private static readonly (int w, int h) SizeHard   = (16, 16);  // 33×33

    private int cW, cH, gW, gH;
    private bool[,] wall;
    private Vector2 origin;
    private Sprite squareSprite;
    private Transform generatedRoot; // окремий контейнер для нових блоків

    // ─────────────────────────────────────────────────────────────────────

    public void Generate(GameManager.Difficulty diff)
    {
        // ── ТРЕНУВАННЯ: вмикаємо оригінальний лабіринт, не генеруємо ────
        if (diff == GameManager.Difficulty.Training)
        {
            if (wallsParent     != null) wallsParent.gameObject.SetActive(true);
            if (generatedRoot   != null) generatedRoot.gameObject.SetActive(false);
            // Камера — підлаштовуємо під оригінальний лабіринт
            FitCameraToWallsParent();
            return;
        }

        squareSprite = MakeSquareSprite();

        (int w, int h) size = diff switch
        {
            GameManager.Difficulty.Easy   => SizeEasy,
            GameManager.Difficulty.Medium => SizeMedium,
            GameManager.Difficulty.Hard   => SizeHard,
            _                             => SizeEasy
        };
        cW = size.w; cH = size.h;
        gW = cW * 2 + 1;
        gH = cH * 2 + 1;

        origin = new Vector2(-gW * blockSize / 2f, -gH * blockSize / 2f);

        int seed = diff switch
        {
            GameManager.Difficulty.Easy   => 1337,
            GameManager.Difficulty.Medium => 4242,
            GameManager.Difficulty.Hard   => 9999,
            _                             => 1337
        };

        // Ховаємо старі стіни, показуємо згенеровані
        if (wallsParent != null) wallsParent.gameObject.SetActive(false);

        // Очищуємо попередньо згенерований лабіринт
        if (generatedRoot != null) Destroy(generatedRoot.gameObject);
        var rootGO = new GameObject("_GeneratedMaze");
        generatedRoot = rootGO.transform;

        BuildData(seed);
        DrawMaze();
        PlaceObjects(diff);
        FitCamera();
    }

    // ── Алгоритм DFS (Recursive Backtracking) ────────────────────────────

    void BuildData(int seed)
    {
        wall = new bool[gW, gH];
        for (int x = 0; x < gW; x++)
            for (int y = 0; y < gH; y++)
                wall[x, y] = true;

        var rng = new System.Random(seed);
        var vis = new bool[cW, cH];
        var stk = new Stack<(int, int)>();

        vis[0, 0] = true;
        wall[1, 1] = false;
        stk.Push((0, 0));

        int[] dx = {  1, -1,  0,  0 };
        int[] dy = {  0,  0,  1, -1 };

        while (stk.Count > 0)
        {
            var (cx, cy) = stk.Peek();
            var nbrs = new List<int>();

            for (int d = 0; d < 4; d++)
            {
                int nx = cx + dx[d], ny = cy + dy[d];
                if (nx >= 0 && nx < cW && ny >= 0 && ny < cH && !vis[nx, ny])
                    nbrs.Add(d);
            }

            if (nbrs.Count == 0) { stk.Pop(); continue; }

            int dir = nbrs[rng.Next(nbrs.Count)];
            int ncx = cx + dx[dir], ncy = cy + dy[dir];

            wall[cx * 2 + 1 + dx[dir], cy * 2 + 1 + dy[dir]] = false;
            wall[ncx * 2 + 1, ncy * 2 + 1] = false;

            vis[ncx, ncy] = true;
            stk.Push((ncx, ncy));
        }
    }

    // ── Малюємо лабіринт як прості квадрати ──────────────────────────────

    void DrawMaze()
    {
        // Підлога (один великий квадрат)
        MakeBlock("Floor",
            new Vector2(0f, 0f),
            new Vector2(gW * blockSize, gH * blockSize),
            floorColor,
            isTrigger: false, addCollider: false);

        // Стіни
        for (int x = 0; x < gW; x++)
        {
            for (int y = 0; y < gH; y++)
            {
                if (!wall[x, y]) continue;
                MakeBlock($"W{x}_{y}", GridToWorld(x, y), Vector2.one * blockSize, wallColor,
                    isTrigger: false, addCollider: true);
            }
        }
    }

    GameObject MakeBlock(string name, Vector2 pos, Vector2 size, Color color,
                         bool isTrigger, bool addCollider)
    {
        var go = new GameObject(name);
        go.transform.SetParent(generatedRoot);
        go.transform.position = pos;
        go.transform.localScale = size;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = squareSprite;
        sr.color        = color;
        sr.sortingOrder = name.StartsWith("Floor") ? -1 : 0;

        if (addCollider)
        {
            var col       = go.AddComponent<BoxCollider2D>();
            col.isTrigger = isTrigger;

            // Нульове тертя — гравець не застрягає на кутах
            var pm = new PhysicsMaterial2D("WallNoFriction")
                { friction = 0f, bounciness = 0f };
            col.sharedMaterial = pm;
        }

        return go;
    }

    // ── Розміщення гравця, привида, ключа, виходу ────────────────────────

    void PlaceObjects(GameManager.Difficulty diff)
    {
        var used = new HashSet<Vector2Int>();

        Vector2Int cPlayer = new(0,      0);
        Vector2Int cGhost  = new(cW - 1, cH - 1);
        Vector2Int cKey    = new(cW / 2, cH / 2);
        Vector2Int cExit   = new(cW - 1, 0);

        used.Add(cPlayer); used.Add(cGhost);
        used.Add(cKey);    used.Add(cExit);

        // Гравець
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = CellWorld(cPlayer);
            // Скидаємо швидкість щоб не застряв
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        // Перший привид
        var ghosts = FindObjectsByType<GhostAI>(FindObjectsSortMode.None);
        if (ghosts.Length > 0)
            ghosts[0].transform.position = CellWorld(cGhost);

        // Другий привид (Важко)
        if (diff == GameManager.Difficulty.Hard && ghostPrefab != null)
            Instantiate(ghostPrefab, CellWorld(new Vector2Int(0, cH - 1)), Quaternion.identity);

        // Ключ — переміщуємо існуючий об'єкт
        foreach (var c in FindObjectsByType<Collectible>(FindObjectsSortMode.None))
            if (c.itemType == Collectible.ItemType.Key)
                { c.transform.position = CellWorld(cKey); c.gameObject.SetActive(true); break; }

        // Вихід
        var exitDoor = FindAnyObjectByType<ExitDoor>();
        if (exitDoor != null)
            exitDoor.transform.position = CellWorld(cExit);

        // Кристали — переміщуємо існуючі
        RedistributeCrystals(diff, used);
    }

    void RedistributeCrystals(GameManager.Difficulty diff, HashSet<Vector2Int> used)
    {
        var crystals = new List<Collectible>();
        foreach (var c in FindObjectsByType<Collectible>(FindObjectsSortMode.None))
            if (c.itemType == Collectible.ItemType.Crystal)
                crystals.Add(c);

        int need = diff switch
        {
            GameManager.Difficulty.Easy   => Mathf.Min(6,  crystals.Count),
            GameManager.Difficulty.Medium => Mathf.Min(12, crystals.Count),
            GameManager.Difficulty.Hard   => Mathf.Min(20, crystals.Count),
            _                             => crystals.Count
        };

        var rng = new System.Random(777);

        for (int i = 0; i < crystals.Count; i++)
        {
            if (i < need)
            {
                var cell = RandomCell(used, rng);
                if (cell.x >= 0) { used.Add(cell); crystals[i].transform.position = CellWorld(cell); }
                crystals[i].gameObject.SetActive(true);
            }
            else
            {
                crystals[i].gameObject.SetActive(false); // ховаємо зайві
            }
        }
    }

    // ── Камера ────────────────────────────────────────────────────────────

    void FitCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        float aspect = (float)Screen.width / Screen.height;
        float sz = Mathf.Max(gW * blockSize / 2f / aspect, gH * blockSize / 2f) + 0.5f;
        cam.orthographicSize = sz;
    }

    // ── Допоміжні ─────────────────────────────────────────────────────────

    static Sprite MakeSquareSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    Vector2 GridToWorld(int x, int y)
        => origin + new Vector2((x + 0.5f) * blockSize, (y + 0.5f) * blockSize);

    Vector2 CellWorld(Vector2Int c)
        => GridToWorld(c.x * 2 + 1, c.y * 2 + 1);

    Vector2Int RandomCell(HashSet<Vector2Int> used, System.Random rng)
    {
        for (int i = 0; i < 300; i++)
        {
            var c = new Vector2Int(rng.Next(cW), rng.Next(cH));
            if (!used.Contains(c)) return c;
        }
        return new Vector2Int(-1, -1);
    }

    /// <summary>
    /// Підлаштовує камеру під оригінальний WallsContainer (режим Тренування).
    /// </summary>
    void FitCameraToWallsParent()
    {
        var cam = Camera.main;
        if (cam == null) return;

        if (wallsParent == null)
        {
            // Якщо WallsContainer не призначено — просто центруємо на (0,0)
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.orthographicSize   = 10f;
            return;
        }

        // Знаходимо межі всіх об'єктів у WallsContainer
        var renderers = wallsParent.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);

        cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);

        float aspect  = (float)Screen.width / Screen.height;
        float byWidth = bounds.extents.x / aspect;
        cam.orthographicSize = Mathf.Max(byWidth, bounds.extents.y) + 1f;
    }
}
