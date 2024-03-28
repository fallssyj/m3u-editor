using Prism.Mvvm;

namespace m3u_editor.Common.Models
{
    public class configEntry : BindableBase
    {
        private bool isDark;

        public bool IsDark
        {
            get { return isDark; }
            set { isDark = value; }
        }

    }
}
