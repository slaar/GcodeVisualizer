using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace GcodeVisualizer
{
    public partial class EditForm : Form
    {
        Drawable PreviousCommand = new Drawable();
        public EditForm()
        {
            InitializeComponent();
        }
        public EditForm(string d, Drawable prev)
        {
            InitializeComponent();
            EditTextBox.Text = d;
            PreviousCommand = prev;
            EditTextBox.BackColor = Color.White;
        }

        private void EditForm_Load(object sender, EventArgs e)
        {

        }

        private void EditTextBox_TextChanged(object sender, EventArgs e)
        {
            if (ValidateGCode(EditTextBox.Text))
                EditTextBox.BackColor = Color.DarkSeaGreen;
            else
                EditTextBox.BackColor = Color.LightCoral;
        }

        private bool ValidateGCode(string cmd)
        {
            IList<string> parsedLine = cmd.Split(' ').ToList<string>();
            var CommentMarker = parsedLine.IndexOf(";");
            var OpenParen = parsedLine.IndexOf("(");
            var CloseParen = parsedLine.IndexOf(")");
            Dictionary<string, float> args = new Dictionary<string, float>();
            bool HasX = false;
            bool HasY = false;
            bool HasZ = false;
            bool HasI = false;
            bool HasJ = false;
            bool HasK = false;
            bool HasG = false;
            bool IsComment = false;
            bool IsG02 = false;
            bool IsG03 = false;
            bool IsG00 = false;
            bool IsG01 = false;
            bool Clean = true;
            string ValidPattern = @"[XYIJ]\-*[0-9\.]+";
            Regex r = new Regex(ValidPattern);
            foreach (var bit in parsedLine)
            {
                if (bit.Count() > 0 && (CommentMarker < 0 || cmd.IndexOf(bit) < CommentMarker) && (OpenParen < 0 || cmd.IndexOf(bit) < OpenParen))
                {
                    if (bit[0] == 'G')
                    {
                        if (bit == "G00" || bit == "G0")
                        {
                            HasG = true;
                            IsG00 = true;
                        }
                        else if (bit == "G01" || bit == "G1")
                        {
                            IsG01 = true;
                            HasG = true;
                        }
                        else if (bit == "G03" || bit == "G3")
                        {
                            IsG03 = true;
                            HasG = true;
                        }
                        else if (bit == "G02" || bit == "G2")
                        {
                            IsG02 = true;
                            HasG = true;
                        }
                    }
                    else if (r.IsMatch(bit))
                    {
                        if (bit[0] == 'X')
                        {
                            HasX = true;
                        }
                        if (bit[0] == 'Y')
                        {
                            HasY = true;
                        }
                        if (bit[0] == 'Z')
                        {
                            HasZ = true;
                        }
                        if (bit[0] == 'I')
                        {
                            HasI = true;
                        }
                        if (bit[0] == 'J')
                        {
                            HasJ = true;
                        }
                        if (bit[0] == 'K')
                        {
                            HasK = true;
                        }

                        if (!Char.IsLetter(bit[0]) && bit[0] != ';' && bit[0] != '(')
                        {
                            Clean = false;
                        }
                        bool LetterFound = false;
                        foreach (char c in bit)
                        {
                            if (Char.IsLetter(c))
                                if (!LetterFound)
                                    LetterFound = true;
                                else
                                    Clean = false;
                        }

                    }


                    if (!HasG)
                    {
                        if (PreviousCommand is HeadMotion)
                        {
                            IsG00 = true;
                            HasG = true;
                        }
                        else if (PreviousCommand is LineSegment)
                        {
                            IsG01 = true;
                            HasG = true;
                        }
                        else if (PreviousCommand is Arc)
                        {
                            Arc a = (Arc)PreviousCommand;
                            //IsG02 = a.CW; IsG03 = !a.CW;  //haaaaaaaa

                            if (a.CW)
                                IsG02 = true;
                            else
                                IsG03 = true;

                            HasG = true;
                        }
                    }
                }
            }
            string output = "";
            if (IsG00) output = output + " G00";
            if (IsG01) output = output + " G01";
            if (IsG02) output = output + " G02";
            if (IsG03) output = output + " G03";
            if (HasX) output = output + " HasX";
            if (HasY) output = output + " HasY";
            if (HasZ) output = output + " HasZ";
            if (HasI) output = output + " HasI";
            if (HasJ) output = output + " HasJ";
            if (HasK) output = output + " HasK";
            if (HasG) output = output + " HasG";
            EditDebugOutput.Text = output;
            return (Clean && 
                (
                    ((IsG00 || IsG01) && (HasX || HasY || HasZ)) || 
                    ((IsG02 | IsG03) && (HasX && HasY && HasI && HasJ))
                )
            );
        }

        private void EditBoxTestButton_Click(object sender, EventArgs e)
        {
            IList<string> parsedLine = EditTextBox.Text.Split(' ').ToList<string>();
            //MessageBox.Show(parsedLine[1]);
        }
    }
}
