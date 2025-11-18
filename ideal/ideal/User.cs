using ideal.Config;
using ideal.Forms;
using System.Windows.Forms;

namespace ideal
{
    public class User
    {
        public static cxSistem MySistem1 = null;
        public static cxSistem MySistem2 = null;
        public static cxSistem MySistem3 = null;
        public static cxSistem MySistem4 = null;
        public static cxSistem MySistem5 = null;
        public static cxSistem MySistem6 = null;
        public static cxSistem MySistem7 = null;
        public static cxSistem MySistem8 = null;
        public static cxSistem MySistem9 = null;
        public static cxSistem MySistem10 = null;




        public void FormYukle(cxSistem sistem1, cxSistem sistem2, cxSistem sistem3, cxSistem sistem4, cxSistem sistem5, cxSistem sistem6, cxSistem sistem7, cxSistem sistem8, cxSistem sistem9, cxSistem sistem10)
        {
            MySistem1 = sistem1;
            MySistem2 = sistem2;
            MySistem3 = sistem3;
            MySistem4 = sistem4;
            MySistem5 = sistem5;
            MySistem6 = sistem6;
            MySistem7 = sistem7;
            MySistem8 = sistem8;   
            MySistem9 = sistem9;
            MySistem10 = sistem10;

            var config = AppConfig.Instance;

            if (FrmMain.Reference == null)
            {
                AppInitializer.Initialize();
                FrmMain.Reference = new FrmMain();
                FrmMain.Reference.FormClosing += Reference_FormClosing;
            }

            FrmMain.Reference.Show();

        }

        private void Reference_FormClosing(object sender, FormClosingEventArgs e)
        {
            FrmMain.Reference = null;
        }
    }
}


