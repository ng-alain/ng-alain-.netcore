using System.Collections.Generic;

namespace asdf.Models
{
    public class App
    {
        public Project project { get; set; }

        public User user { get; set; }

        public List<Menu> menu { get; set; }
    }

    public class Project
    {
        public string name { get; set; }
    }

    public class Menu
    {
        public string text { get; set; }
        public bool group { get; set; }
        public bool shortcut_root { get; set; }
        public string link { get; set; }
        public string icon { get; set; }
        public bool linkExact { get; set; } = true;
        public List<Menu> children { get; set; }
    }
}
