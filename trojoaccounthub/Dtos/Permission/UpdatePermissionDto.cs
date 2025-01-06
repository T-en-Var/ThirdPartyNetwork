namespace trojoaccounthub.Dtos.Permission
{
    public class UpdatePermissionDto : OmmitableName
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
