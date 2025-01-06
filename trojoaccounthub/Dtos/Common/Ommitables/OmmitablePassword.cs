namespace trojoaccounthub.Dtos.Common.Ommitables
{
    public class OmmitablePassword
    {
        private string? _password;
        private bool _passwordIsSet = false;

        public bool PasswordIsSet => _passwordIsSet;

        public string? Password
        {
            get => _password;
            init
            {
                _password = value;
                _passwordIsSet = true;
            }
        }
    }
}
