using System.IO;
using System.Reflection;
using System.Windows;
using SpieRibbon.Chrome;

namespace SpieRibbon.UI
{
    public partial class PatchNotesWindow : Window
    {
        public PatchNotesWindow()
        {
            InitializeComponent();
            SpieChrome.Apply(this, TitleBarHost, "Patch notes");
            NotesText.Text = LoadChangelog();
        }

        private static string LoadChangelog()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("SpieRibbon.CHANGELOG.md"))
            {
                if (stream == null)
                    return "Patch notes are not available in this build.";

                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }
    }
}
