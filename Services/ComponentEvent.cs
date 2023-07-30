namespace XBOT.Services
{
    public class ComponentEvent<T>
    {
        public HashSet<string> _GuidId { get; }
        public T Component { get; set; }
        private TaskCompletionSource<T> _interactionCompletionSource;
        private CancellationTokenSource _cancellationTokenSource;

        public ComponentEvent(HashSet<string> GuidId = null)
        {
            if (GuidId == null)
                GuidId = new HashSet<string>();

            _GuidId = GuidId;
            _interactionCompletionSource = new TaskCompletionSource<T>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task AddGuid(string GuidId)
        {
            _GuidId.Add(GuidId);
        }

        public async Task<T> WaitForInteraction(double Seconds = 60)
        {
            var cancellationToken = _cancellationTokenSource.Token;
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(Seconds), cancellationToken);

            var completedTask = await Task.WhenAny(_interactionCompletionSource.Task, timeoutTask);
            if (completedTask == _interactionCompletionSource.Task)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await _interactionCompletionSource.Task;
            }
            else
            {
                return default(T);
            }
        }

        public void CompleteInteraction()
        {
            _interactionCompletionSource.SetResult(Component);
            _cancellationTokenSource.Cancel();
        }
    }

    public class ComponentEventService
    {
        private Dictionary<string, object> _userInteractions;

        public ComponentEventService()
        {
            _userInteractions = new Dictionary<string, object>();
        }

        public void AddInteraction<T>(HashSet<string> guidIds, ComponentEvent<T> userInteraction)
        {
            foreach (var guidId in guidIds)
            {
                _userInteractions[guidId] = userInteraction;
            }
        }

        public ComponentEvent<T> GetInteraction<T>(string GuidId)
        {
            if (_userInteractions.TryGetValue(GuidId, out var interaction))
            {
                return (ComponentEvent<T>)interaction;
            }
            return null;
        }

        public void RemoveInteraction(string GuidId)
        {
            _userInteractions.Remove(GuidId);
        }
    }
}
