namespace AzureAdApp.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
