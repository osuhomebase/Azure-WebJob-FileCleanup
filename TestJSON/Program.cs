using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestJSON
{
    class Program
    {
        static void Main(string[] args)
        {
            using (StreamReader r = new StreamReader("Profiles.json"))
            {
                var json = r.ReadToEnd();
                var items = JsonConvert.DeserializeObject<List<StorageUnit>>(json);
                foreach (var item in items)
                {
                     Console.WriteLine("{0} {1} {2}", item.containerName, item.connectionString, item.cleanupHours);
                }
            }
            Console.ReadKey();

        }
    }
    class StorageUnit
    {
        public string containerName { get; set; }
        public string connectionString { get; set; }
        public string cleanupHours { get; set; }
    }
}
