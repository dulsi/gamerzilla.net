namespace backend.Dto;

public class UserInfoDto
{
    public string userName { get; set; }

    public string password { get; set; }

    public bool admin { get; set; }

    public bool visible { get; set; }

    public bool approved { get; set; }

    public bool canApprove { get; set; }
}
