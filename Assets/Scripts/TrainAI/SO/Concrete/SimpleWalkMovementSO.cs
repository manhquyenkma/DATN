using TrainAI.Core;
using TrainAI.SO.Base;
using UnityEngine;

namespace TrainAI.SO.Concrete
{
    // Lightweight transform-only NPC mover. Walks toward Target at `speed`
    // m/s, snaps to the ground via a downward raycast each tick, and yaws
    // to face the heading. Designed for a campus with no baked NavMesh
    // (10_World currently has m_NavMeshData=0) so the NavMesh strategy
    // would silently no-op. SimpleWalk doesn't pathfind around obstacles,
    // but for the GDD's free-roam student NPCs ("học sinh đi lại theo
    // thời gian biểu"), straight-line locomotion between scheduled
    // AreaSO waypoints reads correctly from the player's perspective and
    // avoids the cost of baking a 200×140 NavMesh.
    //
    // Update cadence: MovementService.Tick fires at 0.2s intervals, so
    // each Tick gets ~0.2s of dt. Speed scales accordingly — 2.5 m/s ⇒
    // 0.5m per tick, which is visible motion but doesn't teleport.
    [CreateAssetMenu(fileName = "Movement_SimpleWalk", menuName = "TrainAI/Movement/Simple Walk")]
    public class SimpleWalkMovementSO : MovementStrategySO
    {
        [Tooltip("Walking speed in metres per second.")]
        public float speed = 2.0f;
        [Tooltip("Distance under which the agent is considered 'arrived'. Prevents oscillation.")]
        public float arriveRadius = 0.5f;
        [Tooltip("Degrees per second the agent yaws toward heading. 360 = snap.")]
        public float turnSpeed = 240f;

        public override IMovementAgent Bind(Transform npc)
            => new SimpleWalkAgent(npc, speed, arriveRadius, turnSpeed);
    }

    internal class SimpleWalkAgent : IMovementAgent
    {
        readonly Transform _t;
        readonly float _speed;
        readonly float _arriveRadius;
        readonly float _turnSpeed;

        // When the NPC reaches its target, it picks random patrol offsets
        // around the target so it looks alive instead of freezing in place.
        const float kPatrolRadius = 3f;
        Vector3 _patrolPoint;
        bool _patrolling;

        Vector3 _target;
        public Vector3 Target
        {
            get => _target;
            set { if (_target != value) { _target = value; _patrolling = false; } }
        }

        public SimpleWalkAgent(Transform t, float speed, float arriveRadius, float turnSpeed)
        {
            _t = t;
            _speed = speed;
            _arriveRadius = arriveRadius;
            _turnSpeed = turnSpeed;
        }

        public void Tick(object _, float dt)
        {
            if (_t == null) return;
            Vector3 cur = _t.position;
            Vector3 destination = _patrolling ? _patrolPoint : Target;
            Vector3 to = destination - cur;
            to.y = 0f;
            float dist = to.magnitude;

            if (dist < _arriveRadius)
            {
                // Arrived at main target → pick a random patrol point nearby
                // Arrived at patrol point → pick another one
                PickNewPatrolPoint();
                return;
            }

            Vector3 dir = to / dist;
            // Patrol speed is slower (60%) so NPC looks like it's casually walking around
            float currentSpeed = _patrolling ? _speed * 0.6f : _speed;
            float step = Mathf.Min(currentSpeed * dt, dist);
            Vector3 next = cur + dir * step;

            // Ground-snap
            if (Physics.Raycast(next + Vector3.up * 2f, Vector3.down, out var hit, 6f, ~0, QueryTriggerInteraction.Ignore))
                next.y = hit.point.y;
            else
                next.y = 0f;

            _t.position = next;

            Quaternion want = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z), Vector3.up);
            _t.rotation = Quaternion.RotateTowards(_t.rotation, want, _turnSpeed * dt);
        }

        void PickNewPatrolPoint()
        {
            _patrolling = true;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(1f, kPatrolRadius);
            _patrolPoint = Target + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }
    }
}
