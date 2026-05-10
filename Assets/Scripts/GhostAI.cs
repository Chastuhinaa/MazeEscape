using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Привид рухається через одну корутину — без Update.
/// Це усуває баг "зупинки" від конфлікту двох потоків.
/// </summary>
public class GhostAI : MonoBehaviour
{
    [Header("Рух")]
    public float speed    = 4f;
    public float cellSize = 1f;

    [Header("Шари стін")]
    public LayerMask wallLayer;

    [Header("Переслідування")]
    public float chaseRange = 10f;

    private Transform player;
    private bool      isActive = false;

    private static readonly Vector2Int[] Dirs =
        { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

    // ── Ініціалізація ─────────────────────────────────────────────────────

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (GameManager.Instance != null)
            speed = GameManager.Instance.ghostSpeed;
    }

    public void Activate()
    {
        if (GameManager.Instance != null)
            speed = GameManager.Instance.ghostSpeed;

        isActive = true;
        StartCoroutine(MoveLoop());
    }

    // ── Головна корутина руху ─────────────────────────────────────────────

    IEnumerator MoveLoop()
    {
        yield return null; // чекаємо один кадр поки сцена ініціалізується

        while (isActive)
        {
            // Пауза якщо гра не активна
            if (GameManager.Instance?.State != GameManager.GameState.Playing)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            // Синхронізуємо швидкість (може змінитись між рівнями)
            if (GameManager.Instance != null)
                speed = GameManager.Instance.ghostSpeed;

            // Вибираємо ціль
            Vector2 target = ChooseTarget();

            // Шукаємо шлях
            List<Vector2> path = BFS(transform.position, target);

            if (path.Count == 0)
            {
                // Шлях не знайдено — чекаємо і пробуємо знову
                yield return new WaitForSeconds(0.3f);
                continue;
            }

            // Рухаємось по кожній точці шляху
            foreach (Vector2 waypoint in path)
            {
                while (Vector2.Distance(transform.position, waypoint) > 0.05f)
                {
                    if (!isActive) yield break;

                    if (GameManager.Instance?.State != GameManager.GameState.Playing)
                    {
                        yield return new WaitForSeconds(0.1f);
                        break;
                    }

                    transform.position = Vector2.MoveTowards(
                        transform.position, waypoint, speed * Time.deltaTime);

                    yield return null; // один кадр
                }
            }

            // Мінімальна затримка перед наступним шляхом
            yield return new WaitForSeconds(0.05f);
        }
    }

    // ── Вибір цілі ────────────────────────────────────────────────────────

    Vector2 ChooseTarget()
    {
        if (player != null)
        {
            float dist  = Vector2.Distance(transform.position, player.position);
            bool  chase = dist <= chaseRange && HasLineOfSight();
            if (chase) return player.position;
        }
        return PickWanderTarget();
    }

    Vector2 PickWanderTarget()
    {
        // Шукаємо прохідну точку в радіусі 12 юнітів
        for (int i = 0; i < 40; i++)
        {
            Vector2 candidate = (Vector2)transform.position + Random.insideUnitCircle * 12f;
            if (!IsWall(ToCell(candidate)))
                return candidate;
        }

        // Fallback: сусідня вільна клітинка
        foreach (var dir in Dirs)
        {
            Vector2Int nb = ToCell(transform.position) + dir;
            if (!IsWall(nb)) return FromCell(nb);
        }

        return transform.position;
    }

    bool HasLineOfSight()
    {
        if (player == null) return false;
        Vector2 dir  = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float   dist = Vector2.Distance(transform.position, player.position);
        return Physics2D.Raycast(transform.position, dir, dist, wallLayer).collider == null;
    }

    // ── BFS ──────────────────────────────────────────────────────────────

    List<Vector2> BFS(Vector2 startW, Vector2 endW)
    {
        Vector2Int start = ToCell(startW);
        Vector2Int end   = ToCell(endW);

        if (IsWall(end)) end = NearestFloor(end);
        if (start == end) return new List<Vector2>();

        var queue   = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        var parent  = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        Vector2Int closest = start;
        float      minDist = float.MaxValue;
        int        limit   = 3000;

        while (queue.Count > 0 && limit-- > 0)
        {
            Vector2Int cur = queue.Dequeue();
            if (cur == end) return Reconstruct(parent, start, end);

            float d = Manhattan(cur, end);
            if (d < minDist) { minDist = d; closest = cur; }

            foreach (var dir in Dirs)
            {
                Vector2Int nb = cur + dir;
                if (!visited.Contains(nb) && !IsWall(nb))
                {
                    visited.Add(nb);
                    parent[nb] = cur;
                    queue.Enqueue(nb);
                }
            }
        }

        return closest != start
            ? Reconstruct(parent, start, closest)
            : new List<Vector2>();
    }

    List<Vector2> Reconstruct(Dictionary<Vector2Int, Vector2Int> parent,
                               Vector2Int start, Vector2Int end)
    {
        var result = new List<Vector2>();
        Vector2Int cur = end;
        while (cur != start)
        {
            result.Add(FromCell(cur));
            if (!parent.TryGetValue(cur, out cur)) break;
        }
        result.Reverse();
        return result;
    }

    // ── Утиліти ───────────────────────────────────────────────────────────

    bool IsWall(Vector2Int cell)
        => Physics2D.OverlapBox(FromCell(cell), Vector2.one * cellSize * 0.8f, 0f, wallLayer) != null;

    Vector2Int NearestFloor(Vector2Int cell)
    {
        for (int r = 1; r <= 4; r++)
            for (int dx2 = -r; dx2 <= r; dx2++)
                for (int dy2 = -r; dy2 <= r; dy2++)
                {
                    var c = cell + new Vector2Int(dx2, dy2);
                    if (!IsWall(c)) return c;
                }
        return cell;
    }

    Vector2Int ToCell(Vector2 w)
        => new(Mathf.RoundToInt(w.x / cellSize), Mathf.RoundToInt(w.y / cellSize));

    Vector2 FromCell(Vector2Int c)
        => new(c.x * cellSize, c.y * cellSize);

    static float Manhattan(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    // ── Пошкодження ───────────────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<PlayerHealth>()?.TakeDamage();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
            col.gameObject.GetComponent<PlayerHealth>()?.TakeDamage();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
#endif
}
