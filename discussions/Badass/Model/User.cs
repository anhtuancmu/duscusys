namespace Badass.Model
{
    public class User
    {
        public User()
        {            
        }

        public User(int id, string name, int color)
        {
            Id = id;
            Name = name;
            Color = color;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Color { get; set; }
    }
}