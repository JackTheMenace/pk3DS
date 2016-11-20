﻿using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Forms;
using pk3DS.Properties;

namespace pk3DS
{
    public partial class PersonalEditor7 : Form
    {
        public PersonalEditor7()
        {
            InitializeComponent();
            helditem_boxes = new[] { CB_HeldItem1, CB_HeldItem2, CB_HeldItem3 };
            ability_boxes = new[] { CB_Ability1, CB_Ability2, CB_Ability3 };
            typing_boxes = new[] { CB_Type1, CB_Type2 };
            eggGroup_boxes = new[] { CB_EggGroup1, CB_EggGroup2 };
            byte_boxes = new[] { TB_BaseHP, TB_BaseATK, TB_BaseDEF, TB_BaseSPA, TB_BaseSPD, TB_BaseSPE, TB_Gender, TB_HatchCycles, TB_Friendship, TB_CatchRate };
            ev_boxes = new[] { TB_HPEVs, TB_ATKEVs, TB_DEFEVs, TB_SPEEVs, TB_SPAEVs, TB_SPDEVs };
            rstat_boxes = new[] { CHK_rHP, CHK_rATK, CHK_rDEF, CHK_rSPA, CHK_rSPD, CHK_rSPE };

            data = File.ReadAllBytes(paths.Last()); // Load last to data.
            Personal = new PersonalTable(data, GameVersion.SM);
            species[0] = "---";
            abilities[0] = items[0] = moves[0] = "";
            AltForms = Personal.getFormList(species, Main.Config.MaxSpeciesID);
            entryNames = Personal.getPersonalEntryList(AltForms, species, Main.Config.MaxSpeciesID, out baseForms, out formVal);

            Setup();
            CB_Species.SelectedIndex = 1;
        }
        #region Global Variables
        private readonly string[] paths = Directory.GetFiles("personal", "*.*", SearchOption.TopDirectoryOnly);

        private readonly string[] items = Main.getText(TextName.ItemNames);
        private readonly string[] moves = Main.getText(TextName.MoveNames);
        private readonly string[] species = Main.getText(TextName.SpeciesNames);
        private readonly string[] abilities = Main.getText(TextName.AbilityNames);
        private readonly string[] forms = Main.getText(TextName.Forms);
        private readonly string[] types = Main.getText(TextName.Types);

        private readonly byte[] data;
        private readonly string[] entryNames;

        private readonly ComboBox[] helditem_boxes;
        private readonly ComboBox[] ability_boxes;
        private readonly ComboBox[] typing_boxes;
        private readonly ComboBox[] eggGroup_boxes;

        private readonly MaskedTextBox[] byte_boxes;
        private readonly MaskedTextBox[] ev_boxes;
        private readonly CheckBox[] rstat_boxes;

        private readonly string[] eggGroups = { "---", "Monster", "Water 1", "Bug", "Flying", "Field", "Fairy", "Grass", "Human-Like", "Water 3", "Mineral", "Amorphous", "Water 2", "Ditto", "Dragon", "Undiscovered" };
        private readonly string[] EXPGroups = { "Medium-Fast", "Erratic", "Fluctuating", "Medium-Slow", "Fast", "Slow" };
        private readonly string[] colors = { "Red", "Blue", "Yellow", "Green", "Black", "Brown", "Purple", "Gray", "White", "Pink" };
        private readonly ushort[] tutormoves = { 338, 307, 308, 520, 519, 518, 434, 620 };

        private readonly PersonalTable Personal;
        private string[][] AltForms;
        private int[] baseForms, formVal;
        int entry = -1;
        #endregion
        private void Setup()
        {
            ushort[] TMs = new ushort[0];
            TMEditor7.getTMHMList(ref TMs);
            CLB_TMHM.Items.Clear();

            if (TMs.Length == 0) // No ExeFS to grab TMs from.
            {
                for (int i = 1; i <= 100; i++)
                    CLB_TMHM.Items.Add("TM" + i.ToString("00"));
            }
            else // Use TMHM moves.
            {
                for (int i = 1; i <= 100; i++)
                    CLB_TMHM.Items.Add($"TM{i.ToString("00")} {moves[TMs[i - 1]]}");
            }
            foreach (ushort m in tutormoves)
                CLB_MoveTutors.Items.Add(moves[m]);

            for (int i = 0; i < entryNames.Length; i++)
                CB_Species.Items.Add($"{entryNames[i]} - {i.ToString("000")}");

            foreach (ComboBox cb in helditem_boxes)
                foreach (string it in items)
                    cb.Items.Add(it);

            foreach (ComboBox cb in ability_boxes)
                foreach (string ab in abilities)
                    cb.Items.Add(ab);

            foreach (ComboBox cb in typing_boxes)
                foreach (string ty in types)
                    cb.Items.Add(ty);

            foreach (ComboBox cb in eggGroup_boxes)
                foreach (string eg in eggGroups)
                    cb.Items.Add(eg);

            foreach (string co in colors)
                CB_Color.Items.Add(co);

            foreach (string eg in EXPGroups)
                CB_EXPGroup.Items.Add(eg);
        }

