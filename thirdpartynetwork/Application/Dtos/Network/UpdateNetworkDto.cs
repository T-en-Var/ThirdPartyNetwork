using System.ComponentModel.DataAnnotations;
using thirdpartynetwork.Application.Dtos.Common.Ommitables;

namespace thirdpartynetwork.Application.Dtos.Network
{
    public class UpdateNetworkDto
    {
        private string? _name;
        private bool _nameIsSet = false;

        public string? Name
        {
            get => _name;
            init
            {
                _name = value;
                _nameIsSet = true;
            }
        }
        public bool NameIsSet => _nameIsSet;
    }
}
