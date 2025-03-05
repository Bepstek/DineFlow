namespace dineflow.ViewModel
{
    public class InventoryDetailViewModel
    {
        public int InventoryId { get; set; }
        public List<InventoryDetailItem> Items { get; set; }
    }

    public class InventoryDetailItem
    {
        public string ItemName { get; set; }
        public string UnitMeasure { get; set; }
        public int Quantity { get; set; }
    }

}