        private void CB_Species_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (entry > -1 && !dumping) saveEntry();
            entry = CB_Species.SelectedIndex;
            readEntry();
        }
        private void ByteLimiter(object sender, EventArgs e)
        {
            MaskedTextBox mtb = sender as MaskedTextBox;
            int val;
            int.TryParse(mtb.Text, out val);
            if (Array.IndexOf(byte_boxes, mtb) > -1 && val > 255)
                mtb.Text = "255";
            else if (Array.IndexOf(ev_boxes, mtb) > -1 && val > 3)
                mtb.Text = "3";
        }

        private PersonalInfo pkm;
        private void readInfo()
        {
            pkm = Main.SpeciesStat[entry];

            TB_BaseHP.Text = pkm.HP.ToString("000");
            TB_BaseATK.Text = pkm.ATK.ToString("000");
            TB_BaseDEF.Text = pkm.DEF.ToString("000");
            TB_BaseSPE.Text = pkm.SPE.ToString("000");
            TB_BaseSPA.Text = pkm.SPA.ToString("000");
            TB_BaseSPD.Text = pkm.SPD.ToString("000");
            TB_HPEVs.Text = pkm.EV_HP.ToString("0");
            TB_ATKEVs.Text = pkm.EV_ATK.ToString("0");
            TB_DEFEVs.Text = pkm.EV_DEF.ToString("0");
            TB_SPEEVs.Text = pkm.EV_SPE.ToString("0");
            TB_SPAEVs.Text = pkm.EV_SPA.ToString("0");
            TB_SPDEVs.Text = pkm.EV_SPD.ToString("0");
            
            CB_Type1.SelectedIndex = pkm.Types[0];
            CB_Type2.SelectedIndex = pkm.Types[1];

            TB_CatchRate.Text = pkm.CatchRate.ToString("000");
            TB_Stage.Text = pkm.EvoStage.ToString("0");

            CB_HeldItem1.SelectedIndex = pkm.Items[0];
            CB_HeldItem2.SelectedIndex = pkm.Items[1];
            CB_HeldItem3.SelectedIndex = pkm.Items[2];

            TB_Gender.Text = pkm.Gender.ToString("000");
            TB_HatchCycles.Text = pkm.HatchCycles.ToString("000");
            TB_Friendship.Text = pkm.BaseFriendship.ToString("000");

            CB_EXPGroup.SelectedIndex = pkm.EXPGrowth;

            CB_EggGroup1.SelectedIndex = pkm.EggGroups[0];
            CB_EggGroup2.SelectedIndex = pkm.EggGroups[1];

            CB_Ability1.SelectedIndex = pkm.Abilities[0];
            CB_Ability2.SelectedIndex = pkm.Abilities[1];
            CB_Ability3.SelectedIndex = pkm.Abilities[2];

            TB_FormeCount.Text = pkm.FormeCount.ToString("000");
            TB_FormeSprite.Text = pkm.FormeSprite.ToString("000");

            TB_RawColor.Text = pkm.Color.ToString("000");
            CB_Color.SelectedIndex = pkm.Color & 0xF;

            TB_BaseExp.Text = pkm.BaseEXP.ToString("000");

            TB_Height.Text = ((decimal)pkm.Height / 100).ToString("00.00");
            TB_Weight.Text = ((decimal)pkm.Weight / 10).ToString("000.0");

            for (int i = 0; i < CLB_TMHM.Items.Count; i++)
                CLB_TMHM.SetItemChecked(i, pkm.TMHM[i]); // Bitflags for TMHM

            for (int i = 0; i < CLB_MoveTutors.Items.Count; i++)
                CLB_MoveTutors.SetItemChecked(i, pkm.TypeTutors[i]); // Bitflags for Tutors
        }
        private void readEntry()
        {
            readInfo();
            
            if (dumping) return;
            int s = baseForms[entry];
            int f = formVal[entry];
            if (entry <= Main.Config.MaxSpeciesID)
                s = entry;
            int[] specForm = {s, f};
            string filename = "_" + specForm[0] + (CB_Species.SelectedIndex > Main.Config.MaxSpeciesID ? "_" + (specForm[1] + 1) : "");
            Bitmap rawImg = (Bitmap)Resources.ResourceManager.GetObject(filename) ?? Resources.unknown;
            Bitmap bigImg = new Bitmap(rawImg.Width * 2, rawImg.Height * 2);
            for (int x = 0; x < rawImg.Width; x++)
            {
                for (int y = 0; y < rawImg.Height; y++)
                {
                    Color c = rawImg.GetPixel(x, y);
                    bigImg.SetPixel(2 * x, 2 * y, c);
                    bigImg.SetPixel(2 * x + 1, 2 * y, c);
                    bigImg.SetPixel(2 * x, 2 * y + 1, c);
                    bigImg.SetPixel(2 * x + 1, 2 * y + 1, c);
                }
            }
            PB_MonSprite.Image = bigImg;
        }
        private void savePersonal()
        {
            pkm.HP = Convert.ToByte(TB_BaseHP.Text);
            pkm.ATK = Convert.ToByte(TB_BaseATK.Text);
            pkm.DEF = Convert.ToByte(TB_BaseDEF.Text);
            pkm.SPE = Convert.ToByte(TB_BaseSPE.Text);
            pkm.SPA = Convert.ToByte(TB_BaseSPA.Text);
            pkm.SPD = Convert.ToByte(TB_BaseSPD.Text);

            pkm.EV_HP = Convert.ToByte(TB_HPEVs.Text);
            pkm.EV_ATK = Convert.ToByte(TB_ATKEVs.Text);
            pkm.EV_DEF = Convert.ToByte(TB_DEFEVs.Text);
            pkm.EV_SPE = Convert.ToByte(TB_SPEEVs.Text);
            pkm.EV_SPA = Convert.ToByte(TB_SPAEVs.Text);
            pkm.EV_SPD = Convert.ToByte(TB_SPDEVs.Text);

            pkm.CatchRate = Convert.ToByte(TB_CatchRate.Text);
            pkm.EvoStage = Convert.ToByte(TB_Stage.Text);

            pkm.Types[0] = (byte) CB_Type1.SelectedIndex;
            pkm.Types[1] = (byte) CB_Type2.SelectedIndex;
            pkm.Items[0] = (ushort) CB_HeldItem1.SelectedIndex;
            pkm.Items[1] = (ushort) CB_HeldItem2.SelectedIndex;
            pkm.Items[2] = (ushort) CB_HeldItem3.SelectedIndex;

            pkm.Gender = Convert.ToByte(TB_Gender.Text);
            pkm.HatchCycles = Convert.ToByte(TB_HatchCycles.Text);
            pkm.BaseFriendship = Convert.ToByte(TB_Friendship.Text);
            pkm.EXPGrowth = (byte) CB_EXPGroup.SelectedIndex;
            pkm.EggGroups[0] = (byte) CB_EggGroup1.SelectedIndex;
            pkm.EggGroups[1] = (byte) CB_EggGroup2.SelectedIndex;

            pkm.Abilities[0] = (byte) CB_Ability1.SelectedIndex;
            pkm.Abilities[1] = (byte) CB_Ability2.SelectedIndex;
            pkm.Abilities[2] = (byte) CB_Ability3.SelectedIndex;

            pkm.FormeSprite = Convert.ToUInt16(TB_FormeSprite.Text);
            pkm.FormeCount = Convert.ToByte(TB_FormeCount.Text);
            pkm.Color = (byte) (Convert.ToByte(CB_Color.SelectedIndex) | (Convert.ToByte(TB_RawColor.Text) & 0xF0));
            pkm.BaseEXP = Convert.ToUInt16(TB_BaseExp.Text);

            pkm.Height = (int)Convert.ToDouble(TB_Height.Text)*100;
            pkm.Weight = (int)Convert.ToDouble(TB_Weight.Text)*10;

            for (int i = 0; i < CLB_TMHM.Items.Count; i++)
                pkm.TMHM[i] = CLB_TMHM.GetItemChecked(i);

            for (int t = 0; t < CLB_MoveTutors.Items.Count; t++)
                pkm.TypeTutors[t] = CLB_MoveTutors.GetItemChecked(t);
        }
        private void saveEntry()
        {
            savePersonal();
            byte[] edits = pkm.Write();
            File.WriteAllBytes(paths[entry], edits);
            Array.Copy(edits, 0, data, entry * edits.Length, edits.Length);
            File.WriteAllBytes(paths[paths.Length - 1], data);
        }

