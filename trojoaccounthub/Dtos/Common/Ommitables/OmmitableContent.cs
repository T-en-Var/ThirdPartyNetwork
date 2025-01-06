namespace trojoaccounthub.Dtos.Common.Ommitables
{
    public class OmmitableContent
    {
        private string? _content;
        private bool _contentIsSet = false;

        public bool ContentIsSet => _contentIsSet;

        public string? Content
        {
            get => _content;
            init
            {
                _content = value;
                _contentIsSet = true;
            }
        }
    }
}
