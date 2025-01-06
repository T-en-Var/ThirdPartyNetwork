namespace trojoaccounthub.Dtos.Common.Ommitables
{
    public class OmmitableLastName
    {
        private string? _lastName;
        private bool _lastNameIsSet = false;

        public bool LastNameIsSet => _lastNameIsSet;

        public string? LastName
        {
            get => _lastName;
            init
            {
                _lastName = value;
                _lastNameIsSet = true;
            }
        }
    }
}
