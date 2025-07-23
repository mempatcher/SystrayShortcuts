namespace SystrayShortcuts
{
    public partial class TrayForm : Form
    {
        private TrayMain main;

        public TrayForm()
        {
            //InitializeComponent();
            main = new TrayMain();
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            base.OnLoad(e);
        }
    }
}