        private void B_Randomize_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            const int TMPercent = 35; // Average Learnable TMs is 35.260.
            const int TutorPercent = 2; //136 special tutor moves learnable by species in Untouched ORAS.
            ushort[] itemlist = Main.Config.ORAS ? Legal.Pouch_Items_ORAS : Legal.Pouch_Items_XY;
            ushort[] berrylist = Legal.Pouch_Berry_XY;
            Array.Resize(ref itemlist, itemlist.Length + berrylist.Length);
            Array.Copy(berrylist, 0, itemlist, itemlist.Length - berrylist.Length, berrylist.Length);

            int itemlen = itemlist.Length;
            int abillen = CB_Ability1.Items.Count;
            int typelen = CB_Type1.Items.Count;

            for (int i = 1; i < CB_Species.Items.Count; i++)
            {
                CB_Species.SelectedIndex = i; // Get new Species

                // Fiddle with TM Learnsets
                if (CHK_TM.Checked)
                    for (int t = 0; t < 100; t++)
                        CLB_TMHM.SetItemCheckState(t, rnd.Next(0, 100) < TMPercent ? CheckState.Checked : CheckState.Unchecked);
                if (CHK_HM.Checked)
                    for (int t = 100; t < CLB_TMHM.Items.Count;t++)
                        CLB_TMHM.SetItemCheckState(t, rnd.Next(0, 100) < TMPercent ? CheckState.Checked : CheckState.Unchecked);
                if (CHK_Tutors.Checked)
                {
                    for (int t = 0; t < CLB_MoveTutors.Items.Count; t++)
                        CLB_MoveTutors.SetItemCheckState(t, rnd.Next(0, 100) < TutorPercent ? CheckState.Checked : CheckState.Unchecked);
                    if (Main.Config.ORAS && (CB_Species.SelectedIndex == 384 || CB_Species.SelectedIndex == 814)) //Make sure Rayquaza can learn Dragon Ascent.
                        CLB_MoveTutors.SetItemCheckState(CLB_MoveTutors.Items.Count-1, CheckState.Checked); 
                }

                // Abilities:
                if (CHK_Ability.Checked)
                {
                    ComboBox[] abils = {CB_Ability1, CB_Ability2, CB_Ability3};
                    for (int a = 0; a < 3; a++) // Set 3 New Abilities, none being Wonder Guard (25) unless CHK_WGuard is checked.
                    {
                        int newabil = rnd.Next(1, abillen);
                        while (newabil == 25 && !CHK_WGuard.Checked)
                            newabil = rnd.Next(1, abillen);
                        if (abils[a].SelectedIndex != 25 || CHK_WGuard.Checked) abils[a].SelectedIndex = newabil;
                    }
                }

                // Fiddle with Base Stats, don't muck with Shedinja.
                if (CHK_Stats.Checked)
                {
                    if (Convert.ToByte(byte_boxes[0].Text) != 1)
                        for (int z = 0; z < 6; z++)
                            if (rstat_boxes[z].Checked)
                                byte_boxes[z].Text =
                                    Math.Max(5,rnd.Next(
                                            Math.Min(255, (int) (Convert.ToByte(byte_boxes[z].Text)*(1 - NUD_StatDev.Value/100))),
                                            Math.Min(255, (int) (Convert.ToByte(byte_boxes[z].Text)*(1 + NUD_StatDev.Value/100)))
                                            )).ToString("000");
                }
                // EV yield stays the same...

                if (CHK_CatchRate.Checked)
                    TB_CatchRate.Text = rnd.Next(3, 251).ToString("000"); //Random Catch Rate between 3 and 250. Should I make this normally distributed?
                if (CHK_EggGroup.Checked)
                {
                    if (rnd.Next(0, 100) < NUD_Egg.Value) // 50% chance to have either One or Two Egg Groups
                        CB_EggGroup1.SelectedIndex = CB_EggGroup2.SelectedIndex = rnd.Next(1, CB_EggGroup1.Items.Count);
                    else
                    {
                        CB_EggGroup1.SelectedIndex = rnd.Next(1, CB_EggGroup1.Items.Count);
                        CB_EggGroup2.SelectedIndex = rnd.Next(1, CB_EggGroup1.Items.Count);
                    }
                }

                // Items
                if (CHK_Item.Checked)
                {
                    CB_HeldItem1.SelectedIndex = itemlist[rnd.Next(1, itemlen)];
                    CB_HeldItem2.SelectedIndex = itemlist[rnd.Next(1, itemlen)];
                    CB_HeldItem3.SelectedIndex = itemlist[rnd.Next(1, itemlen)];
                }

                // Type
                if (CHK_Type.Checked)
                {
                    if (rnd.Next(0, 100) < NUD_TypePercent.Value) // 50% chance to have either Single or Dual Typing
                        CB_Type1.SelectedIndex = CB_Type2.SelectedIndex = rnd.Next(0, typelen);
                    else
                    {
                        CB_Type1.SelectedIndex = rnd.Next(0, typelen);
                        CB_Type2.SelectedIndex = rnd.Next(0, typelen);
                    }
                }
            }
            saveEntry();
            Util.Alert("All relevant Pokemon Personal Entries have been randomized!");
        }
        private void B_ModifyAll(object sender, EventArgs e)
        {
            for (int i = 1; i < CB_Species.Items.Count; i++)
            {
                CB_Species.SelectedIndex = i; // Get new Species

                if (CHK_NoEV.Checked)
                    for (int z = 0; z < 6; z++)
                        ev_boxes[z].Text = 0.ToString();
                if (CHK_LowCatch.Checked)
                    TB_CatchRate.Text = 3.ToString("000");
                if (CHK_Growth.Checked)
                    CB_EXPGroup.SelectedIndex = 5;
                if (CHK_EXP.Checked)
                    TB_BaseExp.Text = ((float)NUD_EXP.Value*(Convert.ToUInt16(TB_BaseExp.Text)/100f)).ToString("000");

                if (CHK_QuickHatch.Checked)
                    TB_HatchCycles.Text = 1.ToString();
            }
            CB_Species.SelectedIndex = 1;
            Util.Alert("All species modified to specification!");
        }
        private bool dumping;
        private void B_Dump_Click(object sender, EventArgs e)
        {

            if (DialogResult.Yes != Util.Prompt(MessageBoxButtons.YesNo, "Dump all Personal Entries to Text File?"))
                return;

            dumping = true;
            string result = "";
            for (int i = 0; i < CB_Species.Items.Count; i++)
            {
                CB_Species.SelectedIndex = i; // Get new Species
                result += "======" + Environment.NewLine + entry + " " + CB_Species.Text + Environment.NewLine + "======" + Environment.NewLine;

                result +=
                    $"Base Stats: {TB_BaseHP.Text}.{TB_BaseATK.Text}.{TB_BaseDEF.Text}.{TB_BaseSPA.Text}.{TB_BaseSPD.Text}.{TB_BaseSPE.Text} (BST: {pkm.BST})" + Environment.NewLine;
                result +=
                    $"EV Yield: {TB_HPEVs.Text}.{TB_ATKEVs.Text}.{TB_DEFEVs.Text}.{TB_SPAEVs.Text}.{TB_SPDEVs.Text}.{TB_SPEEVs.Text}" + Environment.NewLine;
                result += $"Abilities: {CB_Ability1.Text} (1) | {CB_Ability2.Text} (2) | {CB_Ability3.Text} (H)" + Environment.NewLine;

                result += string.Format(CB_Type1.SelectedIndex != CB_Type2.SelectedIndex ? "Type: {0} / {1}" : "Type: {0}", CB_Type1.Text, CB_Type2.Text);

                result += $"Item 1 (50%): {CB_HeldItem1.Text}" + Environment.NewLine;
                result += $"Item 2 (5%): {CB_HeldItem2.Text}" + Environment.NewLine;
                result += $"Item 3 (1%): {CB_HeldItem3.Text}" + Environment.NewLine;
                // I don't want to add anything else. Should be pretty easy for anyone else to expand.

                result += Environment.NewLine;
            }

            SaveFileDialog sfd = new SaveFileDialog {FileName = "Personal Entries.txt", Filter = "Text File|*.txt"};

            SystemSounds.Asterisk.Play();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string path = sfd.FileName;
                File.WriteAllText(path, result, Encoding.Unicode);
            }
            dumping = false;
        }
        private void CHK_Stats_CheckedChanged(object sender, EventArgs e)
        {
            L_StatDev.Visible = NUD_StatDev.Visible = CHK_Stats.Checked;
        }
        private void CHK_Ability_CheckedChanged(object sender, EventArgs e)
        {
            CHK_WGuard.Enabled = CHK_Ability.Checked;
            if (!CHK_WGuard.Enabled)
                CHK_WGuard.Checked = false;
        }

        private void formClosing(object sender, FormClosingEventArgs e)
        {
            if (entry > -1) saveEntry();
        }
    }
}