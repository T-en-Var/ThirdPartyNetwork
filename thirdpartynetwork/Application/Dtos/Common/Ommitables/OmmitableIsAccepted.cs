namespace thirdpartynetwork.Dtos.Common.Ommitables
{
    public class OmmitableIsAccepted
    {
        private bool? _isAccepted;
        private bool _isAcceptedIsSet = false;

        public bool IsAcceptedIsSet => _isAcceptedIsSet;

        public bool? IsAccepted
        {
            get => _isAccepted;
            init
            {
                _isAccepted = value;
                _isAcceptedIsSet = true;
            }
        }
    }
}
