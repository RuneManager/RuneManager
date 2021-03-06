namespace RuneApp
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.dataMonsterList = new System.Windows.Forms.ListView();
            this.colMonName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colMonGrade = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colMonPriority = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colMonID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colMonType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colMonLevel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripBuildStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripCopyright = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripSpacer = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shrinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.speedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.attackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.healthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defenseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.criticalDamageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.waterAttackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fireAttackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windAttackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lightAttackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkAttackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.userManualHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.monImage = new System.Windows.Forms.PictureBox();
            this.statLevel = new System.Windows.Forms.Label();
            this.statID = new System.Windows.Forms.Label();
            this.statName = new System.Windows.Forms.Label();
            this.ACCTotal = new System.Windows.Forms.Label();
            this.ACCBonus = new System.Windows.Forms.Label();
            this.ACCBase = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.RESTotal = new System.Windows.Forms.Label();
            this.RESBonus = new System.Windows.Forms.Label();
            this.RESBase = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.CDTotal = new System.Windows.Forms.Label();
            this.CDBonus = new System.Windows.Forms.Label();
            this.CDBase = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.CRTotal = new System.Windows.Forms.Label();
            this.CRBonus = new System.Windows.Forms.Label();
            this.CRBase = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.SPDTotal = new System.Windows.Forms.Label();
            this.SPDBonus = new System.Windows.Forms.Label();
            this.SPDBase = new System.Windows.Forms.Label();
            this.DEFTotal = new System.Windows.Forms.Label();
            this.DEFBonus = new System.Windows.Forms.Label();
            this.DEFBase = new System.Windows.Forms.Label();
            this.ATKTotal = new System.Windows.Forms.Label();
            this.ATKBonus = new System.Windows.Forms.Label();
            this.ATKBase = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.HPTotal = new System.Windows.Forms.Label();
            this.HPBonus = new System.Windows.Forms.Label();
            this.HPBase = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.HealthLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.runeDial = new RuneApp.RuneDial();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabMonsters = new System.Windows.Forms.TabPage();
            this.tsMonTab = new System.Windows.Forms.ToolStrip();
            this.tsbIncreasePriority = new System.Windows.Forms.ToolStripButton();
            this.tsbDecreasePriority = new System.Windows.Forms.ToolStripButton();
            this.tsbCreateBuild = new System.Windows.Forms.ToolStripButton();
            this.tsbReloadSave = new System.Windows.Forms.ToolStripButton();
            this.tsbUnequipAll = new System.Windows.Forms.ToolStripButton();
            this.unequipMonsterButton = new System.Windows.Forms.ToolStripButton();
            this.tsBtnLockMon = new System.Windows.Forms.ToolStripButton();
            this.tabRunes = new System.Windows.Forms.TabPage();
            this.tsRuneTab = new System.Windows.Forms.ToolStrip();
            this.RuneTab_UnfilterButton = new System.Windows.Forms.ToolStripButton();
            this.RuneTab_LockButton = new System.Windows.Forms.ToolStripButton();
            this.RuneTab_SaveButton = new System.Windows.Forms.ToolStripButton();
            this.dataRuneList = new System.Windows.Forms.ListView();
            this.runesSet = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesGrade = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesMType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesMValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesLevel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesEff = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesMaxEff = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabBuilds = new System.Windows.Forms.TabPage();
            this.listView4 = new System.Windows.Forms.ListView();
            this.buildID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip5 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.tabCrafts = new System.Windows.Forms.TabPage();
            this.dataCraftList = new System.Windows.Forms.ListView();
            this.chCraftId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chCraftSet = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chCraftAttr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chCraftGrade = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chCraftType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip7 = new System.Windows.Forms.ToolStrip();
            this.loadoutList = new System.Windows.Forms.ListView();
            this.buildNameCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buildIDCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buildMonIDCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buildSwapCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buildPowerCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buildTimeCol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.toolStrip4 = new System.Windows.Forms.ToolStrip();
            this.tsBtnLoadsRemove = new System.Windows.Forms.ToolStripButton();
            this.tsBtnLoadsClear = new System.Windows.Forms.ToolStripButton();
            this.tsBtnLoadsLock = new System.Windows.Forms.ToolStripButton();
            this.tsBtnRuneStats = new System.Windows.Forms.ToolStripButton();
            this.tsBtnLoadsSave = new System.Windows.Forms.ToolStripButton();
            this.tsBtnLoadsLoad = new System.Windows.Forms.ToolStripButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.toolStrip6 = new System.Windows.Forms.ToolStrip();
            this.tsBtnBuildsMoveUp = new System.Windows.Forms.ToolStripButton();
            this.tsBtnBuildsMoveDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsBtnBuildsRemove = new System.Windows.Forms.ToolStripButton();
            this.tsBtnBuildsRunOne = new System.Windows.Forms.ToolStripSplitButton();
            this.allToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resumeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.in30SecondsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.in8HoursToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.in16HoursToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsBtnBuildsSave = new System.Windows.Forms.ToolStripButton();
            this.tsBtnBuildsUnlock = new System.Windows.Forms.ToolStripButton();
            this.tsBtnFindSpeed = new System.Windows.Forms.ToolStripButton();
            this.tsBtnLink = new System.Windows.Forms.ToolStripButton();
            this.tsBtnSkip = new System.Windows.Forms.ToolStripButton();
            this.buildList = new System.Windows.Forms.ListView();
            this.buildCHName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buildCHPriority = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buildCHID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buildCHProgress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buildCHMID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buildCHTeams = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.useRunesCheck = new System.Windows.Forms.CheckBox();
            this.updateBox = new System.Windows.Forms.GroupBox();
            this.updateWhat = new System.Windows.Forms.Button();
            this.updateComplain = new System.Windows.Forms.Label();
            this.updateDownload = new System.Windows.Forms.Button();
            this.updateNew = new System.Windows.Forms.Label();
            this.updateCurrent = new System.Windows.Forms.Label();
            this.menu_buildlist = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.teamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findGoodRunes = new System.Windows.Forms.CheckBox();
            this.fileBox = new System.Windows.Forms.GroupBox();
            this.btnRefreshSave = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cbGoFast = new System.Windows.Forms.CheckBox();
            this.cbFillRunes = new System.Windows.Forms.CheckBox();
            this.runeInventory = new RuneApp.RuneBox();
            this.runeEquipped = new RuneApp.RuneBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnRuneDial = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.monImage)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabMonsters.SuspendLayout();
            this.tsMonTab.SuspendLayout();
            this.tabRunes.SuspendLayout();
            this.tsRuneTab.SuspendLayout();
            this.tabBuilds.SuspendLayout();
            this.toolStrip5.SuspendLayout();
            this.tabCrafts.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.toolStrip4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.toolStrip6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.updateBox.SuspendLayout();
            this.menu_buildlist.SuspendLayout();
            this.fileBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataMonsterList
            // 
            this.dataMonsterList.AllowColumnReorder = true;
            this.dataMonsterList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataMonsterList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colMonName,
            this.colMonGrade,
            this.colMonPriority,
            this.colMonID,
            this.colMonType,
            this.colMonLevel});
            this.dataMonsterList.FullRowSelect = true;
            this.dataMonsterList.HideSelection = false;
            this.dataMonsterList.Location = new System.Drawing.Point(0, 25);
            this.dataMonsterList.Margin = new System.Windows.Forms.Padding(2);
            this.dataMonsterList.MultiSelect = false;
            this.dataMonsterList.Name = "dataMonsterList";
            this.dataMonsterList.Size = new System.Drawing.Size(242, 603);
            this.dataMonsterList.TabIndex = 0;
            this.dataMonsterList.UseCompatibleStateImageBehavior = false;
            this.dataMonsterList.View = System.Windows.Forms.View.Details;
            this.dataMonsterList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView2_ColumnClick);
            this.dataMonsterList.SelectedIndexChanged += new System.EventHandler(this.monstertab_list_select);
            this.dataMonsterList.DoubleClick += new System.EventHandler(this.toolStripButton7_Click);
            // 
            // colMonName
            // 
            this.colMonName.DisplayIndex = 3;
            this.colMonName.Text = "Name";
            this.colMonName.Width = 130;
            // 
            // colMonGrade
            // 
            this.colMonGrade.Text = "★";
            this.colMonGrade.Width = 25;
            // 
            // colMonPriority
            // 
            this.colMonPriority.DisplayIndex = 0;
            this.colMonPriority.Text = "Priority";
            this.colMonPriority.Width = 40;
            // 
            // colMonID
            // 
            this.colMonID.DisplayIndex = 4;
            this.colMonID.Text = "ID";
            this.colMonID.Width = 0;
            // 
            // colMonType
            // 
            this.colMonType.DisplayIndex = 5;
            this.colMonType.Text = "TypeID";
            this.colMonType.Width = 0;
            // 
            // colMonLevel
            // 
            this.colMonLevel.DisplayIndex = 2;
            this.colMonLevel.Text = "Lvl";
            this.colMonLevel.Width = 30;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.toolStripBuildStatus,
            this.toolStripCopyright,
            this.toolStripSpacer});
            this.statusStrip1.Location = new System.Drawing.Point(0, 708);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1350, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(57, 17);
            this.toolStripStatusLabel1.Text = "Locked: 0";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(64, 17);
            this.toolStripStatusLabel2.Text = "Unequip: 0";
            // 
            // toolStripBuildStatus
            // 
            this.toolStripBuildStatus.Name = "toolStripBuildStatus";
            this.toolStripBuildStatus.Size = new System.Drawing.Size(75, 17);
            this.toolStripBuildStatus.Text = "Build Status: ";
            // 
            // toolStripCopyright
            // 
            this.toolStripCopyright.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.toolStripCopyright.Name = "toolStripCopyright";
            this.toolStripCopyright.Size = new System.Drawing.Size(1131, 17);
            this.toolStripCopyright.Spring = true;
            this.toolStripCopyright.Text = "Images belong to Com2Us";
            this.toolStripCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // toolStripSpacer
            // 
            this.toolStripSpacer.Name = "toolStripSpacer";
            this.toolStripSpacer.Size = new System.Drawing.Size(12, 17);
            this.toolStripSpacer.Spring = true;
            this.toolStripSpacer.Text = "_";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1350, 25);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.shrinesToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1350, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadSaveDialogue);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // shrinesToolStripMenuItem
            // 
            this.shrinesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.speedToolStripMenuItem,
            this.attackToolStripMenuItem,
            this.healthToolStripMenuItem,
            this.defenseToolStripMenuItem,
            this.criticalDamageToolStripMenuItem,
            this.waterAttackToolStripMenuItem,
            this.fireAttackToolStripMenuItem,
            this.windAttackToolStripMenuItem,
            this.lightAttackToolStripMenuItem,
            this.darkAttackToolStripMenuItem});
            this.shrinesToolStripMenuItem.Name = "shrinesToolStripMenuItem";
            this.shrinesToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.shrinesToolStripMenuItem.Text = "Shrines";
            // 
            // speedToolStripMenuItem
            // 
            this.speedToolStripMenuItem.Name = "speedToolStripMenuItem";
            this.speedToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.speedToolStripMenuItem.Text = "Speed";
            // 
            // attackToolStripMenuItem
            // 
            this.attackToolStripMenuItem.Name = "attackToolStripMenuItem";
            this.attackToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.attackToolStripMenuItem.Text = "Attack";
            // 
            // healthToolStripMenuItem
            // 
            this.healthToolStripMenuItem.Name = "healthToolStripMenuItem";
            this.healthToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.healthToolStripMenuItem.Text = "Health";
            // 
            // defenseToolStripMenuItem
            // 
            this.defenseToolStripMenuItem.Name = "defenseToolStripMenuItem";
            this.defenseToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.defenseToolStripMenuItem.Text = "Defense";
            // 
            // criticalDamageToolStripMenuItem
            // 
            this.criticalDamageToolStripMenuItem.Name = "criticalDamageToolStripMenuItem";
            this.criticalDamageToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.criticalDamageToolStripMenuItem.Text = "Critical Damage";
            // 
            // waterAttackToolStripMenuItem
            // 
            this.waterAttackToolStripMenuItem.Name = "waterAttackToolStripMenuItem";
            this.waterAttackToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.waterAttackToolStripMenuItem.Text = "Water Attack";
            // 
            // fireAttackToolStripMenuItem
            // 
            this.fireAttackToolStripMenuItem.Name = "fireAttackToolStripMenuItem";
            this.fireAttackToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.fireAttackToolStripMenuItem.Text = "Fire Attack";
            // 
            // windAttackToolStripMenuItem
            // 
            this.windAttackToolStripMenuItem.Name = "windAttackToolStripMenuItem";
            this.windAttackToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.windAttackToolStripMenuItem.Text = "Wind Attack";
            // 
            // lightAttackToolStripMenuItem
            // 
            this.lightAttackToolStripMenuItem.Name = "lightAttackToolStripMenuItem";
            this.lightAttackToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.lightAttackToolStripMenuItem.Text = "Light Attack";
            // 
            // darkAttackToolStripMenuItem
            // 
            this.darkAttackToolStripMenuItem.Name = "darkAttackToolStripMenuItem";
            this.darkAttackToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.darkAttackToolStripMenuItem.Text = "Dark Attack";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkForUpdatesToolStripMenuItem,
            this.aboutToolStripMenuItem1,
            this.userManualHelpToolStripMenuItem});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for Updates";
            this.checkForUpdatesToolStripMenuItem.Click += new System.EventHandler(this.checkForUpdatesToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(176, 22);
            this.aboutToolStripMenuItem1.Text = "About";
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.aboutToolStripMenuItem1_Click);
            // 
            // userManualHelpToolStripMenuItem
            // 
            this.userManualHelpToolStripMenuItem.Name = "userManualHelpToolStripMenuItem";
            this.userManualHelpToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.userManualHelpToolStripMenuItem.Text = "User Manual / Help";
            this.userManualHelpToolStripMenuItem.Click += new System.EventHandler(this.userManualHelpToolStripMenuItem_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnRuneDial);
            this.groupBox1.Controls.Add(this.monImage);
            this.groupBox1.Controls.Add(this.statLevel);
            this.groupBox1.Controls.Add(this.statID);
            this.groupBox1.Controls.Add(this.statName);
            this.groupBox1.Controls.Add(this.ACCTotal);
            this.groupBox1.Controls.Add(this.ACCBonus);
            this.groupBox1.Controls.Add(this.ACCBase);
            this.groupBox1.Controls.Add(this.label32);
            this.groupBox1.Controls.Add(this.RESTotal);
            this.groupBox1.Controls.Add(this.RESBonus);
            this.groupBox1.Controls.Add(this.RESBase);
            this.groupBox1.Controls.Add(this.label28);
            this.groupBox1.Controls.Add(this.CDTotal);
            this.groupBox1.Controls.Add(this.CDBonus);
            this.groupBox1.Controls.Add(this.CDBase);
            this.groupBox1.Controls.Add(this.label24);
            this.groupBox1.Controls.Add(this.CRTotal);
            this.groupBox1.Controls.Add(this.CRBonus);
            this.groupBox1.Controls.Add(this.CRBase);
            this.groupBox1.Controls.Add(this.label20);
            this.groupBox1.Controls.Add(this.SPDTotal);
            this.groupBox1.Controls.Add(this.SPDBonus);
            this.groupBox1.Controls.Add(this.SPDBase);
            this.groupBox1.Controls.Add(this.DEFTotal);
            this.groupBox1.Controls.Add(this.DEFBonus);
            this.groupBox1.Controls.Add(this.DEFBase);
            this.groupBox1.Controls.Add(this.ATKTotal);
            this.groupBox1.Controls.Add(this.ATKBonus);
            this.groupBox1.Controls.Add(this.ATKBase);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.HPTotal);
            this.groupBox1.Controls.Add(this.HPBonus);
            this.groupBox1.Controls.Add(this.HPBase);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.HealthLabel);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.runeDial);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.groupBox1.Location = new System.Drawing.Point(1105, 49);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(245, 659);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Stats";
            // 
            // monImage
            // 
            this.monImage.Image = global::RuneApp.InternalServer.InternalServer.mon_spot;
            this.monImage.Location = new System.Drawing.Point(189, 13);
            this.monImage.Name = "monImage";
            this.monImage.Size = new System.Drawing.Size(50, 50);
            this.monImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.monImage.TabIndex = 52;
            this.monImage.TabStop = false;
            // 
            // statLevel
            // 
            this.statLevel.AutoSize = true;
            this.statLevel.Location = new System.Drawing.Point(44, 46);
            this.statLevel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.statLevel.Name = "statLevel";
            this.statLevel.Size = new System.Drawing.Size(41, 13);
            this.statLevel.TabIndex = 50;
            this.statLevel.Text = "label10";
            this.statLevel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // statID
            // 
            this.statID.AutoSize = true;
            this.statID.Location = new System.Drawing.Point(44, 32);
            this.statID.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.statID.Name = "statID";
            this.statID.Size = new System.Drawing.Size(35, 13);
            this.statID.TabIndex = 49;
            this.statID.Text = "label9";
            this.statID.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // statName
            // 
            this.statName.AutoSize = true;
            this.statName.Location = new System.Drawing.Point(44, 18);
            this.statName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.statName.Name = "statName";
            this.statName.Size = new System.Drawing.Size(35, 13);
            this.statName.TabIndex = 48;
            this.statName.Text = "label1";
            this.statName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ACCTotal
            // 
            this.ACCTotal.AutoSize = true;
            this.ACCTotal.Location = new System.Drawing.Point(146, 185);
            this.ACCTotal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ACCTotal.Name = "ACCTotal";
            this.ACCTotal.Size = new System.Drawing.Size(21, 13);
            this.ACCTotal.TabIndex = 34;
            this.ACCTotal.Text = "1%";
            this.ACCTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ACCBonus
            // 
            this.ACCBonus.AutoSize = true;
            this.ACCBonus.Location = new System.Drawing.Point(100, 185);
            this.ACCBonus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ACCBonus.Name = "ACCBonus";
            this.ACCBonus.Size = new System.Drawing.Size(27, 13);
            this.ACCBonus.TabIndex = 33;
            this.ACCBonus.Text = "+0%";
            // 
            // ACCBase
            // 
            this.ACCBase.AutoSize = true;
            this.ACCBase.Location = new System.Drawing.Point(59, 185);
            this.ACCBase.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ACCBase.Name = "ACCBase";
            this.ACCBase.Size = new System.Drawing.Size(21, 13);
            this.ACCBase.TabIndex = 32;
            this.ACCBase.Text = "0%";
            this.ACCBase.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(5, 185);
            this.label32.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(26, 13);
            this.label32.TabIndex = 31;
            this.label32.Text = "Acc";
            // 
            // RESTotal
            // 
            this.RESTotal.AutoSize = true;
            this.RESTotal.Location = new System.Drawing.Point(146, 171);
            this.RESTotal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.RESTotal.Name = "RESTotal";
            this.RESTotal.Size = new System.Drawing.Size(21, 13);
            this.RESTotal.TabIndex = 30;
            this.RESTotal.Text = "1%";
            this.RESTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // RESBonus
            // 
            this.RESBonus.AutoSize = true;
            this.RESBonus.Location = new System.Drawing.Point(100, 171);
            this.RESBonus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.RESBonus.Name = "RESBonus";
            this.RESBonus.Size = new System.Drawing.Size(27, 13);
            this.RESBonus.TabIndex = 29;
            this.RESBonus.Text = "+0%";
            // 
            // RESBase
            // 
            this.RESBase.AutoSize = true;
            this.RESBase.Location = new System.Drawing.Point(59, 171);
            this.RESBase.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.RESBase.Name = "RESBase";
            this.RESBase.Size = new System.Drawing.Size(21, 13);
            this.RESBase.TabIndex = 28;
            this.RESBase.Text = "0%";
            this.RESBase.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(5, 171);
            this.label28.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(26, 13);
            this.label28.TabIndex = 27;
            this.label28.Text = "Res";
            // 
            // CDTotal
            // 
            this.CDTotal.AutoSize = true;
            this.CDTotal.Location = new System.Drawing.Point(146, 158);
            this.CDTotal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.CDTotal.Name = "CDTotal";
            this.CDTotal.Size = new System.Drawing.Size(21, 13);
            this.CDTotal.TabIndex = 26;
            this.CDTotal.Text = "1%";
            this.CDTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // CDBonus
            // 
            this.CDBonus.AutoSize = true;
            this.CDBonus.Location = new System.Drawing.Point(100, 158);
            this.CDBonus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.CDBonus.Name = "CDBonus";
            this.CDBonus.Size = new System.Drawing.Size(27, 13);
            this.CDBonus.TabIndex = 25;
            this.CDBonus.Text = "+0%";
            // 
            // CDBase
            // 
            this.CDBase.AutoSize = true;
            this.CDBase.Location = new System.Drawing.Point(59, 158);
            this.CDBase.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.CDBase.Name = "CDBase";
            this.CDBase.Size = new System.Drawing.Size(21, 13);
            this.CDBase.TabIndex = 24;
            this.CDBase.Text = "0%";
            this.CDBase.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(5, 158);
            this.label24.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(39, 13);
            this.label24.TabIndex = 23;
            this.label24.Text = "C Dmg";
            // 
            // CRTotal
            // 
            this.CRTotal.AutoSize = true;
            this.CRTotal.Location = new System.Drawing.Point(146, 144);
            this.CRTotal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.CRTotal.Name = "CRTotal";
            this.CRTotal.Size = new System.Drawing.Size(21, 13);
            this.CRTotal.TabIndex = 22;
            this.CRTotal.Text = "1%";
            this.CRTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // CRBonus
            // 
            this.CRBonus.AutoSize = true;
            this.CRBonus.Location = new System.Drawing.Point(100, 144);
            this.CRBonus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.CRBonus.Name = "CRBonus";
            this.CRBonus.Size = new System.Drawing.Size(27, 13);
            this.CRBonus.TabIndex = 21;
            this.CRBonus.Text = "+0%";
            // 
            // CRBase
            // 
            this.CRBase.AutoSize = true;
            this.CRBase.Location = new System.Drawing.Point(59, 144);
            this.CRBase.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.CRBase.Name = "CRBase";
            this.CRBase.Size = new System.Drawing.Size(21, 13);
            this.CRBase.TabIndex = 20;
            this.CRBase.Text = "0%";
            this.CRBase.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(5, 144);
            this.label20.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(40, 13);
            this.label20.TabIndex = 19;
            this.label20.Text = "C Rate";
            // 
            // SPDTotal
            // 
            this.SPDTotal.AutoSize = true;
            this.SPDTotal.Location = new System.Drawing.Point(173, 110);
            this.SPDTotal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SPDTotal.Name = "SPDTotal";
            this.SPDTotal.Size = new System.Drawing.Size(13, 13);
            this.SPDTotal.TabIndex = 18;
            this.SPDTotal.Text = "1";
            this.SPDTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // SPDBonus
            // 
            this.SPDBonus.AutoSize = true;
            this.SPDBonus.Location = new System.Drawing.Point(100, 110);
            this.SPDBonus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SPDBonus.Name = "SPDBonus";
            this.SPDBonus.Size = new System.Drawing.Size(19, 13);
            this.SPDBonus.TabIndex = 17;
            this.SPDBonus.Text = "+0";
            // 
            // SPDBase
            // 
            this.SPDBase.AutoSize = true;
            this.SPDBase.Location = new System.Drawing.Point(59, 110);
            this.SPDBase.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SPDBase.Name = "SPDBase";
            this.SPDBase.Size = new System.Drawing.Size(13, 13);
            this.SPDBase.TabIndex = 16;
            this.SPDBase.Text = "0";
            this.SPDBase.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // DEFTotal
            // 
            this.DEFTotal.AutoSize = true;
            this.DEFTotal.Location = new System.Drawing.Point(173, 97);
            this.DEFTotal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.DEFTotal.Name = "DEFTotal";
            this.DEFTotal.Size = new System.Drawing.Size(13, 13);
            this.DEFTotal.TabIndex = 15;
            this.DEFTotal.Text = "1";
            this.DEFTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // DEFBonus
            // 
            this.DEFBonus.AutoSize = true;
            this.DEFBonus.Location = new System.Drawing.Point(100, 97);
            this.DEFBonus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.DEFBonus.Name = "DEFBonus";
            this.DEFBonus.Size = new System.Drawing.Size(19, 13);
            this.DEFBonus.TabIndex = 14;
            this.DEFBonus.Text = "+0";
            // 
            // DEFBase
            // 
            this.DEFBase.AutoSize = true;
            this.DEFBase.Location = new System.Drawing.Point(59, 97);
            this.DEFBase.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.DEFBase.Name = "DEFBase";
            this.DEFBase.Size = new System.Drawing.Size(13, 13);
            this.DEFBase.TabIndex = 13;
            this.DEFBase.Text = "0";
            this.DEFBase.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ATKTotal
            // 
            this.ATKTotal.AutoSize = true;
            this.ATKTotal.Location = new System.Drawing.Point(173, 83);
            this.ATKTotal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ATKTotal.Name = "ATKTotal";
            this.ATKTotal.Size = new System.Drawing.Size(13, 13);
            this.ATKTotal.TabIndex = 12;
            this.ATKTotal.Text = "1";
            this.ATKTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ATKBonus
            // 
            this.ATKBonus.AutoSize = true;
            this.ATKBonus.Location = new System.Drawing.Point(100, 83);
            this.ATKBonus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ATKBonus.Name = "ATKBonus";
            this.ATKBonus.Size = new System.Drawing.Size(19, 13);
            this.ATKBonus.TabIndex = 11;
            this.ATKBonus.Text = "+0";
            // 
            // ATKBase
            // 
            this.ATKBase.AutoSize = true;
            this.ATKBase.Location = new System.Drawing.Point(59, 83);
            this.ATKBase.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ATKBase.Name = "ATKBase";
            this.ATKBase.Size = new System.Drawing.Size(13, 13);
            this.ATKBase.TabIndex = 10;
            this.ATKBase.Text = "0";
            this.ATKBase.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 46);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Level";
            // 
            // HPTotal
            // 
            this.HPTotal.AutoSize = true;
            this.HPTotal.Location = new System.Drawing.Point(173, 69);
            this.HPTotal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.HPTotal.Name = "HPTotal";
            this.HPTotal.Size = new System.Drawing.Size(13, 13);
            this.HPTotal.TabIndex = 8;
            this.HPTotal.Text = "1";
            this.HPTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // HPBonus
            // 
            this.HPBonus.AutoSize = true;
            this.HPBonus.Location = new System.Drawing.Point(100, 69);
            this.HPBonus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.HPBonus.Name = "HPBonus";
            this.HPBonus.Size = new System.Drawing.Size(19, 13);
            this.HPBonus.TabIndex = 7;
            this.HPBonus.Text = "+0";
            // 
            // HPBase
            // 
            this.HPBase.AutoSize = true;
            this.HPBase.Location = new System.Drawing.Point(59, 69);
            this.HPBase.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.HPBase.Name = "HPBase";
            this.HPBase.Size = new System.Drawing.Size(13, 13);
            this.HPBase.TabIndex = 6;
            this.HPBase.Text = "0";
            this.HPBase.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 110);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "SPD";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 97);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(28, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "DEF";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 83);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(28, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "ATK";
            // 
            // HealthLabel
            // 
            this.HealthLabel.AutoSize = true;
            this.HealthLabel.Location = new System.Drawing.Point(4, 69);
            this.HealthLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.HealthLabel.Name = "HealthLabel";
            this.HealthLabel.Size = new System.Drawing.Size(22, 13);
            this.HealthLabel.TabIndex = 2;
            this.HealthLabel.Text = "HP";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 32);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(18, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "ID";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 18);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Name";
            // 
            // runeDial
            // 
            this.runeDial.Loadout = null;
            this.runeDial.Location = new System.Drawing.Point(8, 201);
            this.runeDial.Name = "runeDial";
            this.runeDial.Size = new System.Drawing.Size(225, 188);
            this.runeDial.TabIndex = 51;
            this.runeDial.RuneClick += new System.EventHandler<RuneApp.RuneClickEventArgs>(this.runeDial_RuneClick);
            this.runeDial.DialDoubleClick += new System.EventHandler(this.runeDial1_DoubleClick);
            this.runeDial.LoadChanged += new System.EventHandler<RuneOptim.Management.Loadout>(this.runeDial1_LoadoutChanged);
            this.runeDial.DoubleClick += new System.EventHandler(this.runeDial1_DoubleClick);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabMonsters);
            this.tabControl1.Controls.Add(this.tabRunes);
            this.tabControl1.Controls.Add(this.tabBuilds);
            this.tabControl1.Controls.Add(this.tabCrafts);
            this.tabControl1.Location = new System.Drawing.Point(2, 2);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(252, 648);
            this.tabControl1.TabIndex = 10;
            // 
            // tabMonsters
            // 
            this.tabMonsters.Controls.Add(this.tsMonTab);
            this.tabMonsters.Controls.Add(this.dataMonsterList);
            this.tabMonsters.Location = new System.Drawing.Point(4, 22);
            this.tabMonsters.Margin = new System.Windows.Forms.Padding(2);
            this.tabMonsters.Name = "tabMonsters";
            this.tabMonsters.Padding = new System.Windows.Forms.Padding(2);
            this.tabMonsters.Size = new System.Drawing.Size(244, 622);
            this.tabMonsters.TabIndex = 0;
            this.tabMonsters.Text = "Monsters";
            this.tabMonsters.UseVisualStyleBackColor = true;
            // 
            // tsMonTab
            // 
            this.tsMonTab.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbIncreasePriority,
            this.tsbDecreasePriority,
            this.tsbCreateBuild,
            this.tsbReloadSave,
            this.tsbUnequipAll,
            this.unequipMonsterButton,
            this.tsBtnLockMon});
            this.tsMonTab.Location = new System.Drawing.Point(2, 2);
            this.tsMonTab.Name = "tsMonTab";
            this.tsMonTab.Size = new System.Drawing.Size(240, 25);
            this.tsMonTab.TabIndex = 1;
            this.tsMonTab.Text = "tsMonTab";
            // 
            // tsbIncreasePriority
            // 
            this.tsbIncreasePriority.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbIncreasePriority.Image = global::RuneApp.App.up;
            this.tsbIncreasePriority.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbIncreasePriority.Name = "tsbIncreasePriority";
            this.tsbIncreasePriority.Size = new System.Drawing.Size(23, 22);
            this.tsbIncreasePriority.Text = "Increase Priority";
            this.tsbIncreasePriority.Click += new System.EventHandler(this.tsbIncreasePriority_Click);
            // 
            // tsbDecreasePriority
            // 
            this.tsbDecreasePriority.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbDecreasePriority.Image = global::RuneApp.App.down;
            this.tsbDecreasePriority.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDecreasePriority.Name = "tsbDecreasePriority";
            this.tsbDecreasePriority.Size = new System.Drawing.Size(23, 22);
            this.tsbDecreasePriority.Text = "Decrease Priority";
            this.tsbDecreasePriority.Click += new System.EventHandler(this.tsbDecreasePriority_Click);
            // 
            // tsbCreateBuild
            // 
            this.tsbCreateBuild.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCreateBuild.Image = global::RuneApp.App.add;
            this.tsbCreateBuild.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCreateBuild.Name = "tsbCreateBuild";
            this.tsbCreateBuild.Size = new System.Drawing.Size(23, 22);
            this.tsbCreateBuild.Text = "Create Build";
            this.tsbCreateBuild.Click += new System.EventHandler(this.toolStripButton7_Click);
            // 
            // tsbReloadSave
            // 
            this.tsbReloadSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbReloadSave.Image = global::RuneApp.App.refresh;
            this.tsbReloadSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbReloadSave.Name = "tsbReloadSave";
            this.tsbReloadSave.Size = new System.Drawing.Size(23, 22);
            this.tsbReloadSave.Text = "Reload Save";
            this.tsbReloadSave.Click += new System.EventHandler(this.tsbReloadSave_Click);
            // 
            // tsbUnequipAll
            // 
            this.tsbUnequipAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbUnequipAll.Image = global::RuneApp.App.broken;
            this.tsbUnequipAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbUnequipAll.Name = "tsbUnequipAll";
            this.tsbUnequipAll.Size = new System.Drawing.Size(23, 22);
            this.tsbUnequipAll.Text = "Unequip All";
            this.tsbUnequipAll.Click += new System.EventHandler(this.tsbUnequipAll_Click);
            // 
            // unequipMonsterButton
            // 
            this.unequipMonsterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.unequipMonsterButton.Image = global::RuneApp.App.fromMon;
            this.unequipMonsterButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.unequipMonsterButton.Name = "unequipMonsterButton";
            this.unequipMonsterButton.Size = new System.Drawing.Size(23, 22);
            this.unequipMonsterButton.Text = "Unequip Selected";
            this.unequipMonsterButton.Click += new System.EventHandler(this.unequipMonsterButton_Click);
            // 
            // tsBtnLockMon
            // 
            this.tsBtnLockMon.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnLockMon.Image = global::RuneApp.App.locked;
            this.tsBtnLockMon.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnLockMon.Name = "tsBtnLockMon";
            this.tsBtnLockMon.Size = new System.Drawing.Size(23, 22);
            this.tsBtnLockMon.Text = "Lock this Monster";
            this.tsBtnLockMon.ToolTipText = "Lock this Monster";
            this.tsBtnLockMon.Click += new System.EventHandler(this.tsBtnLockMon_Click);
            // 
            // tabRunes
            // 
            this.tabRunes.Controls.Add(this.tsRuneTab);
            this.tabRunes.Controls.Add(this.dataRuneList);
            this.tabRunes.Location = new System.Drawing.Point(4, 22);
            this.tabRunes.Margin = new System.Windows.Forms.Padding(2);
            this.tabRunes.Name = "tabRunes";
            this.tabRunes.Padding = new System.Windows.Forms.Padding(2);
            this.tabRunes.Size = new System.Drawing.Size(244, 622);
            this.tabRunes.TabIndex = 1;
            this.tabRunes.Text = "Runes";
            this.tabRunes.UseVisualStyleBackColor = true;
            // 
            // tsRuneTab
            // 
            this.tsRuneTab.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RuneTab_UnfilterButton,
            this.RuneTab_LockButton,
            this.RuneTab_SaveButton});
            this.tsRuneTab.Location = new System.Drawing.Point(2, 2);
            this.tsRuneTab.Name = "tsRuneTab";
            this.tsRuneTab.Size = new System.Drawing.Size(240, 25);
            this.tsRuneTab.TabIndex = 2;
            this.tsRuneTab.Text = "tsRuneTab";
            // 
            // RuneTab_UnfilterButton
            // 
            this.RuneTab_UnfilterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RuneTab_UnfilterButton.Image = global::RuneApp.App.refresh;
            this.RuneTab_UnfilterButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RuneTab_UnfilterButton.Name = "RuneTab_UnfilterButton";
            this.RuneTab_UnfilterButton.Size = new System.Drawing.Size(23, 22);
            this.RuneTab_UnfilterButton.Text = "Reset Filter";
            this.RuneTab_UnfilterButton.Click += new System.EventHandler(this.runetab_clearfilter);
            // 
            // RuneTab_LockButton
            // 
            this.RuneTab_LockButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RuneTab_LockButton.Image = global::RuneApp.App.whole;
            this.RuneTab_LockButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RuneTab_LockButton.Name = "RuneTab_LockButton";
            this.RuneTab_LockButton.Size = new System.Drawing.Size(23, 22);
            this.RuneTab_LockButton.Text = "Toggle Locked";
            this.RuneTab_LockButton.Click += new System.EventHandler(this.runelistSwapLocked);
            // 
            // RuneTab_SaveButton
            // 
            this.RuneTab_SaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RuneTab_SaveButton.Image = global::RuneApp.App.save;
            this.RuneTab_SaveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RuneTab_SaveButton.Name = "RuneTab_SaveButton";
            this.RuneTab_SaveButton.Size = new System.Drawing.Size(23, 22);
            this.RuneTab_SaveButton.Text = "Save save.json";
            this.RuneTab_SaveButton.Click += new System.EventHandler(this.runetab_savebutton_click);
            // 
            // dataRuneList
            // 
            this.dataRuneList.AllowColumnReorder = true;
            this.dataRuneList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataRuneList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.runesSet,
            this.runesID,
            this.runesGrade,
            this.runesMType,
            this.runesMValue,
            this.runesLevel,
            this.runesEff,
            this.runesMaxEff});
            this.dataRuneList.FullRowSelect = true;
            this.dataRuneList.HideSelection = false;
            this.dataRuneList.Location = new System.Drawing.Point(0, 25);
            this.dataRuneList.Margin = new System.Windows.Forms.Padding(2);
            this.dataRuneList.MultiSelect = false;
            this.dataRuneList.Name = "dataRuneList";
            this.dataRuneList.Size = new System.Drawing.Size(245, 601);
            this.dataRuneList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.dataRuneList.TabIndex = 1;
            this.dataRuneList.UseCompatibleStateImageBehavior = false;
            this.dataRuneList.View = System.Windows.Forms.View.Details;
            this.dataRuneList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView2_ColumnClick);
            this.dataRuneList.SelectedIndexChanged += new System.EventHandler(this.runetab_list_select);
            this.dataRuneList.DoubleClick += new System.EventHandler(this.runelistSwapLocked);
            // 
            // runesSet
            // 
            this.runesSet.DisplayIndex = 2;
            this.runesSet.Text = "Set";
            this.runesSet.Width = 55;
            // 
            // runesID
            // 
            this.runesID.DisplayIndex = 6;
            this.runesID.Text = "ID";
            this.runesID.Width = 0;
            // 
            // runesGrade
            // 
            this.runesGrade.DisplayIndex = 0;
            this.runesGrade.Text = "★";
            this.runesGrade.Width = 25;
            // 
            // runesMType
            // 
            this.runesMType.Text = "MainType";
            this.runesMType.Width = 80;
            // 
            // runesMValue
            // 
            this.runesMValue.DisplayIndex = 7;
            this.runesMValue.Text = "MainValue";
            // 
            // runesLevel
            // 
            this.runesLevel.DisplayIndex = 1;
            this.runesLevel.Text = "Level";
            this.runesLevel.Width = 40;
            // 
            // runesEff
            // 
            this.runesEff.DisplayIndex = 4;
            this.runesEff.Text = "Efficiency";
            this.runesEff.Width = 40;
            // 
            // runesMaxEff
            // 
            this.runesMaxEff.DisplayIndex = 5;
            this.runesMaxEff.Text = "Max Eff";
            this.runesMaxEff.Width = 40;
            // 
            // tabBuilds
            // 
            this.tabBuilds.Controls.Add(this.listView4);
            this.tabBuilds.Controls.Add(this.toolStrip5);
            this.tabBuilds.Location = new System.Drawing.Point(4, 22);
            this.tabBuilds.Margin = new System.Windows.Forms.Padding(2);
            this.tabBuilds.Name = "tabBuilds";
            this.tabBuilds.Size = new System.Drawing.Size(244, 622);
            this.tabBuilds.TabIndex = 2;
            this.tabBuilds.Text = "Builds";
            this.tabBuilds.UseVisualStyleBackColor = true;
            // 
            // listView4
            // 
            this.listView4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView4.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.buildID});
            this.listView4.HideSelection = false;
            this.listView4.Location = new System.Drawing.Point(2, 23);
            this.listView4.Margin = new System.Windows.Forms.Padding(2);
            this.listView4.Name = "listView4";
            this.listView4.Size = new System.Drawing.Size(192, 600);
            this.listView4.TabIndex = 1;
            this.listView4.UseCompatibleStateImageBehavior = false;
            this.listView4.View = System.Windows.Forms.View.Details;
            this.listView4.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView2_ColumnClick);
            // 
            // buildID
            // 
            this.buildID.Text = "ID";
            // 
            // toolStrip5
            // 
            this.toolStrip5.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton5});
            this.toolStrip5.Location = new System.Drawing.Point(0, 0);
            this.toolStrip5.Name = "toolStrip5";
            this.toolStrip5.Size = new System.Drawing.Size(244, 25);
            this.toolStrip5.TabIndex = 0;
            this.toolStrip5.Text = "Do Nothing";
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton5.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton5.Image")));
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton5.Text = "Use Build";
            // 
            // tabCrafts
            // 
            this.tabCrafts.Controls.Add(this.dataCraftList);
            this.tabCrafts.Controls.Add(this.toolStrip7);
            this.tabCrafts.Location = new System.Drawing.Point(4, 22);
            this.tabCrafts.Name = "tabCrafts";
            this.tabCrafts.Size = new System.Drawing.Size(244, 622);
            this.tabCrafts.TabIndex = 3;
            this.tabCrafts.Text = "Crafts";
            this.tabCrafts.UseVisualStyleBackColor = true;
            // 
            // dataCraftList
            // 
            this.dataCraftList.AllowColumnReorder = true;
            this.dataCraftList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataCraftList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chCraftId,
            this.chCraftSet,
            this.chCraftAttr,
            this.chCraftGrade,
            this.chCraftType});
            this.dataCraftList.FullRowSelect = true;
            this.dataCraftList.HideSelection = false;
            this.dataCraftList.Location = new System.Drawing.Point(2, 27);
            this.dataCraftList.Margin = new System.Windows.Forms.Padding(2);
            this.dataCraftList.MultiSelect = false;
            this.dataCraftList.Name = "dataCraftList";
            this.dataCraftList.Size = new System.Drawing.Size(240, 593);
            this.dataCraftList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.dataCraftList.TabIndex = 2;
            this.dataCraftList.UseCompatibleStateImageBehavior = false;
            this.dataCraftList.View = System.Windows.Forms.View.Details;
            this.dataCraftList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView2_ColumnClick);
            this.dataCraftList.SelectedIndexChanged += new System.EventHandler(this.crafttab_list_select);
            // 
            // chCraftId
            // 
            this.chCraftId.Text = "Id";
            this.chCraftId.Width = 0;
            // 
            // chCraftSet
            // 
            this.chCraftSet.Text = "Set";
            this.chCraftSet.Width = 55;
            // 
            // chCraftAttr
            // 
            this.chCraftAttr.Text = "Attribute";
            // 
            // chCraftGrade
            // 
            this.chCraftGrade.Text = "Grade";
            // 
            // chCraftType
            // 
            this.chCraftType.Text = "Type";
            // 
            // toolStrip7
            // 
            this.toolStrip7.Location = new System.Drawing.Point(0, 0);
            this.toolStrip7.Name = "toolStrip7";
            this.toolStrip7.Size = new System.Drawing.Size(244, 25);
            this.toolStrip7.TabIndex = 0;
            this.toolStrip7.Text = "toolStrip7";
            // 
            // loadoutList
            // 
            this.loadoutList.AllowColumnReorder = true;
            this.loadoutList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loadoutList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.buildNameCol,
            this.buildIDCol,
            this.buildMonIDCol,
            this.buildSwapCol,
            this.buildPowerCol,
            this.buildTimeCol});
            this.loadoutList.FullRowSelect = true;
            this.loadoutList.HideSelection = false;
            this.loadoutList.Location = new System.Drawing.Point(4, 37);
            this.loadoutList.Margin = new System.Windows.Forms.Padding(2);
            this.loadoutList.Name = "loadoutList";
            this.loadoutList.Size = new System.Drawing.Size(282, 601);
            this.loadoutList.TabIndex = 12;
            this.loadoutList.UseCompatibleStateImageBehavior = false;
            this.loadoutList.View = System.Windows.Forms.View.Details;
            this.loadoutList.SelectedIndexChanged += new System.EventHandler(this.loadoutlist_SelectedIndexChanged);
            // 
            // buildNameCol
            // 
            this.buildNameCol.DisplayIndex = 1;
            this.buildNameCol.Text = "Name";
            this.buildNameCol.Width = 80;
            // 
            // buildIDCol
            // 
            this.buildIDCol.DisplayIndex = 0;
            this.buildIDCol.Text = "ID";
            this.buildIDCol.Width = 10;
            // 
            // buildMonIDCol
            // 
            this.buildMonIDCol.DisplayIndex = 3;
            this.buildMonIDCol.Text = "Mon ID";
            this.buildMonIDCol.Width = 10;
            // 
            // buildSwapCol
            // 
            this.buildSwapCol.DisplayIndex = 2;
            this.buildSwapCol.Text = "Swap";
            this.buildSwapCol.Width = 40;
            // 
            // buildPowerCol
            // 
            this.buildPowerCol.Text = "Up";
            this.buildPowerCol.Width = 40;
            // 
            // buildTimeCol
            // 
            this.buildTimeCol.Text = "Time";
            this.buildTimeCol.Width = 40;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.toolStrip4);
            this.groupBox2.Controls.Add(this.loadoutList);
            this.groupBox2.Location = new System.Drawing.Point(2, 2);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(290, 642);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Loadouts";
            // 
            // toolStrip4
            // 
            this.toolStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsBtnLoadsRemove,
            this.tsBtnLoadsClear,
            this.tsBtnLoadsLock,
            this.tsBtnRuneStats,
            this.tsBtnLoadsSave,
            this.tsBtnLoadsLoad});
            this.toolStrip4.Location = new System.Drawing.Point(2, 15);
            this.toolStrip4.Name = "toolStrip4";
            this.toolStrip4.Size = new System.Drawing.Size(286, 25);
            this.toolStrip4.TabIndex = 13;
            this.toolStrip4.Text = "toolStrip4";
            // 
            // tsBtnLoadsRemove
            // 
            this.tsBtnLoadsRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnLoadsRemove.Image = global::RuneApp.App.subtract;
            this.tsBtnLoadsRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnLoadsRemove.Name = "tsBtnLoadsRemove";
            this.tsBtnLoadsRemove.Size = new System.Drawing.Size(23, 22);
            this.tsBtnLoadsRemove.Text = "Remove Loadout";
            this.tsBtnLoadsRemove.Click += new System.EventHandler(this.tsBtnLoadsRemove_Click);
            // 
            // tsBtnLoadsClear
            // 
            this.tsBtnLoadsClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnLoadsClear.Image = global::RuneApp.App.refresh;
            this.tsBtnLoadsClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnLoadsClear.Name = "tsBtnLoadsClear";
            this.tsBtnLoadsClear.Size = new System.Drawing.Size(23, 22);
            this.tsBtnLoadsClear.Text = "Remove All";
            this.tsBtnLoadsClear.Click += new System.EventHandler(this.tsBtnLoadsClear_Click);
            // 
            // tsBtnLoadsLock
            // 
            this.tsBtnLoadsLock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnLoadsLock.Image = global::RuneApp.App.whole;
            this.tsBtnLoadsLock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnLoadsLock.Name = "tsBtnLoadsLock";
            this.tsBtnLoadsLock.Size = new System.Drawing.Size(23, 22);
            this.tsBtnLoadsLock.Text = "Lock Runes";
            this.tsBtnLoadsLock.Click += new System.EventHandler(this.tsBtnLoadsLock_Click);
            // 
            // tsBtnRuneStats
            // 
            this.tsBtnRuneStats.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnRuneStats.Image = global::RuneApp.App.save;
            this.tsBtnRuneStats.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnRuneStats.Name = "tsBtnRuneStats";
            this.tsBtnRuneStats.Size = new System.Drawing.Size(23, 22);
            this.tsBtnRuneStats.Text = "Write Runes to Xlsx";
            this.tsBtnRuneStats.Click += new System.EventHandler(this.tsBtnRuneStats_Click);
            // 
            // tsBtnLoadsSave
            // 
            this.tsBtnLoadsSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnLoadsSave.Image = global::RuneApp.App.monToBox;
            this.tsBtnLoadsSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnLoadsSave.Name = "tsBtnLoadsSave";
            this.tsBtnLoadsSave.Size = new System.Drawing.Size(23, 22);
            this.tsBtnLoadsSave.Text = "Export Loadouts";
            this.tsBtnLoadsSave.Click += new System.EventHandler(this.tsBtnLoadsSave_Click);
            // 
            // tsBtnLoadsLoad
            // 
            this.tsBtnLoadsLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnLoadsLoad.Image = global::RuneApp.App.boxToMon;
            this.tsBtnLoadsLoad.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnLoadsLoad.Name = "tsBtnLoadsLoad";
            this.tsBtnLoadsLoad.Size = new System.Drawing.Size(23, 22);
            this.tsBtnLoadsLoad.Text = "Import Loadouts";
            this.tsBtnLoadsLoad.Click += new System.EventHandler(this.tsBtnLoadsLoad_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.toolStrip6);
            this.groupBox3.Controls.Add(this.buildList);
            this.groupBox3.Location = new System.Drawing.Point(2, 2);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(299, 642);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Builds";
            // 
            // toolStrip6
            // 
            this.toolStrip6.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsBtnBuildsMoveUp,
            this.tsBtnBuildsMoveDown,
            this.toolStripSeparator2,
            this.tsBtnBuildsRemove,
            this.tsBtnBuildsRunOne,
            this.toolStripSeparator1,
            this.tsBtnBuildsSave,
            this.tsBtnBuildsUnlock,
            this.tsBtnFindSpeed,
            this.tsBtnLink,
            this.tsBtnSkip});
            this.toolStrip6.Location = new System.Drawing.Point(2, 15);
            this.toolStrip6.Name = "toolStrip6";
            this.toolStrip6.Size = new System.Drawing.Size(295, 25);
            this.toolStrip6.TabIndex = 13;
            this.toolStrip6.Text = "toolStrip6";
            // 
            // tsBtnBuildsMoveUp
            // 
            this.tsBtnBuildsMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnBuildsMoveUp.Image = global::RuneApp.App.up;
            this.tsBtnBuildsMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnBuildsMoveUp.Name = "tsBtnBuildsMoveUp";
            this.tsBtnBuildsMoveUp.Size = new System.Drawing.Size(23, 22);
            this.tsBtnBuildsMoveUp.Text = "Move Up";
            this.tsBtnBuildsMoveUp.Click += new System.EventHandler(this.tsBtnBuildsMoveUp_Click);
            // 
            // tsBtnBuildsMoveDown
            // 
            this.tsBtnBuildsMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnBuildsMoveDown.Image = global::RuneApp.App.down;
            this.tsBtnBuildsMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnBuildsMoveDown.Name = "tsBtnBuildsMoveDown";
            this.tsBtnBuildsMoveDown.Size = new System.Drawing.Size(23, 22);
            this.tsBtnBuildsMoveDown.Text = "Move Down";
            this.tsBtnBuildsMoveDown.Click += new System.EventHandler(this.tsBtnBuildsMoveDown_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsBtnBuildsRemove
            // 
            this.tsBtnBuildsRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnBuildsRemove.Image = global::RuneApp.App.subtract;
            this.tsBtnBuildsRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnBuildsRemove.Name = "tsBtnBuildsRemove";
            this.tsBtnBuildsRemove.Size = new System.Drawing.Size(23, 22);
            this.tsBtnBuildsRemove.Text = "Remove Build";
            this.tsBtnBuildsRemove.Click += new System.EventHandler(this.tsBtnBuildsRemove_Click);
            // 
            // tsBtnBuildsRunOne
            // 
            this.tsBtnBuildsRunOne.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnBuildsRunOne.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allToolStripMenuItem,
            this.resumeToolStripMenuItem,
            this.runToToolStripMenuItem});
            this.tsBtnBuildsRunOne.Image = global::RuneApp.App.right;
            this.tsBtnBuildsRunOne.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnBuildsRunOne.Name = "tsBtnBuildsRunOne";
            this.tsBtnBuildsRunOne.Size = new System.Drawing.Size(32, 22);
            this.tsBtnBuildsRunOne.Text = "Run Builds";
            this.tsBtnBuildsRunOne.ButtonClick += new System.EventHandler(this.tsBtnBuildsRunOne_Click);
            // 
            // allToolStripMenuItem
            // 
            this.allToolStripMenuItem.Image = global::RuneApp.App.go;
            this.allToolStripMenuItem.Name = "allToolStripMenuItem";
            this.allToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.allToolStripMenuItem.Text = "All";
            this.allToolStripMenuItem.Click += new System.EventHandler(this.tsBtnBuildsRunAll_Click);
            // 
            // resumeToolStripMenuItem
            // 
            this.resumeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.in30SecondsToolStripMenuItem,
            this.in8HoursToolStripMenuItem,
            this.in16HoursToolStripMenuItem});
            this.resumeToolStripMenuItem.Image = global::RuneApp.App.resume;
            this.resumeToolStripMenuItem.Name = "resumeToolStripMenuItem";
            this.resumeToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.resumeToolStripMenuItem.Text = "Resume";
            this.resumeToolStripMenuItem.Click += new System.EventHandler(this.tsBtnBuildsResume_Click);
            // 
            // in30SecondsToolStripMenuItem
            // 
            this.in30SecondsToolStripMenuItem.Name = "in30SecondsToolStripMenuItem";
            this.in30SecondsToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.in30SecondsToolStripMenuItem.Text = "In 30 seconds...";
            this.in30SecondsToolStripMenuItem.Click += new System.EventHandler(this.In30SecondsToolStripMenuItem_Click);
            // 
            // in8HoursToolStripMenuItem
            // 
            this.in8HoursToolStripMenuItem.Name = "in8HoursToolStripMenuItem";
            this.in8HoursToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.in8HoursToolStripMenuItem.Text = "In 8 hours...";
            this.in8HoursToolStripMenuItem.Click += new System.EventHandler(this.In8HoursToolStripMenuItem_Click);
            // 
            // in16HoursToolStripMenuItem
            // 
            this.in16HoursToolStripMenuItem.Name = "in16HoursToolStripMenuItem";
            this.in16HoursToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.in16HoursToolStripMenuItem.Text = "In 16 hours...";
            this.in16HoursToolStripMenuItem.Click += new System.EventHandler(this.In16HoursToolStripMenuItem_Click);
            // 
            // runToToolStripMenuItem
            // 
            this.runToToolStripMenuItem.Image = global::RuneApp.App.upto;
            this.runToToolStripMenuItem.Name = "runToToolStripMenuItem";
            this.runToToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.runToToolStripMenuItem.Text = "Run to";
            this.runToToolStripMenuItem.Click += new System.EventHandler(this.tsBtnBuildsRunUpTo_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsBtnBuildsSave
            // 
            this.tsBtnBuildsSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnBuildsSave.Image = global::RuneApp.App.save;
            this.tsBtnBuildsSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnBuildsSave.Name = "tsBtnBuildsSave";
            this.tsBtnBuildsSave.Size = new System.Drawing.Size(23, 22);
            this.tsBtnBuildsSave.Text = "Save Builds";
            this.tsBtnBuildsSave.Click += new System.EventHandler(this.tsBtnBuildsSave_Click);
            // 
            // tsBtnBuildsUnlock
            // 
            this.tsBtnBuildsUnlock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnBuildsUnlock.Image = global::RuneApp.App.broken;
            this.tsBtnBuildsUnlock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnBuildsUnlock.Name = "tsBtnBuildsUnlock";
            this.tsBtnBuildsUnlock.Size = new System.Drawing.Size(23, 22);
            this.tsBtnBuildsUnlock.Text = "Unlock All";
            this.tsBtnBuildsUnlock.Click += new System.EventHandler(this.tsBtnBuildsUnlock_Click);
            // 
            // tsBtnFindSpeed
            // 
            this.tsBtnFindSpeed.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnFindSpeed.Image = global::RuneApp.App.runedial;
            this.tsBtnFindSpeed.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnFindSpeed.Name = "tsBtnFindSpeed";
            this.tsBtnFindSpeed.Size = new System.Drawing.Size(23, 22);
            this.tsBtnFindSpeed.Text = "toolStripButton3";
            this.tsBtnFindSpeed.ToolTipText = "Identify large builds";
            this.tsBtnFindSpeed.Click += new System.EventHandler(this.tsBtnFindSpeed_Click);
            // 
            // tsBtnLink
            // 
            this.tsBtnLink.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnLink.Image = global::RuneApp.App.unlockedYes;
            this.tsBtnLink.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnLink.Name = "tsBtnLink";
            this.tsBtnLink.Size = new System.Drawing.Size(23, 22);
            this.tsBtnLink.Text = "Make a linked build";
            this.tsBtnLink.ToolTipText = "Make a linked build";
            this.tsBtnLink.Click += new System.EventHandler(this.tsBtnLink_Click);
            // 
            // tsBtnSkip
            // 
            this.tsBtnSkip.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnSkip.Image = ((System.Drawing.Image)(resources.GetObject("tsBtnSkip.Image")));
            this.tsBtnSkip.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnSkip.Name = "tsBtnSkip";
            this.tsBtnSkip.Size = new System.Drawing.Size(23, 22);
            this.tsBtnSkip.Text = "Skip";
            this.tsBtnSkip.ToolTipText = "Skip";
            this.tsBtnSkip.Click += new System.EventHandler(this.tsBtnSkip_Click);
            // 
            // buildList
            // 
            this.buildList.AllowColumnReorder = true;
            this.buildList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buildList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.buildCHName,
            this.buildCHPriority,
            this.buildCHID,
            this.buildCHProgress,
            this.buildCHMID,
            this.buildCHTeams});
            this.buildList.FullRowSelect = true;
            this.buildList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.buildList.HideSelection = false;
            this.buildList.Location = new System.Drawing.Point(4, 37);
            this.buildList.Margin = new System.Windows.Forms.Padding(2);
            this.buildList.Name = "buildList";
            this.buildList.Size = new System.Drawing.Size(291, 601);
            this.buildList.TabIndex = 12;
            this.buildList.UseCompatibleStateImageBehavior = false;
            this.buildList.View = System.Windows.Forms.View.Details;
            this.buildList.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.buildList_ColumnWidthChanging);
            this.buildList.SelectedIndexChanged += new System.EventHandler(this.buildList_SelectedIndexChanged);
            this.buildList.DoubleClick += new System.EventHandler(this.buildList_DoubleClick);
            this.buildList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.buildList_MouseClick);
            // 
            // buildCHName
            // 
            this.buildCHName.DisplayIndex = 3;
            this.buildCHName.Text = "Name";
            this.buildCHName.Width = 80;
            // 
            // buildCHPriority
            // 
            this.buildCHPriority.DisplayIndex = 0;
            this.buildCHPriority.Text = "Priority";
            this.buildCHPriority.Width = 40;
            // 
            // buildCHID
            // 
            this.buildCHID.DisplayIndex = 1;
            this.buildCHID.Text = "ID";
            this.buildCHID.Width = 0;
            // 
            // buildCHProgress
            // 
            this.buildCHProgress.DisplayIndex = 4;
            this.buildCHProgress.Text = "Progress";
            this.buildCHProgress.Width = 80;
            // 
            // buildCHMID
            // 
            this.buildCHMID.DisplayIndex = 2;
            this.buildCHMID.Text = "MID";
            this.buildCHMID.Width = 0;
            // 
            // buildCHTeams
            // 
            this.buildCHTeams.Text = "Teams";
            this.buildCHTeams.Width = 80;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Size = new System.Drawing.Size(601, 646);
            this.splitContainer1.SplitterDistance = 303;
            this.splitContainer1.TabIndex = 16;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.Location = new System.Drawing.Point(12, 52);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer2.Size = new System.Drawing.Size(867, 652);
            this.splitContainer2.SplitterDistance = 256;
            this.splitContainer2.TabIndex = 17;
            // 
            // useRunesCheck
            // 
            this.useRunesCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.useRunesCheck.AutoSize = true;
            this.useRunesCheck.Location = new System.Drawing.Point(894, 52);
            this.useRunesCheck.Name = "useRunesCheck";
            this.useRunesCheck.Size = new System.Drawing.Size(127, 17);
            this.useRunesCheck.TabIndex = 18;
            this.useRunesCheck.Text = "Use Equipped Runes";
            this.useRunesCheck.UseVisualStyleBackColor = true;
            this.useRunesCheck.CheckedChanged += new System.EventHandler(this.useRunesCheck_CheckedChanged);
            // 
            // updateBox
            // 
            this.updateBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.updateBox.Controls.Add(this.updateWhat);
            this.updateBox.Controls.Add(this.updateComplain);
            this.updateBox.Controls.Add(this.updateDownload);
            this.updateBox.Controls.Add(this.updateNew);
            this.updateBox.Controls.Add(this.updateCurrent);
            this.updateBox.Location = new System.Drawing.Point(885, 592);
            this.updateBox.Name = "updateBox";
            this.updateBox.Size = new System.Drawing.Size(215, 113);
            this.updateBox.TabIndex = 19;
            this.updateBox.TabStop = false;
            this.updateBox.Text = "Version";
            this.updateBox.Visible = false;
            // 
            // updateWhat
            // 
            this.updateWhat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.updateWhat.Location = new System.Drawing.Point(134, 55);
            this.updateWhat.Name = "updateWhat";
            this.updateWhat.Size = new System.Drawing.Size(75, 23);
            this.updateWhat.TabIndex = 23;
            this.updateWhat.Text = "What\'s New";
            this.updateWhat.UseVisualStyleBackColor = true;
            this.updateWhat.Visible = false;
            this.updateWhat.Click += new System.EventHandler(this.updateWhat_Click);
            // 
            // updateComplain
            // 
            this.updateComplain.AutoSize = true;
            this.updateComplain.Location = new System.Drawing.Point(98, 16);
            this.updateComplain.Name = "updateComplain";
            this.updateComplain.Size = new System.Drawing.Size(77, 13);
            this.updateComplain.TabIndex = 22;
            this.updateComplain.Text = "Complaint Text";
            // 
            // updateDownload
            // 
            this.updateDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.updateDownload.Enabled = false;
            this.updateDownload.Location = new System.Drawing.Point(134, 84);
            this.updateDownload.Name = "updateDownload";
            this.updateDownload.Size = new System.Drawing.Size(75, 23);
            this.updateDownload.TabIndex = 21;
            this.updateDownload.Text = "Download";
            this.updateDownload.UseVisualStyleBackColor = true;
            this.updateDownload.Click += new System.EventHandler(this.updateDownload_Click);
            // 
            // updateNew
            // 
            this.updateNew.AutoSize = true;
            this.updateNew.Location = new System.Drawing.Point(6, 48);
            this.updateNew.Name = "updateNew";
            this.updateNew.Size = new System.Drawing.Size(68, 13);
            this.updateNew.TabIndex = 20;
            this.updateNew.Text = "New: 0.0.0.0";
            // 
            // updateCurrent
            // 
            this.updateCurrent.AutoSize = true;
            this.updateCurrent.Location = new System.Drawing.Point(6, 28);
            this.updateCurrent.Name = "updateCurrent";
            this.updateCurrent.Size = new System.Drawing.Size(80, 13);
            this.updateCurrent.TabIndex = 0;
            this.updateCurrent.Text = "Current: 0.0.0.0";
            // 
            // menu_buildlist
            // 
            this.menu_buildlist.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.teamToolStripMenuItem});
            this.menu_buildlist.Name = "menu_buildlist";
            this.menu_buildlist.Size = new System.Drawing.Size(103, 26);
            // 
            // teamToolStripMenuItem
            // 
            this.teamToolStripMenuItem.Name = "teamToolStripMenuItem";
            this.teamToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.teamToolStripMenuItem.Text = "Team";
            // 
            // findGoodRunes
            // 
            this.findGoodRunes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.findGoodRunes.AutoSize = true;
            this.findGoodRunes.Enabled = false;
            this.findGoodRunes.Location = new System.Drawing.Point(894, 75);
            this.findGoodRunes.Name = "findGoodRunes";
            this.findGoodRunes.Size = new System.Drawing.Size(109, 17);
            this.findGoodRunes.TabIndex = 18;
            this.findGoodRunes.Text = "Find Good Runes";
            this.findGoodRunes.UseVisualStyleBackColor = true;
            this.findGoodRunes.CheckedChanged += new System.EventHandler(this.findGoodRunes_CheckedChanged);
            // 
            // fileBox
            // 
            this.fileBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.fileBox.Controls.Add(this.btnRefreshSave);
            this.fileBox.Controls.Add(this.label1);
            this.fileBox.Location = new System.Drawing.Point(885, 505);
            this.fileBox.Name = "fileBox";
            this.fileBox.Size = new System.Drawing.Size(215, 81);
            this.fileBox.TabIndex = 21;
            this.fileBox.TabStop = false;
            this.fileBox.Text = "File Status";
            this.fileBox.Visible = false;
            // 
            // btnRefreshSave
            // 
            this.btnRefreshSave.Location = new System.Drawing.Point(134, 52);
            this.btnRefreshSave.Name = "btnRefreshSave";
            this.btnRefreshSave.Size = new System.Drawing.Size(75, 23);
            this.btnRefreshSave.TabIndex = 1;
            this.btnRefreshSave.Text = "Refresh";
            this.btnRefreshSave.UseVisualStyleBackColor = true;
            this.btnRefreshSave.Click += new System.EventHandler(this.btnRefreshSave_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(193, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "A change has been made to your save.";
            // 
            // cbGoFast
            // 
            this.cbGoFast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbGoFast.AutoSize = true;
            this.cbGoFast.Location = new System.Drawing.Point(894, 99);
            this.cbGoFast.Name = "cbGoFast";
            this.cbGoFast.Size = new System.Drawing.Size(142, 17);
            this.cbGoFast.TabIndex = 22;
            this.cbGoFast.Text = "Go fast and break things";
            this.cbGoFast.UseVisualStyleBackColor = true;
            this.cbGoFast.CheckedChanged += new System.EventHandler(this.cbGoFast_CheckedChanged);
            // 
            // cbFillRunes
            // 
            this.cbFillRunes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbFillRunes.AutoSize = true;
            this.cbFillRunes.Location = new System.Drawing.Point(894, 123);
            this.cbFillRunes.Name = "cbFillRunes";
            this.cbFillRunes.Size = new System.Drawing.Size(62, 17);
            this.cbFillRunes.TabIndex = 23;
            this.cbFillRunes.Text = "Fill Only";
            this.cbFillRunes.UseVisualStyleBackColor = true;
            this.cbFillRunes.CheckedChanged += new System.EventHandler(this.cbFillRunes_CheckedChanged);
            // 
            // runeInventory
            // 
            this.runeInventory.AllowGrind = true;
            this.runeInventory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runeInventory.Location = new System.Drawing.Point(885, 135);
            this.runeInventory.Name = "runeInventory";
            this.runeInventory.RuneId = ((ulong)(0ul));
            this.runeInventory.Size = new System.Drawing.Size(215, 179);
            this.runeInventory.TabIndex = 20;
            this.runeInventory.TabStop = false;
            this.runeInventory.Visible = false;
            this.runeInventory.OnClickHide += new System.EventHandler(this.lbCloseInventory_Click);
            // 
            // runeEquipped
            // 
            this.runeEquipped.AllowGrind = true;
            this.runeEquipped.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runeEquipped.Location = new System.Drawing.Point(885, 320);
            this.runeEquipped.Name = "runeEquipped";
            this.runeEquipped.RuneId = ((ulong)(0ul));
            this.runeEquipped.Size = new System.Drawing.Size(215, 179);
            this.runeEquipped.TabIndex = 20;
            this.runeEquipped.TabStop = false;
            this.runeEquipped.Visible = false;
            this.runeEquipped.OnClickHide += new System.EventHandler(this.lbCloseEquipped_Click);
            // 
            // btnRuneDial
            // 
            this.btnRuneDial.Location = new System.Drawing.Point(206, 338);
            this.btnRuneDial.Name = "btnRuneDial";
            this.btnRuneDial.Size = new System.Drawing.Size(27, 22);
            this.btnRuneDial.TabIndex = 53;
            this.btnRuneDial.Text = ">";
            this.btnRuneDial.UseVisualStyleBackColor = true;
            this.btnRuneDial.Click += new System.EventHandler(this.runeDial1_DoubleClick);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1350, 730);
            this.Controls.Add(this.cbFillRunes);
            this.Controls.Add(this.cbGoFast);
            this.Controls.Add(this.fileBox);
            this.Controls.Add(this.runeInventory);
            this.Controls.Add(this.runeEquipped);
            this.Controls.Add(this.updateBox);
            this.Controls.Add(this.findGoodRunes);
            this.Controls.Add(this.useRunesCheck);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = global::RuneApp.App.Icon;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Rune Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.monImage)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabMonsters.ResumeLayout(false);
            this.tabMonsters.PerformLayout();
            this.tsMonTab.ResumeLayout(false);
            this.tsMonTab.PerformLayout();
            this.tabRunes.ResumeLayout(false);
            this.tabRunes.PerformLayout();
            this.tsRuneTab.ResumeLayout(false);
            this.tsRuneTab.PerformLayout();
            this.tabBuilds.ResumeLayout(false);
            this.tabBuilds.PerformLayout();
            this.toolStrip5.ResumeLayout(false);
            this.toolStrip5.PerformLayout();
            this.tabCrafts.ResumeLayout(false);
            this.tabCrafts.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.toolStrip4.ResumeLayout(false);
            this.toolStrip4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.toolStrip6.ResumeLayout(false);
            this.toolStrip6.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.updateBox.ResumeLayout(false);
            this.updateBox.PerformLayout();
            this.menu_buildlist.ResumeLayout(false);
            this.fileBox.ResumeLayout(false);
            this.fileBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView dataMonsterList;
        private System.Windows.Forms.ColumnHeader colMonName;
        private System.Windows.Forms.ColumnHeader colMonGrade;
        private System.Windows.Forms.ColumnHeader colMonPriority;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label HealthLabel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label HPBase;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label HPTotal;
        private System.Windows.Forms.Label HPBonus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label ACCTotal;
        private System.Windows.Forms.Label ACCBonus;
        private System.Windows.Forms.Label ACCBase;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label RESTotal;
        private System.Windows.Forms.Label RESBonus;
        private System.Windows.Forms.Label RESBase;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label CDTotal;
        private System.Windows.Forms.Label CDBonus;
        private System.Windows.Forms.Label CDBase;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label CRTotal;
        private System.Windows.Forms.Label CRBonus;
        private System.Windows.Forms.Label CRBase;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label SPDTotal;
        private System.Windows.Forms.Label SPDBonus;
        private System.Windows.Forms.Label SPDBase;
        private System.Windows.Forms.Label DEFTotal;
        private System.Windows.Forms.Label DEFBonus;
        private System.Windows.Forms.Label DEFBase;
        private System.Windows.Forms.Label ATKTotal;
        private System.Windows.Forms.Label ATKBonus;
        private System.Windows.Forms.Label ATKBase;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabMonsters;
        private System.Windows.Forms.TabPage tabRunes;
        private System.Windows.Forms.ListView dataRuneList;
        private System.Windows.Forms.ColumnHeader runesSet;
        private System.Windows.Forms.ColumnHeader runesID;
        private System.Windows.Forms.ColumnHeader runesGrade;
        private System.Windows.Forms.ColumnHeader runesMType;
        private System.Windows.Forms.ColumnHeader runesMValue;
        private System.Windows.Forms.ToolStrip tsMonTab;
        private System.Windows.Forms.ToolStripButton tsbIncreasePriority;
        private System.Windows.Forms.ToolStripButton tsbDecreasePriority;
        private System.Windows.Forms.ToolStrip tsRuneTab;
        private System.Windows.Forms.ToolStripButton RuneTab_UnfilterButton;
        private System.Windows.Forms.ListView loadoutList;
        private System.Windows.Forms.TabPage tabBuilds;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ToolStrip toolStrip4;
        private System.Windows.Forms.ToolStripButton tsBtnLoadsClear;
        private System.Windows.Forms.ToolStrip toolStrip5;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ListView listView4;
        private System.Windows.Forms.ColumnHeader buildID;
        private System.Windows.Forms.Label statLevel;
        private System.Windows.Forms.Label statID;
        private System.Windows.Forms.Label statName;
        private System.Windows.Forms.ColumnHeader buildNameCol;
        private System.Windows.Forms.ColumnHeader buildIDCol;
        private System.Windows.Forms.ToolStripButton tsbCreateBuild;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ToolStrip toolStrip6;
        private System.Windows.Forms.ToolStripButton tsBtnBuildsMoveUp;
        private System.Windows.Forms.ListView buildList;
        private System.Windows.Forms.ColumnHeader buildCHName;
        private System.Windows.Forms.ColumnHeader buildCHID;
        private System.Windows.Forms.ColumnHeader buildCHPriority;
        private System.Windows.Forms.ColumnHeader buildCHProgress;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton tsBtnBuildsSave;
        private System.Windows.Forms.ToolStripButton tsBtnBuildsRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsBtnBuildsUnlock;
        private System.Windows.Forms.ToolStripButton tsBtnLoadsRemove;
        private System.Windows.Forms.ToolStripButton tsbReloadSave;
        private System.Windows.Forms.ToolStripButton tsbUnequipAll;
        private System.Windows.Forms.ToolStripButton tsBtnBuildsMoveDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.CheckBox useRunesCheck;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.GroupBox updateBox;
        private System.Windows.Forms.Button updateDownload;
        private System.Windows.Forms.Label updateNew;
        private System.Windows.Forms.Label updateCurrent;
        private System.Windows.Forms.Label updateComplain;
        private System.Windows.Forms.Button updateWhat;
        private System.Windows.Forms.ColumnHeader buildSwapCol;
        private System.Windows.Forms.ColumnHeader buildMonIDCol;
        private System.Windows.Forms.ColumnHeader buildPowerCol;
        private System.Windows.Forms.ColumnHeader buildTimeCol;
        private System.Windows.Forms.ToolStripMenuItem shrinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem speedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem userManualHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripButton RuneTab_LockButton;
        private System.Windows.Forms.ToolStripButton RuneTab_SaveButton;
        private System.Windows.Forms.ToolStripButton unequipMonsterButton;
        private System.Windows.Forms.ToolStripButton tsBtnLoadsLock;
        private System.Windows.Forms.ColumnHeader buildCHMID;
        private System.Windows.Forms.ToolStripMenuItem attackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem healthToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defenseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem criticalDamageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem waterAttackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fireAttackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windAttackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lightAttackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem darkAttackToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton tsBtnRuneStats;
        private System.Windows.Forms.ContextMenuStrip menu_buildlist;
        private System.Windows.Forms.ToolStripMenuItem teamToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader buildCHTeams;
        private System.Windows.Forms.ToolStripButton tsBtnLoadsSave;
        private System.Windows.Forms.ToolStripButton tsBtnLoadsLoad;
        private RuneBox runeEquipped;
        private RuneBox runeInventory;
        private System.Windows.Forms.CheckBox findGoodRunes;
        private System.Windows.Forms.ToolStripButton tsBtnFindSpeed;
        private RuneDial runeDial;
        private System.Windows.Forms.GroupBox fileBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRefreshSave;
        private System.Windows.Forms.ColumnHeader colMonID;
        private System.Windows.Forms.TabPage tabCrafts;
        private System.Windows.Forms.ListView dataCraftList;
        private System.Windows.Forms.ColumnHeader chCraftSet;
        private System.Windows.Forms.ToolStrip toolStrip7;
        private System.Windows.Forms.ColumnHeader chCraftAttr;
        private System.Windows.Forms.ColumnHeader chCraftGrade;
        private System.Windows.Forms.ColumnHeader chCraftType;
        private System.Windows.Forms.ColumnHeader chCraftId;
        private System.Windows.Forms.ToolStripStatusLabel toolStripCopyright;
        private System.Windows.Forms.ToolStripStatusLabel toolStripSpacer;
        private System.Windows.Forms.ColumnHeader runesLevel;
        private System.Windows.Forms.ColumnHeader colMonType;
        private System.Windows.Forms.ColumnHeader colMonLevel;
        private System.Windows.Forms.PictureBox monImage;
        private System.Windows.Forms.ToolStripButton tsBtnLockMon;
        private System.Windows.Forms.ToolStripSplitButton tsBtnBuildsRunOne;
        private System.Windows.Forms.ToolStripMenuItem resumeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runToToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton tsBtnLink;
        private System.Windows.Forms.CheckBox cbGoFast;
        private System.Windows.Forms.CheckBox cbFillRunes;
        private System.Windows.Forms.ToolStripStatusLabel toolStripBuildStatus;
        private System.Windows.Forms.ToolStripButton tsBtnSkip;
        private System.Windows.Forms.ColumnHeader runesEff;
        private System.Windows.Forms.ColumnHeader runesMaxEff;
        private System.Windows.Forms.ToolStripMenuItem in8HoursToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem in16HoursToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem in30SecondsToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnRuneDial;
    }
}

