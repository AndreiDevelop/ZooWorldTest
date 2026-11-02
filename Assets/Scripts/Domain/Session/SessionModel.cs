using System;

namespace ZooWorld.Domain.Session
{
    public sealed class SessionModel
    {
        public event Action<SessionState> StateChanged;

        public SessionState State { get; private set; } = SessionState.Created;
        public bool MainReady { get; private set; }
        public bool HudReady { get; private set; }
        public string FailureReason { get; private set; }
        public bool CanSimulate => State == SessionState.Ready;

        public void BeginLoading()
        {
            if (State != SessionState.Created)
            {
                throw new InvalidOperationException("Only a new session can begin loading.");
            }

            SetState(SessionState.Loading);
        }

        public void MarkMainReady()
        {
            RequireLoading();
            MainReady = true;
            TryBecomeReady();
        }

        public void MarkHudReady()
        {
            RequireLoading();
            HudReady = true;
            TryBecomeReady();
        }

        public void BeginStopping()
        {
            if (State == SessionState.Stopping || State == SessionState.Stopped)
            {
                return;
            }

            MainReady = false;
            HudReady = false;
            SetState(SessionState.Stopping);
        }

        public void MarkStopped()
        {
            if (State != SessionState.Stopping)
            {
                throw new InvalidOperationException("A session must stop its producers before unloading.");
            }

            SetState(SessionState.Stopped);
        }

        public void Fail(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("A session failure requires a reason.", nameof(reason));
            }

            FailureReason = reason;
            MainReady = false;
            HudReady = false;
            SetState(SessionState.Failed);
        }

        private void RequireLoading()
        {
            if (State != SessionState.Loading)
            {
                throw new InvalidOperationException("Scene readiness is accepted only while loading.");
            }
        }

        private void TryBecomeReady()
        {
            if (MainReady && HudReady)
            {
                SetState(SessionState.Ready);
            }
        }

        private void SetState(SessionState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
