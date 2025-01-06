namespace trojoaccounthub.Dtos.Common.Ommitables
{
    public class OmmitableTitle
    {
        private string? _title;
        private bool _titleIsSet = false;

        public bool TitleIsSet => _titleIsSet;

        public string? Title
        {
            get => _title;
            init
            {
                _title = value;
                _titleIsSet = true;
            }
        }
    }
}
