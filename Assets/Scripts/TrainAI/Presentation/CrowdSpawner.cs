using UnityEngine;
using UnityEngine.AI;

namespace TrainAI.Presentation
{
    public class CrowdSpawner : MonoBehaviour
    {
        public GameObject npcPrefab;
        public int spawnCount = 20;
        public float spawnRadius = 50f;

        void Start()
        {
            if (npcPrefab == null) return;

            for (int i = 0; i < spawnCount; i++)
            {
                var go = Instantiate(npcPrefab, transform);
                go.name = $"CrowdNPC_{i}";
                
                // Random position on NavMesh
                Vector2 rand = Random.insideUnitCircle * spawnRadius;
                Vector3 pos = transform.position + new Vector3(rand.x, 0, rand.y);
                if (NavMesh.SamplePosition(pos, out var hit, spawnRadius, NavMesh.AllAreas))
                {
                    go.transform.position = hit.position;
                }
                
                // Ensure NavMeshAgent
                var agent = go.GetComponent<NavMeshAgent>();
                if (agent == null) agent = go.AddComponent<NavMeshAgent>();
                agent.speed = Random.Range(1.2f, 2.0f); // Varying walking speeds

                // Remove logic reserved for named NPCs so they don't break/error
                var view = go.GetComponent<NpcView>();
                if (view != null) Destroy(view);
                
                // Attach wandering logic
                go.AddComponent<NpcWander>();
            }
        }
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class NpcWander : MonoBehaviour
    {
        public float wanderRadius = 25f;
        public float minWait = 1f;
        public float maxWait = 5f;

        NavMeshAgent _agent;
        float _timer;

        void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _timer = Random.Range(minWait, maxWait);
        }

        void Update()
        {
            if (_agent == null || !_agent.isOnNavMesh) return;

            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0)
                {
                    _timer = Random.Range(minWait, maxWait);
                    Vector2 rand = Random.insideUnitCircle * wanderRadius;
                    Vector3 target = transform.position + new Vector3(rand.x, 0, rand.y);
                    if (NavMesh.SamplePosition(target, out var hit, wanderRadius, NavMesh.AllAreas))
                    {
                        _agent.SetDestination(hit.position);
                    }
                }
            }
        }
    }
}
