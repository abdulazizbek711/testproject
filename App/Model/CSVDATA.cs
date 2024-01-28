using CsvHelper.Configuration.Attributes;

namespace App
{
    public class CSVDATA
    {
        [Index(0)]
        public string UserId { get; set; }
    
        [Index(1)]
        public string Name { get; set; }
    
        [Index(2)]
        public string SecondName { get; set; }
    
        [Index(3)]
        public string Number { get; set; }
    }
}