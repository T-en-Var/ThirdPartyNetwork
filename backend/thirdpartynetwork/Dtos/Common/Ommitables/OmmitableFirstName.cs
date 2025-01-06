namespace thirdpartynetwork.Dtos.Common.Ommitables
{
    public class OmmitableFirstName
    {
        private string? _firstName;
        private bool _firstNameIsSet = false;

        public bool FirstNameIsSet => _firstNameIsSet;

        public string? FirstName
        {
            get => _firstName;
            init
            {
                _firstName = value;
                _firstNameIsSet = true;
            }
        }
    }
}
