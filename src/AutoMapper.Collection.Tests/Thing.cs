namespace AutoMapper.Collection.Tests
{
    public class Thing
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public override string ToString() { return Title; }
    }
}