namespace PassKee.Orm.Dto
{
    public class ListDto<T>
    {
        public virtual ICollection<T> Items { get; set; } = new List<T>();
        
        public virtual int TotalCount { get; set; }

        public ListDto()
        {
        }

        public ListDto(ICollection<T> items, int count)
        {
            Items = items;
            TotalCount = count;
        }
    }
}
