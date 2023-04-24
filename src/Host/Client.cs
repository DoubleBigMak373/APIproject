namespace ProjectS
{
    public class Client
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string BirthdayYear { get; set; }
        public string Email { get; set; }
        public string Number { get; set; }
        public string Adress { get; set; }

        public Client()
        {
            Id = Guid.NewGuid();
        }
    }
}
