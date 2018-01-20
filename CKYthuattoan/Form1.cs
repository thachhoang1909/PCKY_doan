using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;
using System.IO;

namespace CKYthuattoan
{
    public partial class Form1 : Form
    {
        static Tuple<List<string[]>, List<string[]>> rule;
        static List<List<List<Node>>> table;
        static DataTable dataTable;

        public class Node
        {
            public string element;
            public float probability;

            public Node first_element;
            public Node second_element;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            // run python script to convert rule
            string appPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            string fileName = appPath + @"\convert_rule.py";

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(@"C:\Python34\python.exe", fileName)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            p.Start();

            //string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Close();

            // readfile rule
            rule = readFileRule();

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        static List<Node> getVT(string element)
        {
            List<Node> results = new List<Node>();

            for (int j = 0; j < rule.Item2.Count; j++)
                if (element == rule.Item2[j][1])
                {
                    Node temp = new Node();
                    temp.element = rule.Item2[j][0];
                    Node temp_first_element = new Node();
                    temp_first_element.element = rule.Item2[j][1];

                    temp.first_element = temp_first_element;
                    temp.probability = float.Parse(rule.Item2[j][2], CultureInfo.InvariantCulture.NumberFormat);
                    results.Add(temp);
                }

            return results;
        }

        static List<Node> getVT(List<Node> element1, List<Node> element2)
        {
            /*
             * element1, element2 ex: [[S,NP,VP,0.5], [NP, NN, NN, 0.1]]
             */
            List<Node> results = new List<Node>();
            for (int i = 0; i < element1.Count; i++)
            {
                for (int j = 0; j < element2.Count; j++)
                {
                    // find in rule
                    for (int k = 0; k < rule.Item1.Count; k++)
                    {
                        if (element1[i].element == rule.Item1[k][1])
                        {
                            if (element2[j].element == rule.Item1[k][2])
                            {

                                /// calculate probability
                                /// p(newRule) = p(element1)*p(elemen2)
                                /// Check in traceBack
                                /// 

                                float pNew = float.Parse(rule.Item1[k][3], CultureInfo.InvariantCulture.NumberFormat);
                                pNew = element1[i].probability * element2[j].probability * pNew;

                                /// Check new rule in traceBack table
                                /// If it has already in table then compare probability
                                /// If greater then replace, else do not thing
                                /// 
                                Boolean isInTable = false;
                                for (int m = 0; m < results.Count; m++)
                                {
                                    /// check for each rule that contain element1 and element2
                                    ///

                                    if (results[m].element == rule.Item1[k][0])
                                    {
                                        if (results[m].first_element.element == rule.Item1[k][1])
                                        {
                                            if (results[m].second_element.element == rule.Item1[k][2])
                                            {
                                                isInTable = true;
                                                if (element1[i].probability > 0 && element2[j].probability > 0 && results[m].probability < pNew)
                                                {
                                                    results[m].probability = pNew;
                                                    results[m].first_element = element1[i];
                                                    results[m].second_element = element2[j];
                                                }
                                            }
                                        }
                                    }
                                }

                                if (!isInTable)
                                {
                                    Node temp = new Node();
                                    temp.element = rule.Item1[k][0];
                                    temp.probability = pNew;
                                    temp.first_element = element1[i];
                                    temp.second_element = element2[j];

                                    results.Add(temp);
                                }
                            }

                        }
                    }
                }
            }

            return results;
        }

        static Tuple<List<string[]>, List<string[]>> readFileRule()
        {
            // read path
            string pathDic = System.IO.Directory.GetCurrentDirectory();
            string pathFile = pathDic + @"\rule.txt";
            string[] lines = System.IO.File.ReadAllLines(pathFile);

            List<string[]> rule1 = new List<string[]>(); // S -> NP VP
            List<string[]> rule2 = new List<string[]>(); // VB -> hoc


            string[] stringSeparators = new string[] { "->", " " };
            for (int i = 0; i < lines.Length; i++)
            {
                string[] tempString = lines[i].Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (tempString.Length == 3)
                    rule2.Add(tempString);
                else if(tempString.Length == 4)
                    rule1.Add(tempString);
            }

            return Tuple.Create(rule1, rule2);
        }

