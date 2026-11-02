using System;
using UnityEngine;
using Zenject;
using ZooWorld.Domain.Session;

namespace ZooWorld.Infrastructure
{
    public abstract class SessionSceneRoot : MonoBehaviour
    {
        public SessionModel Session { get; private set; }
        public DeathStatisticsModel Statistics { get; private set; }

        [Inject]
        public void Construct(SessionModel session, DeathStatisticsModel statistics)
        {
            Session = session;
            Statistics = statistics;
        }

        public virtual void ValidateReadiness()
        {
            if (Session == null || Statistics == null)
            {
                throw new InvalidOperationException($"Scene root '{name}' has not received its shared session models.");
            }
        }

        public virtual void Release()
        {
            Session = null;
            Statistics = null;
        }

        private void OnDestroy()
        {
            Release();
        }
    }
}
