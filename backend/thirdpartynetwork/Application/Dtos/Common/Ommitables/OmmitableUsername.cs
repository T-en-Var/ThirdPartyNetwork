namespace thirdpartynetwork.Application.Dtos.Common.Ommitables
{
    public class OmmitableUsername
    {
        private string? _username;
        private bool _usernameIsSet = false;

        public bool UsernameIsSet => _usernameIsSet;

        public string? Username
        {
            get => _username;
            init
            {
                _username = value;
                _usernameIsSet = true;
            }
        }
    }
}
