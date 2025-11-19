namespace ideal.Forms
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabPaneMain = new DevExpress.XtraBars.Navigation.TabPane();
            this.tabPageMain = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            this.grupControlStart = new DevExpress.XtraEditors.GroupControl();
            this.checkEditAlgobarDataContext = new DevExpress.XtraEditors.CheckEdit();
            this.txtSembol = new DevExpress.XtraEditors.TextEdit();
            this.txtEndCalendar = new DevExpress.XtraEditors.DateEdit();
            this.checkEditGeistContext = new DevExpress.XtraEditors.CheckEdit();
            this.txtStartCalendar = new DevExpress.XtraEditors.DateEdit();
            this.checkEditSeansClosed = new DevExpress.XtraEditors.CheckEdit();
            this.btnStart = new DevExpress.XtraEditors.SimpleButton();
            this.btnStop = new DevExpress.XtraEditors.SimpleButton();
            this.groupControlProgress = new DevExpress.XtraEditors.GroupControl();
            this.tabPane1 = new DevExpress.XtraBars.Navigation.TabPane();
            this.tabNavigationPage1 = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            this.btnImkbBarDataGunSonu = new DevExpress.XtraEditors.CheckButton();
            this.progressBarImkbBarDataGunSonu = new DevExpress.XtraEditors.ProgressBarControl();
            this.btnImkbTaramaSRSI = new DevExpress.XtraEditors.CheckButton();
            this.progressBarTaramaSRSI = new DevExpress.XtraEditors.ProgressBarControl();
            this.btnImkbTaramaUyumsuzluk = new DevExpress.XtraEditors.CheckButton();
            this.progressBarTaramaUyumsuzluk = new DevExpress.XtraEditors.ProgressBarControl();
            this.btnImkbKompozitData = new DevExpress.XtraEditors.CheckButton();
            this.progressBarKompozitData = new DevExpress.XtraEditors.ProgressBarControl();
            this.btnImkbBarData = new DevExpress.XtraEditors.CheckButton();
            this.progressBarImkbBarData = new DevExpress.XtraEditors.ProgressBarControl();
            this.btnImkbDD = new DevExpress.XtraEditors.CheckButton();
            this.btnImkbYuzeyselVeri = new DevExpress.XtraEditors.CheckButton();
            this.progressBarImkbYuzeyselVeri = new DevExpress.XtraEditors.ProgressBarControl();
            this.progressBarDD = new DevExpress.XtraEditors.ProgressBarControl();
            this.tabNavigationPage2 = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            this.progressBarImkbBarDataTekSembol = new DevExpress.XtraEditors.ProgressBarControl();
            this.btnImkbBarDataTekSembol = new DevExpress.XtraEditors.CheckButton();
            this.btnImkbBarDataSembol = new DevExpress.XtraEditors.CheckButton();
            this.progressBarImkbBarDataSembol = new DevExpress.XtraEditors.ProgressBarControl();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.tabPaneMain)).BeginInit();
            this.tabPaneMain.SuspendLayout();
            this.tabPageMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grupControlStart)).BeginInit();
            this.grupControlStart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditAlgobarDataContext.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSembol.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtEndCalendar.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtEndCalendar.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditGeistContext.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtStartCalendar.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtStartCalendar.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditSeansClosed.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControlProgress)).BeginInit();
            this.groupControlProgress.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tabPane1)).BeginInit();
            this.tabPane1.SuspendLayout();
            this.tabNavigationPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarImkbBarDataGunSonu.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarTaramaSRSI.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarTaramaUyumsuzluk.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarKompozitData.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarImkbBarData.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarImkbYuzeyselVeri.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarDD.Properties)).BeginInit();
            this.tabNavigationPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarImkbBarDataTekSembol.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarImkbBarDataSembol.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // tabPaneMain
            // 
            this.tabPaneMain.Controls.Add(this.tabPageMain);
            this.tabPaneMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPaneMain.Location = new System.Drawing.Point(0, 0);
            this.tabPaneMain.Name = "tabPaneMain";
            this.tabPaneMain.Pages.AddRange(new DevExpress.XtraBars.Navigation.NavigationPageBase[] {
            this.tabPageMain});
            this.tabPaneMain.RegularSize = new System.Drawing.Size(473, 554);
            this.tabPaneMain.SelectedPage = this.tabPageMain;
            this.tabPaneMain.Size = new System.Drawing.Size(473, 554);
            this.tabPaneMain.TabIndex = 0;
            this.tabPaneMain.Text = "tabPane1";
            // 
            // tabPageMain
            // 
            this.tabPageMain.Caption = "Genel";
            this.tabPageMain.Controls.Add(this.grupControlStart);
            this.tabPageMain.Controls.Add(this.groupControlProgress);
            this.tabPageMain.Name = "tabPageMain";
            this.tabPageMain.Size = new System.Drawing.Size(473, 521);
            // 
            // grupControlStart
            // 
            this.grupControlStart.Controls.Add(this.checkEditAlgobarDataContext);
            this.grupControlStart.Controls.Add(this.txtSembol);
            this.grupControlStart.Controls.Add(this.txtEndCalendar);
            this.grupControlStart.Controls.Add(this.checkEditGeistContext);
            this.grupControlStart.Controls.Add(this.txtStartCalendar);
            this.grupControlStart.Controls.Add(this.checkEditSeansClosed);
            this.grupControlStart.Controls.Add(this.btnStart);
            this.grupControlStart.Controls.Add(this.btnStop);
            this.grupControlStart.Location = new System.Drawing.Point(12, 14);
            this.grupControlStart.Name = "grupControlStart";
            this.grupControlStart.Size = new System.Drawing.Size(449, 112);
            this.grupControlStart.TabIndex = 40;
            this.grupControlStart.Text = "Kontrol";
            this.grupControlStart.DoubleClick += new System.EventHandler(this.grupControlStart_DoubleClick);
            // 
            // checkEditAlgobarDataContext
            // 
            this.checkEditAlgobarDataContext.Enabled = false;
            this.checkEditAlgobarDataContext.Location = new System.Drawing.Point(330, 78);
            this.checkEditAlgobarDataContext.Name = "checkEditAlgobarDataContext";
            this.checkEditAlgobarDataContext.Properties.Caption = "DB - Algobar Data";
            this.checkEditAlgobarDataContext.Size = new System.Drawing.Size(112, 20);
            this.checkEditAlgobarDataContext.TabIndex = 16;
            this.checkEditAlgobarDataContext.TabStop = false;
            // 
            // txtSembol
            // 
            this.txtSembol.EditValue = "IMKBH\'";
            this.txtSembol.Location = new System.Drawing.Point(217, 52);
            this.txtSembol.Name = "txtSembol";
            this.txtSembol.Size = new System.Drawing.Size(100, 20);
            this.txtSembol.TabIndex = 15;
            this.txtSembol.Visible = false;
            // 
            // txtEndCalendar
            // 
            this.txtEndCalendar.EditValue = null;
            this.txtEndCalendar.Location = new System.Drawing.Point(111, 52);
            this.txtEndCalendar.Name = "txtEndCalendar";
            this.txtEndCalendar.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.txtEndCalendar.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.txtEndCalendar.Size = new System.Drawing.Size(100, 20);
            this.txtEndCalendar.TabIndex = 14;
            this.txtEndCalendar.TabStop = false;
            this.txtEndCalendar.Visible = false;
            // 
            // checkEditGeistContext
            // 
            this.checkEditGeistContext.Enabled = false;
            this.checkEditGeistContext.Location = new System.Drawing.Point(330, 52);
            this.checkEditGeistContext.Name = "checkEditGeistContext";
            this.checkEditGeistContext.Properties.Caption = "DB - Geist Panel";
            this.checkEditGeistContext.Size = new System.Drawing.Size(100, 20);
            this.checkEditGeistContext.TabIndex = 13;
            this.checkEditGeistContext.TabStop = false;
            // 
            // txtStartCalendar
            // 
            this.txtStartCalendar.EditValue = null;
            this.txtStartCalendar.Location = new System.Drawing.Point(5, 52);
            this.txtStartCalendar.Name = "txtStartCalendar";
            this.txtStartCalendar.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.txtStartCalendar.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.txtStartCalendar.Size = new System.Drawing.Size(100, 20);
            this.txtStartCalendar.TabIndex = 12;
            this.txtStartCalendar.TabStop = false;
            // 
            // checkEditSeansClosed
            // 
            this.checkEditSeansClosed.Location = new System.Drawing.Point(330, 26);
            this.checkEditSeansClosed.Name = "checkEditSeansClosed";
            this.checkEditSeansClosed.Properties.Caption = "Seans Kapalı";
            this.checkEditSeansClosed.Size = new System.Drawing.Size(100, 20);
            this.checkEditSeansClosed.TabIndex = 11;
            this.checkEditSeansClosed.TabStop = false;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(5, 26);
            this.btnStart.Name = "btnStart";
            this.btnStart.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnStart.Size = new System.Drawing.Size(100, 20);
            this.btnStart.TabIndex = 9;
            this.btnStart.TabStop = false;
            this.btnStart.Text = "Başlat";
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(111, 26);
            this.btnStop.Name = "btnStop";
            this.btnStop.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnStop.Size = new System.Drawing.Size(100, 20);
            this.btnStop.TabIndex = 10;
            this.btnStop.TabStop = false;
            this.btnStop.Text = "Durdur";
            // 
            // groupControlProgress
            // 
            this.groupControlProgress.Controls.Add(this.tabPane1);
            this.groupControlProgress.Location = new System.Drawing.Point(12, 132);
            this.groupControlProgress.Name = "groupControlProgress";
            this.groupControlProgress.Size = new System.Drawing.Size(449, 377);
            this.groupControlProgress.TabIndex = 39;
            this.groupControlProgress.Text = "Veri";
            // 
            // tabPane1
            // 
            this.tabPane1.Controls.Add(this.tabNavigationPage1);
            this.tabPane1.Controls.Add(this.tabNavigationPage2);
            this.tabPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPane1.Location = new System.Drawing.Point(2, 23);
            this.tabPane1.Name = "tabPane1";
            this.tabPane1.Pages.AddRange(new DevExpress.XtraBars.Navigation.NavigationPageBase[] {
            this.tabNavigationPage1,
            this.tabNavigationPage2});
            this.tabPane1.RegularSize = new System.Drawing.Size(445, 352);
            this.tabPane1.SelectedPage = this.tabNavigationPage1;
            this.tabPane1.Size = new System.Drawing.Size(445, 352);
            this.tabPane1.TabIndex = 72;
            this.tabPane1.Text = "tabPane1";
            // 
            // tabNavigationPage1
            // 
            this.tabNavigationPage1.Caption = "Günlük";
            this.tabNavigationPage1.Controls.Add(this.btnImkbBarDataGunSonu);
            this.tabNavigationPage1.Controls.Add(this.progressBarImkbBarDataGunSonu);
            this.tabNavigationPage1.Controls.Add(this.btnImkbTaramaSRSI);
            this.tabNavigationPage1.Controls.Add(this.progressBarTaramaSRSI);
            this.tabNavigationPage1.Controls.Add(this.btnImkbTaramaUyumsuzluk);
            this.tabNavigationPage1.Controls.Add(this.progressBarTaramaUyumsuzluk);
            this.tabNavigationPage1.Controls.Add(this.btnImkbKompozitData);
            this.tabNavigationPage1.Controls.Add(this.progressBarKompozitData);
            this.tabNavigationPage1.Controls.Add(this.btnImkbBarData);
            this.tabNavigationPage1.Controls.Add(this.progressBarImkbBarData);
            this.tabNavigationPage1.Controls.Add(this.btnImkbDD);
            this.tabNavigationPage1.Controls.Add(this.btnImkbYuzeyselVeri);
            this.tabNavigationPage1.Controls.Add(this.progressBarImkbYuzeyselVeri);
            this.tabNavigationPage1.Controls.Add(this.progressBarDD);
            this.tabNavigationPage1.Name = "tabNavigationPage1";
            this.tabNavigationPage1.Size = new System.Drawing.Size(445, 319);
            // 
            // btnImkbBarDataGunSonu
            // 
            this.btnImkbBarDataGunSonu.Appearance.Options.UseTextOptions = true;
            this.btnImkbBarDataGunSonu.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.btnImkbBarDataGunSonu.Checked = true;
            this.btnImkbBarDataGunSonu.Location = new System.Drawing.Point(6, 94);
            this.btnImkbBarDataGunSonu.Name = "btnImkbBarDataGunSonu";
            this.btnImkbBarDataGunSonu.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnImkbBarDataGunSonu.Size = new System.Drawing.Size(176, 23);
            this.btnImkbBarDataGunSonu.TabIndex = 85;
            this.btnImkbBarDataGunSonu.TabStop = false;
            this.btnImkbBarDataGunSonu.Text = "IMKB - Bar Data Gun Sonu - 5 DK";
            // 
            // progressBarImkbBarDataGunSonu
            // 
            this.progressBarImkbBarDataGunSonu.Location = new System.Drawing.Point(188, 94);
            this.progressBarImkbBarDataGunSonu.Name = "progressBarImkbBarDataGunSonu";
            this.progressBarImkbBarDataGunSonu.Size = new System.Drawing.Size(252, 23);
            this.progressBarImkbBarDataGunSonu.TabIndex = 84;
            // 
            // btnImkbTaramaSRSI
            // 
            this.btnImkbTaramaSRSI.Appearance.Options.UseTextOptions = true;
            this.btnImkbTaramaSRSI.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.btnImkbTaramaSRSI.Checked = true;
            this.btnImkbTaramaSRSI.Location = new System.Drawing.Point(6, 181);
            this.btnImkbTaramaSRSI.Name = "btnImkbTaramaSRSI";
            this.btnImkbTaramaSRSI.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnImkbTaramaSRSI.Size = new System.Drawing.Size(176, 23);
            this.btnImkbTaramaSRSI.TabIndex = 83;
            this.btnImkbTaramaSRSI.TabStop = false;
            this.btnImkbTaramaSRSI.Text = "IMKB - Tarama SRSI - 60 DK";
            // 
            // progressBarTaramaSRSI
            // 
            this.progressBarTaramaSRSI.Location = new System.Drawing.Point(188, 181);
            this.progressBarTaramaSRSI.Name = "progressBarTaramaSRSI";
            this.progressBarTaramaSRSI.Size = new System.Drawing.Size(252, 23);
            this.progressBarTaramaSRSI.TabIndex = 82;
            // 
            // btnImkbTaramaUyumsuzluk
            // 
            this.btnImkbTaramaUyumsuzluk.Appearance.Options.UseTextOptions = true;
            this.btnImkbTaramaUyumsuzluk.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.btnImkbTaramaUyumsuzluk.Checked = true;
            this.btnImkbTaramaUyumsuzluk.Location = new System.Drawing.Point(6, 152);
            this.btnImkbTaramaUyumsuzluk.Name = "btnImkbTaramaUyumsuzluk";
            this.btnImkbTaramaUyumsuzluk.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnImkbTaramaUyumsuzluk.Size = new System.Drawing.Size(176, 23);
            this.btnImkbTaramaUyumsuzluk.TabIndex = 81;
            this.btnImkbTaramaUyumsuzluk.TabStop = false;
            this.btnImkbTaramaUyumsuzluk.Text = "IMKB - Tarama Uyumsuzluk - 60 DK";
            // 
            // progressBarTaramaUyumsuzluk
            // 
            this.progressBarTaramaUyumsuzluk.Location = new System.Drawing.Point(188, 152);
            this.progressBarTaramaUyumsuzluk.Name = "progressBarTaramaUyumsuzluk";
            this.progressBarTaramaUyumsuzluk.Size = new System.Drawing.Size(252, 23);
            this.progressBarTaramaUyumsuzluk.TabIndex = 80;
            // 
            // btnImkbKompozitData
            // 
            this.btnImkbKompozitData.Appearance.Options.UseTextOptions = true;
            this.btnImkbKompozitData.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.btnImkbKompozitData.Checked = true;
            this.btnImkbKompozitData.Location = new System.Drawing.Point(6, 123);
            this.btnImkbKompozitData.Name = "btnImkbKompozitData";
            this.btnImkbKompozitData.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnImkbKompozitData.Size = new System.Drawing.Size(176, 23);
            this.btnImkbKompozitData.TabIndex = 79;
            this.btnImkbKompozitData.TabStop = false;
            this.btnImkbKompozitData.Text = "IMKB - Kompozit Data - 120 DK";
            // 
            // progressBarKompozitData
            // 
            this.progressBarKompozitData.Location = new System.Drawing.Point(188, 123);
            this.progressBarKompozitData.Name = "progressBarKompozitData";
            this.progressBarKompozitData.Size = new System.Drawing.Size(252, 23);
            this.progressBarKompozitData.TabIndex = 78;
            // 
            // btnImkbBarData
            // 
            this.btnImkbBarData.Appearance.Options.UseTextOptions = true;
            this.btnImkbBarData.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.btnImkbBarData.Checked = true;
            this.btnImkbBarData.Location = new System.Drawing.Point(6, 65);
            this.btnImkbBarData.Name = "btnImkbBarData";
            this.btnImkbBarData.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnImkbBarData.Size = new System.Drawing.Size(176, 23);
            this.btnImkbBarData.TabIndex = 77;
            this.btnImkbBarData.TabStop = false;
            this.btnImkbBarData.Text = "IMKB - Bar Data - 5 DK";
            // 
            // progressBarImkbBarData
            // 
            this.progressBarImkbBarData.Location = new System.Drawing.Point(188, 65);
            this.progressBarImkbBarData.Name = "progressBarImkbBarData";
            this.progressBarImkbBarData.Size = new System.Drawing.Size(252, 23);
            this.progressBarImkbBarData.TabIndex = 76;
            // 
            // btnImkbDD
            // 
            this.btnImkbDD.Appearance.Options.UseTextOptions = true;
            this.btnImkbDD.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.btnImkbDD.Checked = true;
            this.btnImkbDD.Location = new System.Drawing.Point(6, 36);
            this.btnImkbDD.Name = "btnImkbDD";
            this.btnImkbDD.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnImkbDD.Size = new System.Drawing.Size(176, 23);
            this.btnImkbDD.TabIndex = 75;
            this.btnImkbDD.TabStop = false;
            this.btnImkbDD.Text = "IMKB - Gün DD - 120 DK";
            // 
            // btnImkbYuzeyselVeri
            // 
            this.btnImkbYuzeyselVeri.Appearance.Options.UseTextOptions = true;
            this.btnImkbYuzeyselVeri.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.btnImkbYuzeyselVeri.Checked = true;
            this.btnImkbYuzeyselVeri.Location = new System.Drawing.Point(6, 7);
            this.btnImkbYuzeyselVeri.Name = "btnImkbYuzeyselVeri";
            this.btnImkbYuzeyselVeri.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnImkbYuzeyselVeri.Size = new System.Drawing.Size(176, 23);
            this.btnImkbYuzeyselVeri.TabIndex = 74;
            this.btnImkbYuzeyselVeri.TabStop = false;
            this.btnImkbYuzeyselVeri.Text = "IMKB - Yüzeysel - 30 SN";
            // 
            // progressBarImkbYuzeyselVeri
            // 
            this.progressBarImkbYuzeyselVeri.Location = new System.Drawing.Point(188, 7);
            this.progressBarImkbYuzeyselVeri.Name = "progressBarImkbYuzeyselVeri";
            this.progressBarImkbYuzeyselVeri.Size = new System.Drawing.Size(252, 23);
            this.progressBarImkbYuzeyselVeri.TabIndex = 72;
            // 
            // progressBarDD
            // 
            this.progressBarDD.Location = new System.Drawing.Point(188, 35);
            this.progressBarDD.Name = "progressBarDD";
            this.progressBarDD.Size = new System.Drawing.Size(252, 23);
            this.progressBarDD.TabIndex = 73;
            // 
            // tabNavigationPage2
            // 
            this.tabNavigationPage2.Caption = "Tamamlama";
            this.tabNavigationPage2.Controls.Add(this.progressBarImkbBarDataTekSembol);
            this.tabNavigationPage2.Controls.Add(this.btnImkbBarDataTekSembol);
            this.tabNavigationPage2.Controls.Add(this.btnImkbBarDataSembol);
            this.tabNavigationPage2.Controls.Add(this.progressBarImkbBarDataSembol);
            this.tabNavigationPage2.Name = "tabNavigationPage2";
            this.tabNavigationPage2.Size = new System.Drawing.Size(445, 319);
            // 
            // progressBarImkbBarDataTekSembol
            // 
            this.progressBarImkbBarDataTekSembol.Location = new System.Drawing.Point(173, 35);
            this.progressBarImkbBarDataTekSembol.Name = "progressBarImkbBarDataTekSembol";
            this.progressBarImkbBarDataTekSembol.Size = new System.Drawing.Size(267, 23);
            this.progressBarImkbBarDataTekSembol.TabIndex = 73;
            this.progressBarImkbBarDataTekSembol.Visible = false;
            // 
            // btnImkbBarDataTekSembol
            // 
            this.btnImkbBarDataTekSembol.Appearance.Options.UseTextOptions = true;
            this.btnImkbBarDataTekSembol.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.btnImkbBarDataTekSembol.Location = new System.Drawing.Point(5, 35);
            this.btnImkbBarDataTekSembol.Name = "btnImkbBarDataTekSembol";
            this.btnImkbBarDataTekSembol.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnImkbBarDataTekSembol.Size = new System.Drawing.Size(162, 23);
            this.btnImkbBarDataTekSembol.TabIndex = 72;
            this.btnImkbBarDataTekSembol.TabStop = false;
            this.btnImkbBarDataTekSembol.Text = "IMKB - Bar Data Tek Sembol";
            this.btnImkbBarDataTekSembol.Visible = false;
            // 
            // btnImkbBarDataSembol
            // 
            this.btnImkbBarDataSembol.Appearance.Options.UseTextOptions = true;
            this.btnImkbBarDataSembol.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.btnImkbBarDataSembol.Location = new System.Drawing.Point(5, 6);
            this.btnImkbBarDataSembol.Name = "btnImkbBarDataSembol";
            this.btnImkbBarDataSembol.ShowFocusRectangle = DevExpress.Utils.DefaultBoolean.False;
            this.btnImkbBarDataSembol.Size = new System.Drawing.Size(162, 23);
            this.btnImkbBarDataSembol.TabIndex = 71;
            this.btnImkbBarDataSembol.TabStop = false;
            this.btnImkbBarDataSembol.Text = "IMKB - Bar Data Sembol Tarihsel";
            this.btnImkbBarDataSembol.Visible = false;
            // 
            // progressBarImkbBarDataSembol
            // 
            this.progressBarImkbBarDataSembol.Location = new System.Drawing.Point(173, 6);
            this.progressBarImkbBarDataSembol.Name = "progressBarImkbBarDataSembol";
            this.progressBarImkbBarDataSembol.Size = new System.Drawing.Size(267, 23);
            this.progressBarImkbBarDataSembol.TabIndex = 70;
            this.progressBarImkbBarDataSembol.Visible = false;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(473, 554);
            this.Controls.Add(this.tabPaneMain);
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.Text = "Geist Panel User.dll";
            ((System.ComponentModel.ISupportInitialize)(this.tabPaneMain)).EndInit();
            this.tabPaneMain.ResumeLayout(false);
            this.tabPageMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grupControlStart)).EndInit();
            this.grupControlStart.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.checkEditAlgobarDataContext.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSembol.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtEndCalendar.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtEndCalendar.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditGeistContext.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtStartCalendar.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtStartCalendar.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditSeansClosed.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControlProgress)).EndInit();
            this.groupControlProgress.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tabPane1)).EndInit();
            this.tabPane1.ResumeLayout(false);
            this.tabNavigationPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.progressBarImkbBarDataGunSonu.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarTaramaSRSI.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarTaramaUyumsuzluk.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarKompozitData.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarImkbBarData.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarImkbYuzeyselVeri.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarDD.Properties)).EndInit();
            this.tabNavigationPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.progressBarImkbBarDataTekSembol.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarImkbBarDataSembol.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraBars.Navigation.TabPane tabPaneMain;
        private DevExpress.XtraBars.Navigation.TabNavigationPage tabPageMain;
        private DevExpress.XtraEditors.SimpleButton btnStop;
        private DevExpress.XtraEditors.SimpleButton btnStart;
        private System.Windows.Forms.Timer timer1;
        private DevExpress.XtraEditors.GroupControl groupControlProgress;
        private DevExpress.XtraEditors.GroupControl grupControlStart;
        private DevExpress.XtraEditors.CheckEdit checkEditSeansClosed;
        protected internal DevExpress.XtraEditors.DateEdit txtStartCalendar;
        private DevExpress.XtraEditors.CheckEdit checkEditGeistContext;
        protected internal DevExpress.XtraEditors.DateEdit txtEndCalendar;
        protected internal DevExpress.XtraEditors.TextEdit txtSembol;
        private DevExpress.XtraBars.Navigation.TabPane tabPane1;
        private DevExpress.XtraBars.Navigation.TabNavigationPage tabNavigationPage1;
        private DevExpress.XtraEditors.CheckButton btnImkbTaramaUyumsuzluk;
        internal DevExpress.XtraEditors.ProgressBarControl progressBarTaramaUyumsuzluk;
        private DevExpress.XtraEditors.CheckButton btnImkbKompozitData;
        internal DevExpress.XtraEditors.ProgressBarControl progressBarKompozitData;
        private DevExpress.XtraEditors.CheckButton btnImkbBarData;
        internal DevExpress.XtraEditors.ProgressBarControl progressBarImkbBarData;
        private DevExpress.XtraEditors.CheckButton btnImkbDD;
        private DevExpress.XtraEditors.CheckButton btnImkbYuzeyselVeri;
        internal DevExpress.XtraEditors.ProgressBarControl progressBarImkbYuzeyselVeri;
        internal DevExpress.XtraEditors.ProgressBarControl progressBarDD;
        private DevExpress.XtraBars.Navigation.TabNavigationPage tabNavigationPage2;
        internal DevExpress.XtraEditors.ProgressBarControl progressBarImkbBarDataTekSembol;
        private DevExpress.XtraEditors.CheckButton btnImkbBarDataTekSembol;
        private DevExpress.XtraEditors.CheckButton btnImkbBarDataSembol;
        internal DevExpress.XtraEditors.ProgressBarControl progressBarImkbBarDataSembol;
        private DevExpress.XtraEditors.CheckEdit checkEditAlgobarDataContext;
        private DevExpress.XtraEditors.CheckButton btnImkbTaramaSRSI;
        internal DevExpress.XtraEditors.ProgressBarControl progressBarTaramaSRSI;
        private DevExpress.XtraEditors.CheckButton btnImkbBarDataGunSonu;
        internal DevExpress.XtraEditors.ProgressBarControl progressBarImkbBarDataGunSonu;
    }
}