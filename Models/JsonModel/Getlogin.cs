namespace TodoApi.Models.JsonModel
{
    public class Getlogin
    {
        public string login { get; set; }
        public bool authorized { get; set; }
        public long id { get; set; }
        public string role { get; set; }
    }
}