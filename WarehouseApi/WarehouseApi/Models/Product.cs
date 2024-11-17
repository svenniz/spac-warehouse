namespace WarehouseApi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        //I (Nikolaj) needed a constructor, to set up a mock database while testing my search function
        public Product(int id, string name, string description, string category)
        {
            Id = id;
            Name = name;
            Description = description;
            Category = category;
        }
    }
}