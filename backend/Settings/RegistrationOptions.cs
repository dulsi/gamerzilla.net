namespace backend.Settings
{
    public class RegistrationOptions
    {
        public bool Allow { get; set; }
        public bool RequireApproval { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
    }
}
