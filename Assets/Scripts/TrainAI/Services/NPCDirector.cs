using System;
using System.Collections.Generic;
using TrainAI.Core;
using TrainAI.Core.Messages;
using TrainAI.SO.Base;
using UnityEngine;

namespace TrainAI.Services
{
    public class NPCDirector : INPCDirector, IDisposable
    {
        class NpcState
        {
            public AreaSO currentTarget;
            public float wanderTimer;
            public Vector3 wanderPos;
        }

        readonly NPCDB _npcDB;
        readonly IMovementService _movement;
        readonly GameClockRSO _clock;
        
        readonly Dictionary<string, Transform> _registered = new();
        readonly Dictionary<string, NpcState> _states = new();
        readonly List<NPCSO> _scratch = new();

        public NPCDirector(NPCDB npcDB, IMovementService movement, GameClockRSO clock)
        {
            _npcDB = npcDB;
            _movement = movement;
            _clock = clock;
            BroadcastService.Subscribe<TimeTickMsg>(OnTimeTick);
        }

        public void Dispose() => BroadcastService.Unsubscribe<TimeTickMsg>(OnTimeTick);

        public void RegisterNpcTransform(string npcId, Transform t)
        {
            if (string.IsNullOrEmpty(npcId) || t == null) return;
            _registered[npcId] = t;
            
            var npc = _npcDB != null ? _npcDB.ById(npcId) : null;
            if (npc != null && npc.movement != null)
            {
                _movement?.RegisterAgent(t, npc.movement);
                _states[npcId] = new NpcState();
            }
        }

        void OnTimeTick(TimeTickMsg msg)
        {
            if (_npcDB == null || _npcDB.all == null) return;
            
            _scratch.Clear();
            for (int i = 0; i < _npcDB.all.Count; i++) _scratch.Add(_npcDB.all[i]);

            for (int i = 0; i < _scratch.Count; i++)
            {
                var npc = _scratch[i];
                if (npc == null || npc.schedule == null) continue;
                if (!_registered.TryGetValue(npc.id, out var t) || t == null) continue;
                if (!_states.TryGetValue(npc.id, out var state)) continue;

                try
                {
                    var entry = npc.schedule.GetEntryAt(_clock.day, msg.hour, msg.minute);
                    if (entry.target == null) continue;
                    
                    if (state.currentTarget != entry.target)
                    {
                        state.currentTarget = entry.target;
                        state.wanderTimer = 0f;
                        _movement?.SetTarget(t, entry.target.worldPos);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[NPCDirector] schedule eval for '{npc.id}' threw: {e.Message}");
                }
            }
        }

        public void Tick(float dt)
        {
            List<string> deadKeys = null;
            // Handle local wandering around the current schedule target
            foreach (var kvp in _registered)
            {
                var t = kvp.Value;
                if (t == null)
                {
                    if (deadKeys == null) deadKeys = new List<string>();
                    deadKeys.Add(kvp.Key);
                    continue;
                }

                if (!_states.TryGetValue(kvp.Key, out var state)) continue;
                if (state.currentTarget == null) continue;

                // If near the target or already wandering
                float distToCenter = (t.position - state.currentTarget.worldPos).magnitude;
                
                state.wanderTimer -= dt;
                if (state.wanderTimer <= 0)
                {
                    state.wanderTimer = UnityEngine.Random.Range(5f, 15f);
                    
                    // Pick a random spot around the target to wander to
                    Vector2 rand = UnityEngine.Random.insideUnitCircle * 5f;
                    Vector3 rawWanderPos = state.currentTarget.worldPos + new Vector3(rand.x, 0, rand.y);
                    
                    if (UnityEngine.AI.NavMesh.SamplePosition(rawWanderPos, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        var path = new UnityEngine.AI.NavMeshPath();
                        if (UnityEngine.AI.NavMesh.CalculatePath(t.position, hit.position, UnityEngine.AI.NavMesh.AllAreas, path))
                        {
                            if (path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                            {
                                state.wanderPos = hit.position;
                                _movement?.SetTarget(t, state.wanderPos);
                            }
                        }
                    }
                }
            }

            if (deadKeys != null)
            {
                foreach (var k in deadKeys)
                {
                    _registered.Remove(k);
                    _states.Remove(k);
                }
            }
        }
    }
}
