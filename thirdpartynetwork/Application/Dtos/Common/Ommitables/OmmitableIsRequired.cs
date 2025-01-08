namespace thirdpartynetwork.Dtos.Common.Ommitables
{
    public class OmmitableIsRequired
    {
        private bool? _isRequired;
        private bool _isRequiredIsSet = false;

        public bool IsRequiredIsSet => _isRequiredIsSet;

        public bool? IsRequired
        {
            get => _isRequired;
            init
            {
                _isRequired = value;
                _isRequiredIsSet = true;
            }
        }
    }
}
