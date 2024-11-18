using WarehouseApi.Data_Access;

namespace WarehouseApi.Models
{
    /*
    public static class ProductSeeder
    {
        /// <summary>
        /// WARNING, THIS DELETES THE EXISTING DATABASE TABLES! DO NOT USE UNLESS YOU WANT TO DELETE ALL DATA
        /// 
        /// Seeder function, empties the warehouse and loads the products from a file
        /// The 'this' keyword allows us to call WarehouseContext.Seed(filepath)
        /// Errors fall through
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filepath"></param>
        public static void Seed(this WarehouseContext context, string filepath)
        {
            using (StreamReader reader = new StreamReader(filepath))
            {
                //Empty existing database
                context.Products.RemoveRange(context.Products);

                //Read all lines, one at the time
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    //Read as name, category , 
                    var elements = line.Split(',', StringSplitOptions.TrimEntries);
                    if (elements.Length >= 3)
                    {
                        context.Products.Add(new Product(context.Products.Count(), elements[0], elements[2], elements[1]));
                    }
                }

                //Done, update
                context.SaveChanges();
            }
        }
    }*/
}
