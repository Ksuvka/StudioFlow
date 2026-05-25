namespace StudioFlow.Services
{
    public class AppStateService
    {
        public event Action? OnAuthStateChanged;

        private bool _isAuthenticated = false;
        private string _userName = "";
        private string _userRole = "";
        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set
            {
                if (_isAuthenticated != value)
                {
                    _isAuthenticated = value;
                    NotifyAuthStateChanged();
                }
            }
        }

        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    NotifyAuthStateChanged();
                }
            }
        }

        public string UserRole
        {
            get => _userRole;
            set
            {
                if (_userRole != value)
                {
                    _userRole = value;
                    NotifyAuthStateChanged();
                }
            }
        }


        private void NotifyAuthStateChanged() => OnAuthStateChanged?.Invoke();

        public void RefreshAuthState() => NotifyAuthStateChanged();
    }
}