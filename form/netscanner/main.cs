using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NetTools;
using System.Threading;
using System.Net;
using Equin.ApplicationFramework;

namespace NetScan
{
    public partial class main : Form
    {

        // test
        

        // Store ip list
        BindingList<IpDetails> IpList = new BindingList<IpDetails>();

        // Ip list view for datagrid
        BindingListView<IpDetails> IpListView;

        /// <summary>
        /// Form 1 init
        /// </summary>
        public main()
        {
            InitializeComponent();

            // Init view
            IpListView = new BindingListView<IpDetails>(IpList);

            // Aply sort
            IpListView.ApplySort("IpWeight ASC");

            // Aply filter
            IpListView.ApplyFilter(delegate (IpDetails Ip) { return Ip.PingState; });

            // Disablae autogenrate colum
            dataGridView1.AutoGenerateColumns = false;

            // Bind view to datagrid
            dataGridView1.DataSource = IpListView;

            // Hide col 0 witch is for sort
            dataGridView1.Columns[0].Visible = false;

        }

        /// <summary>
        /// Start Scan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

            StartScan();

        }

        void StartScan()
        {

            IPAddressRange iprange;

            // try to parse ip range from textbox
            try
            {
                iprange = IPAddressRange.Parse(textBox1.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Wrong ip range format !\n\n" +
                    "Format :\n" +
                    "  192.168.0.0/24\n" +
                    "  192.168.0.0/255.255.255.0\n" +
                    "  192.168.0.0-192.168.0.255"
                    , "NetScan - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Lock form imputs
            LockInput(true);

            // Clear datagrid
            IpList.Clear();

            // Init progress bar
            progressBar1.Maximum = iprange.AsEnumerable().Count();
            progressBar1.Value = 0;

            // Get time out
            int timeout = int.TryParse(textBox2.Text, out timeout) ? timeout : 100;
            if (timeout < 10 || timeout > 5000)
                timeout = 100;
            
            textBox2.Text = timeout.ToString();

            // Add each ip in range to scan thread
            foreach (IPAddress ip in iprange)
            {

                // Start thread
                IPAddress loopIp = ip;
                WaitCallback func = delegate (object state)
                {
                    if (IpTools.PingIP(loopIp, timeout))
                    {
                        Console.WriteLine("Ping {0} Success", loopIp.ToString());
                        string hostname = IpTools.GetHostName(ip);
                        string mac = IpTools.GetMacFromIP(loopIp);
                        AddRow(loopIp, mac, true, hostname);
                    }
                    else
                    {
                        Console.WriteLine("Ping {0} Failed", loopIp.ToString());
                        AddRow(loopIp, null, false, null);
                    }
                };
                ThreadPool.QueueUserWorkItem(func);
            }

        }

        /// <summary>
        /// Lock or release form imputs
        /// </summary>
        /// <param name="v"></param>
        private void LockInput(bool v)
        {
            textBox1.Enabled = !v;
            textBox2.Enabled = !v;
            button1.Enabled = !v;
        }

        /// <summary>
        /// add row to iplist
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="status"></param>
        private void AddRow(IPAddress ip, string mac, bool status, string hostname)
        {
            MethodInvoker inv = delegate
            {
                // Add row to ip list
                this.IpList.Add(new IpDetails(ip, mac, status, hostname));

                // Update progressbar
                this.progressBar1.Value += 1;

                // Release interface if it is the last ip
                if (this.progressBar1.Value == this.progressBar1.Maximum)
                    this.LockInput(false);

            };

            this.Invoke(inv);

        }

        /// <summary>
        /// Conditional formating for datagridview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

            int ColPingIndex = dataGridView1.Columns["PingState"].Index;


            // Chnage row backgroubnd color if ping is ok
            DataGridViewRow Myrow = dataGridView1.Rows[e.RowIndex];
            if ((bool)Myrow.Cells[ColPingIndex].Value == true)
            {
                Myrow.DefaultCellStyle.BackColor = Color.FromArgb(198, 239, 206);
            }
            else
            {
                Myrow.DefaultCellStyle.BackColor = Color.FromArgb(255, 199, 206);
            }

            // Convert True/False to Yes/No
            if (dataGridView1.Columns[e.ColumnIndex].Name == "PingState")
            {
                if (e.Value is bool)
                {
                    bool value = (bool)e.Value;
                    e.Value = (value) ? "Yes" : "No";
                    e.FormattingApplied = true;
                }
            }

        }

        /// <summary>
        /// Form load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            IPAddress localip = IpTools.GetLocalIpAddress();
            IPAddress mask = IpTools.GetSubnetMask(localip);
            IPAddress network = IpTools.GetNetworkAddress(localip, mask);
            int cidr = IPAddressRange.SubnetMaskLength(mask);

            textBox1.Text = network + "/" + cidr;
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            ShowIpRangeToolTip();

        }

        private void textBox1_MouseHover(object sender, EventArgs e)
        {
            ShowIpRangeToolTip();
        }

        /// <summary>
        /// Show tool tip
        /// </summary>
        void ShowIpRangeToolTip()
        {
            ToolTip tt = new ToolTip();
            tt.InitialDelay = 0;
            tt.Show("Supported format :\n" +
                "  192.168.0.0/24\n" +
                "  192.168.0.0/255.255.255.0\n" +
                "  192.168.0.0-192.168.0.255"
                , textBox1, 5, 20, 5000);
        }

        /// <summary>
        /// Catch enter key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                StartScan();
            }
        }

        /// <summary>
        /// Manage offline host filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                IpListView.RemoveFilter();
            }
            else
            {
                IpListView.ApplyFilter(delegate (IpDetails Ip) { return Ip.PingState; });
            }


        }

        /// <summary>
        /// Past data to cpliboard on double click on cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var cellvalue = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                if (cellvalue != null && !String.IsNullOrWhiteSpace(cellvalue.ToString()))
                {
                    Clipboard.SetText(cellvalue.ToString());
                }
            }

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
