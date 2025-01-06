namespace trojoaccounthub.Dtos.Common.Ommitables
{
    public class OmmitableName
    {
        private string? _name;
        private bool _nameIsSet = false;

        public bool NameIsSet => _nameIsSet;

        public string? Name
        {
            get => _name;
            init
            {
                _name = value;
                _nameIsSet = true;
            }
        }
    }
}
