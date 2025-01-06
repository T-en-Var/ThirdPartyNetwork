namespace trojoaccounthub.Dtos.Common.Ommitables
{
    public class OmmitableProfilePicture
    {
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
