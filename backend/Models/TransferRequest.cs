namespace backend.Models
{
    public class TransferRequest
    {
        public int GameId { get; set; }
        public string NewOwnerUsername { get; set; }
    }
}
