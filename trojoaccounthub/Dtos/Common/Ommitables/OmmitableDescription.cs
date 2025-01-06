namespace trojoaccounthub.Dtos.Common.Ommitables
{
    public class OmmitableDescription
    {
        private string? _description;
        private bool _descriptionIsSet = false;

        public bool DescriptionIsSet => _descriptionIsSet;

        public string? Description
        {
            get => _description;
            init
            {
                _description = value;
                _descriptionIsSet = true;
            }
        }
    }
}