        static private List<List<List<Node>>> CKY(string query)
        {

            // split query 
            string[] stringSeparators = new string[] { " " };
            string[] listWord;
            listWord = query.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            // create data grid view
            //dataTable = createDatatable(listWord);

            List<List<List<Node>>> table = new List<List<List<Node>>>(listWord.Length);

            for (int i = 0; i < listWord.Length; i++)
            {
                table.Add(new List<List<Node>>(listWord.Length));
                for (int j = 0; j < listWord.Length; j++)
                    table[i].Add(new List<Node>());
            }

            // ruleNonterminal chua cac luat VT -> Vp1 Vp2
            // ruleTerminal chua cac luat VT-> word

            for (int j = 0; j < listWord.Length; j++)
            {
                List<Node> tempResuts = getVT(listWord[j]);

                for (int i = 0; i < tempResuts.Count; i++)
                    table[j][j].Add(tempResuts[i]);

                for (int i = j - 1; i >= 0; i--)
                {
                    for (int k = i; k < j; k++)
                    {

                        tempResuts = getVT(table[i][k], table[k + 1][j]);
                        for (int m = 0; m < tempResuts.Count; m++)
                            table[i][j].Add(tempResuts[m]);
                    }
                }
            }

            return table;

        }

        private async void runCKY(string query)
        {

            // split query 
            string[] stringSeparators = new string[] { " " };
            string[] listWord;
            listWord = query.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            

            // create data grid view
            dataTable = new DataTable();
            dataTable = createDatatable(listWord);
            dataGridView1.DataSource = dataTable;
            dataGridView1.Refresh();

            for (int i = 0; i < listWord.Length; i++)
            {
                dataGridView1.Rows[0].Cells[i+1].Value = listWord[i];
                dataGridView1.Rows[i+1].Cells[0].Value = i.ToString();
            }

            table = new List<List<List<Node>>>(listWord.Length);

            for (int i = 0; i < listWord.Length; i++)
            {
                table.Add(new List<List<Node>>(listWord.Length));
                for (int j = 0; j < listWord.Length; j++)
                    table[i].Add(new List<Node>());
            }

            // ruleNonterminal chua cac luat VT -> Vp1 Vp2
            // ruleTerminal chua cac luat VT-> word

            for (int j = 0; j < listWord.Length; j++)
            {
                List<Node> tempResuts = getVT(listWord[j]);
                if(tempResuts.Count != 0)
                {
                    Node value = tempResuts[0];
                    string temp_value = value.element + '(' + (value.probability*100).ToString() + ')';
                    table[j][j].Add(tempResuts[0]);
                    for (int i = 1; i < tempResuts.Count; i++)
                    {
                        table[j][j].Add(tempResuts[i]);
                        temp_value += ", " + tempResuts[i].element + '(' + (tempResuts[i].probability * 100).ToString() + ')';
                    }

                    // show in datagridview
                    await Task.Delay(1000);
                    dataGridView1.Rows[j+1].Cells[j+1].Style.BackColor = Color.Yellow;
                    dataGridView1.Rows[j+1].Cells[j+1].Value = temp_value;
                    await Task.Delay(1000);
                    dataGridView1.Rows[j+1].Cells[j+1].Style.BackColor = Color.White;
                }
                

                for (int i = j - 1; i >= 0; i--)
                {
                    for (int k = i; k < j; k++)
                    {

                        tempResuts = getVT(table[i][k], table[k + 1][j]);

                        if(tempResuts.Count != 0)
                        {
                            string temp_value = "";
                            if (dataGridView1.Rows[i+1].Cells[j+1].FormattedValue != "")
                            {
                                temp_value += dataGridView1.Rows[i+1].Cells[j+1].Value + ",";
                            }
                            temp_value += tempResuts[0].element + '(' + (tempResuts[0].probability * 100).ToString() + ')';
                            table[i][j].Add(tempResuts[0]);
                            for (int m = 1; m < tempResuts.Count; m++)
                            {
                                table[i][j].Add(tempResuts[m]);
                                temp_value += ", " + tempResuts[m].element + '(' + (tempResuts[m].probability * 100).ToString() + ')';
                            }
                            temp_value += " [(" + i.ToString() + "," + (k+1).ToString() + "), (" + (k+1).ToString() + "," + (j+1).ToString() + ")]";
                            // show in datagridview
                            dataGridView1.Rows[i+1].Cells[k+1].Style.BackColor = Color.Aqua;
                            dataGridView1.Rows[k + 1+1].Cells[j+1].Style.BackColor = Color.Aqua;
                            await Task.Delay(1000);
                            dataGridView1.Rows[i+1].Cells[j+1].Style.BackColor = Color.Yellow;
                            dataGridView1.Rows[i+1].Cells[j+1].Value = temp_value;
                            await Task.Delay(1000);
                            dataGridView1.Rows[j+1].Cells[j+1].Style.BackColor = Color.White;
                            dataGridView1.Rows[i+1].Cells[k+1].Style.BackColor = Color.White;
                            dataGridView1.Rows[k + 1+1].Cells[j+1].Style.BackColor = Color.White;

                        }
                        
                    }
                }
            }
            showTree();
        }
        static DataTable createDatatable(string[] listWord)
        {
            DataTable tempTable = new DataTable();

            for (int i = 0; i <= listWord.Length; i++)
            {
                tempTable.Columns.Add(i.ToString());

                while (tempTable.Rows.Count <= listWord.Length+1)
                    tempTable.Rows.Add();

            }
            


            return tempTable;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string query = textBox1.Text.ToString();
            //table = CKY(query);
            //showStepDatatable();
            dataGridView1.DataSource = null;
            dataGridView1.Refresh();
            dataGridView1.Visible = true;
            label1.Visible = true;
            try
            {
                runCKY(query);
                //dataGridView1.DataSource = dataTable;
            }
            catch
            {
                MessageBox.Show("Can't parse this sentence", "Error", MessageBoxButtons.OK);
            }
            
        }


