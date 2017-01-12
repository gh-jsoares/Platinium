using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginInterface
{
    public interface IPluginG
    {
        IPluginHost Host { get; set; }

        string Name { get; }
        string Description { get; }
        string Author { get; }
        string Version { get; }

        UserControl MainInterface { get; }

        void Initialize();
        void Dispose();

    }

    public interface IPluginHost
    {
        void Feedback(string Feedback, IPluginG Plugin);
    }
}
