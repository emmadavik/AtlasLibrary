namespace AtlasLibrary.UsersApi.Models
{
    public class Anvandare
    {
        public int Id { get; set; }

        public string Namn { get; set; } = string.Empty;

        public string Epost { get; set; } = string.Empty;

        public string Losenord { get; set; } = string.Empty;

        public string Roll { get; set; } = string.Empty;
    }
}