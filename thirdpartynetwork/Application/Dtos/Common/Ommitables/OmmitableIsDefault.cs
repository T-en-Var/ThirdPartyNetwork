namespace thirdpartynetwork.Dtos.Common.Ommitables
{
    public class OmmitableIsDefault
    {
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
    }
}
