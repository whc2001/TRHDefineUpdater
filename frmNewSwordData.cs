using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TRHDefineUpdater
{
    public partial class frmNewSwordData : Form
    {
        public TRHDefineParser.SwordModel Sword { get; private set; }

        private int GuessSwordEvolutionLevel(int type)
        {
            switch (type)
            {
                case 1:
                    //短刀
                    return 20;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    //短刀
                    //胁差
                    //打刀
                    //太刀
                    //大太刀
                    //枪
                    //薙刀
                    return 25;
                default:
                    return 25;
            }
        }

        private string SwordAreaToString(int type)
        {
            switch (type)
            {
                case 1:
                    return "狭";
                case 2:
                    return "広";
                case 3:
                    return "横";
                case 4:
                    return "縦";
                default:
                    return "狭";
            }
        }


        private int SwordAreaConv(int type)
        {
            switch (type)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    //短刀
                    //胁差
                    //打刀
                    //太刀
                    return 1;
                case 5:
                    //大太刀
                    return 2;
                case 6:
                    //枪
                    return 4;
                case 7:
                    //薙刀
                    return 3;
                default:
                    return 1;
            }
        }

        private int SwordTypeOfficalDefineToTRHDefine(int type)
        {
            switch (type)
            {
                case 1:
                    //短刀
                    return 7;
                case 2:
                    //胁差
                    return 4;
                case 3:
                    //打刀
                    return 5;
                case 4:
                    //太刀
                    return 2;
                case 5:
                    //大太刀
                    return 1;
                case 6:
                    //枪
                    return 6;
                case 7:
                    //薙刀
                    return 3;
                default:
                    //默认太刀
                    return 2;
            }
        }

        private int SwordGenreOfficalDefineToTRHDefine(int genre)
        {
            switch (genre)
            {
                case 1:
                    //安綱（无定义）
                    return 1;
                case 2:
                    //三条
                    return 2;
                case 3:
                    //三池（无定义）
                    return 1;
                case 4:
                    //青江
                    return 12;
                case 5:
                    //粟田口
                    return 9;
                case 6:
                    //古備前
                    return 4;
                case 7:
                    //来
                    return 8;
                case 8:
                    //村正
                    return 7;
                case 9:
                    //貞宗（无定义）
                    return 1;
                case 10:
                    //長船
                    return 11;
                case 11:
                    //左文字
                    return 6;
                case 12:
                    //兼定
                    return 3;
                case 13:
                    //堀川
                    return 5;
                case 14:
                    //虎徹
                    return 10;
                case 15:
                    //虎徹…？（无定义）
                    return 1;
                default:
                    //默认为空
                    return 1;
            }
        }

        public frmNewSwordData()
        {
            InitializeComponent();
        }

        public frmNewSwordData(string[] swordData)
        {
            InitializeComponent();
            numSwordID.Value = int.Parse(swordData[0]);
            txtName.Text = swordData[2];
            trkRarity.Value = int.Parse(swordData[8]);
            cmbType.SelectedIndex = SwordTypeOfficalDefineToTRHDefine(int.Parse(swordData[6])) - 1;
            cmbGenre.SelectedIndex = SwordGenreOfficalDefineToTRHDefine(int.Parse(swordData[5])) - 1;
            trkArea.Value = SwordAreaConv(int.Parse(swordData[6]));
            trkEquip.Value = int.Parse(swordData[32]);
            trkEvolution.Value = GuessSwordEvolutionLevel(int.Parse(swordData[6]));
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            Sword = new TRHDefineParser.SwordModel()
            {
                id = (int)numSwordID.Value,
                name = txtName.Text.Trim(),
                area = trkArea.Value,
                group = cmbGenre.SelectedIndex + 1,
                type = cmbType.SelectedIndex + 1,
                equip = trkEquip.Value,
                rarity = trkRarity.Value,
                upgrade = trkEvolution.Value
            };
            this.DialogResult = DialogResult.OK;
        }

        private void trkRarity_ValueChanged(object sender, EventArgs e)
        {
            lblRarity.Text = trkRarity.Value.ToString();
        }

        private void trkArea_ValueChanged(object sender, EventArgs e)
        {
            lblArea.Text = SwordAreaToString(trkArea.Value);
        }

        private void trkEquip_ValueChanged(object sender, EventArgs e)
        {
            lblEquip.Text = trkEquip.Value.ToString();
        }

        private void trkEvolution_ValueChanged(object sender, EventArgs e)
        {
            lblEvolution.Text = trkEvolution.Value.ToString();
        }

        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == 0xA3)
            {
                return;
            }
            base.WndProc(ref msg);
        }
    }
}