        private void textBox1_Enter(object sender, EventArgs e)
        {
            ActiveForm.AcceptButton = button1;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }


        public void showTree()
        {
            genPythonScript();
            // connect python
            string appPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            string fileName =appPath +  @"\gen_tree.py";

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(@"C:\Python34\python.exe", fileName)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            p.Start();

            //string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Close();


            Form2 form2 = new Form2();
            form2.Show();
  
            
        }

        public string recurent(Node node)
        {
            string result = "";
            if (node != null)
            {
                result += node.element + ":" + (node.probability*100).ToString("0.00");
                
                if(node.first_element != null)
                {
                    result += "(";
                    result += recurent(node.first_element);
                    result += ")";
                }
                if(node.second_element != null)
                {
                    result += "(";
                    result += recurent(node.second_element);
                    result += ")";
                }
            }
            return result;
        }

        public void genPythonScript()
        {
            /// get highest probability tree
            /// 
            Node nodeMaxProb = table[0][table[0].Count-1][0];
            for (int i=0; i < table[0][table[0].Count-1].Count; i++)
            {
                if (nodeMaxProb.probability < table[0][table[0].Count - 1][i].probability)
                {
                    nodeMaxProb = table[0][table[0].Count - 1][i];
                }
            }


            /// generate tree string for python
            /// 
            Stack<Node> string_tree = new Stack<Node>();
            string_tree.Push(nodeMaxProb);

            string str_tree = "ruleString = '(";

            str_tree += recurent(nodeMaxProb) + ")'";
            string appPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            string fileName = appPath + @"\string_tree.py";
            File.WriteAllText(fileName, str_tree);

            
        }

        private void button2_Click(object sender, EventArgs e)
        {

            showTree();
        }
    }
}
