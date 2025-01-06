namespace thirdpartynetwork.Dtos.Common.Ommitables
{
    public class OmmitableEmail
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
    }
}
