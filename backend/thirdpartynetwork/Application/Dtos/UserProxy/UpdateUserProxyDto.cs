using thirdpartynetwork.Dtos.Common.Ommitables;

namespace thirdpartynetwork.Dtos.UserProxy
{
    public class UpdateUserProxyDto : OmmitableUsername
    {
        private string? _email;
        private bool _emailIsSet = false;

        public bool EmailIsSet => _emailIsSet;

        public string? Email
        {
            get => _email;
            init
            {
                _email = value;
                _emailIsSet = true;
            }
        }
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
        private bool? _isDefault;
        private bool _isDefaultIsSet = false;

        public bool IsDefaultIsSet => _isDefaultIsSet;

        public bool? IsDefault
        {
            get => _isDefault;
            init
            {
                _isDefault = value;
                _isDefaultIsSet = true;
            }
        }
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
        private string? _profilePicture;
        private bool _profilePictureIsSet = false;

        public bool ProfilePictureIsSet => _profilePictureIsSet;

        public string? ProfilePicture
        {
            get => _profilePicture;
            init
            {
                _profilePicture = value;
                _profilePictureIsSet = true;
            }
        }
    }
}
