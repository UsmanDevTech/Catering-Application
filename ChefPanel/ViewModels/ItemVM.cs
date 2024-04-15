using Domain.Entities;

namespace ChefPanel.ViewModels
{
    public class ItemsGetVM
    {
        public int id { get; set; }
        public string name { get; set; }

        public string createdAt { get; set; }
        public bool isDeleted { get; set; }
    }

    public class ItemCreateVM
    {
        public string name { get; set; }
    }

    public class ItemEditVM
    {
        public int id { get; set; }
        public string name { get; set; }
    }


}
