/*
 * radioman 2013 -2018
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Diagnostics;


namespace InfoIcDump
{

    public partial class Form1
    {
        //System API
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadBitmap(IntPtr hInstance, int lpBitmapName);
        [DllImport("gdi32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        //InfoIc functions
        [DllImport("InfoIC.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void GetMfcStru(uint Manuf, ref MfcStruct mfstr);
        [DllImport("InfoIC.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void GetIcStru(uint Manuf, uint device, ref DevStruct IcName);
        [DllImport("InfoIC.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern uint GetIcMFC(string search, uint[] ManArray, uint IcType);
        [DllImport("InfoIC.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern uint GetIcList(string search, uint[] ICArray, uint Manuf, uint IcType);
        [DllImport("InfoIC.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern uint GetDllInfo(ref uint p1, ref uint p2);


        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct MfcStruct
        {
            public uint manufacturer;
            public uint logo;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string manufacturer_name;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string manufacturer_description;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct DevStruct
        {
            public uint protocol;
            public uint unknown;
            public uint category;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
            public string name;
            public uint type;
            public uint code_memory_size;
            public uint data_memory_size;
            public uint data_memory2_size;
            public ushort opts7;
            public ushort read_buffer_size;
            public ushort write_buffer_size;
            public ushort opts1;
            public uint opts2;
            public uint opts3;
            public uint chip_id;
            public uint opts5;
            public uint chip_id_size;
            public uint opts6;
            public uint package_details;
            public uint opts4;
        }


        public class device
        {
            [XmlAttribute("name")]
            public string icname;
            [XmlAttribute("protocol")]
            public string protocol;
            [XmlAttribute("variant")]
            public string type;
            [XmlAttribute("read_buffer_size")]
            public string read_buffer_size;
            [XmlAttribute("write_buffer_size")]
            public string write_buffer_size;
            [XmlAttribute("code_memory_size")]
            public string code_memory_size;
            [XmlAttribute("data_memory_size")]
            public string data_memory_size;
            [XmlAttribute("data_memory2_size")]
            public string data_memory2_size;
            [XmlAttribute("chip_id")]
            public string chip_id;
            [XmlAttribute("chip_id_size")]
            public string chip_id_size;
            [XmlAttribute("opts1")]
            public string opts1;
            [XmlAttribute("opts2")]
            public string opts2;
            [XmlAttribute("opts3")]
            public string opts3;
            [XmlAttribute("opts4")]
            public string opts4;
            [XmlAttribute("opts5")]
            public string opts5;
            [XmlAttribute("opts6")]
            public string opts6;
            [XmlAttribute("opts7")]
            public string opts7;
            [XmlAttribute("package_details")]
            public string package_details;
            [XmlAttribute("fuses")]
            public string fuses;
        }

        public struct MICROCHIP_CSV
        {
            public uint DeviceID;
            public uint DeviceIDMask;
            public string fuses;
        };

        public struct ATMEL_CSV
        {
            public uint DeviceID;
            public string fuses;
        };

        SortedDictionary<string, MICROCHIP_CSV> microchip_csv_list = new SortedDictionary<string, MICROCHIP_CSV>();
        SortedDictionary<string, ATMEL_CSV> atmel_csv_list = new SortedDictionary<string, ATMEL_CSV>();

        //constructor
        public Form1()
        {
            IntPtr Hmodule = LoadLibrary("InfoIc.dll");
            if (Hmodule == IntPtr.Zero)
            {
                if (MessageBox.Show(this, "InfoIc.dll was not found!\n Do you want to load it from other place?",
                    "Load error", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                    load_infoic();
            }

            InitializeComponent();
            try
            {
                using (StreamReader stream_reader = new StreamReader("microchip.csv"))
                {
                    string line;
                    MICROCHIP_CSV csv = new MICROCHIP_CSV();
                    while ((line = stream_reader.ReadLine()) != null)
                    {
                        csv.DeviceID = UInt32.Parse(line.Split(';')[1]);
                        csv.DeviceIDMask = UInt32.Parse(line.Split(';')[2]);
                        csv.fuses = line.Split(';')[3];
                        microchip_csv_list.Add(line.Split(';')[0], csv);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Button1.Enabled = false;
            }


            try
            {
                using (StreamReader stream_reader = new StreamReader("atmel.csv"))
                {
                    string line;
                    ATMEL_CSV csv = new ATMEL_CSV();
                    while ((line = stream_reader.ReadLine()) != null)
                    {
                        csv.DeviceID = UInt32.Parse(line.Split(';')[1]);
                        csv.fuses = line.Split(';')[2];
                        atmel_csv_list.Add(line.Split(';')[0], csv);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Button1.Enabled = false;
            }
            populate_mfc_list();
        }


        //Load the infoic.dll
        private void load_infoic()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "InfoIc.dll";
            dlg.Filter = "dll files (*.dll)|*.dll|All files (*.*)|*.*";
            dlg.CheckFileExists = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                //Workaround to unload the infoic.dll
                while (FreeLibrary(GetModuleHandle("InfoIc.dll"))) ;
                FreeLibrary(GetModuleHandle("InfoIc.dll"));

                //Load the new library
                IntPtr Hmodule = LoadLibrary(dlg.FileName);
                if (Hmodule == IntPtr.Zero)
                    MessageBox.Show(this, "Error loading the " + dlg.FileName,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //device type was changed
        private void RadioButton_CheckedChanged(System.Object sender, System.EventArgs e)
        {
            populate_mfc_list();
        }

        //manufacturers list selection was changed
        private void MfcList_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            MfcStruct mfcstruct = new MfcStruct();
            uint[] tag = (uint[])MfcList.Tag;
            GetMfcStru(tag[MfcList.SelectedIndex], ref mfcstruct);
            LogoImage.Image = GetBitmapFromResources(mfcstruct.logo);
            Label1.Text = mfcstruct.manufacturer_description;
            uint[] devices = new uint[4096];
            DevStruct devstruct = new DevStruct();
            DeviceList.Items.Clear();
            for (int i = 0; i < GetIcList(SearchBox.Text.ToUpper(), devices, (uint)tag[MfcList.SelectedIndex], GetIcType()); i++)
            {
                GetIcStru((uint)tag[MfcList.SelectedIndex], devices[i], ref devstruct);
                DeviceList.Items.Add(devstruct.name);
            }
            DeviceList.Tag = devices;
            if (DeviceList.Items.Count > 0)
                DeviceList.SelectedIndex = 0;
            label_mfc.Text = "Manufacturers:" + MfcList.Items.Count.ToString();
        }

        //device list selection was changed
        private void DeviceList_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            DevStruct devstruct = new DevStruct();
            uint[] tag1 = (uint[])MfcList.Tag;
            uint[] tag2 = (uint[])DeviceList.Tag;
            GetIcStru(tag1[MfcList.SelectedIndex], tag2[DeviceList.SelectedIndex], ref devstruct);
            devstruct.chip_id = change_endianess(devstruct.chip_id, devstruct.chip_id_size);
            txt_info.Text = get_ic_string_ini(devstruct).ToString();
            label_devs.Text = "Devices:" + DeviceList.Items.Count.ToString();
        }

        //export type selection was changed
        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            Button1.Enabled = ((checkBox1.Checked || checkBox2.Checked || checkBox3.Checked || checkBox4.Checked) && Button1.Enabled);
        }


        private void button2_Click(object sender, EventArgs e)
        {
            load_infoic();
            populate_mfc_list();
        }


        //start the infoic.dll dump
        private void Button1_Click(System.Object sender, System.EventArgs e)
        {
            dump_database();
        }

        //get the category
        private uint GetIcType()
        {
            if (RadioAll.Checked)
                return 0;
            else if (RadioRom.Checked)
                return 1;
            else if (RadioMcu.Checked)
                return 2;
            else if (RadioPld.Checked)
                return 3;
            else if (RadioRam.Checked)
                return 4;
            return 5;
        }


        //Populate the manufacturers list
        private void populate_mfc_list()
        {
            try
            {
                uint[] manufacturers = new uint[4096];
                MfcStruct b = new MfcStruct();
                MfcList.Items.Clear();
                DeviceList.Items.Clear();
                for (int i = 0; i < GetIcMFC(SearchBox.Text.ToUpper(), manufacturers, GetIcType()); i++)
                {
                    GetMfcStru(manufacturers[i], ref b);
                    MfcList.Items.Add(b.manufacturer_name);
                }
                MfcList.Tag = manufacturers;
                if (MfcList.Items.Count > 0)
                    MfcList.SelectedIndex = 0;
                uint p1 = 0;
                uint p2 = 0;
                label_total.Text = "Total Devices:" + GetDllInfo(ref p1, ref p2).ToString();
            }
            catch
            {
                foreach (Control control in this.Controls)
                {
                    control.Enabled = false;
                }
            }
        }


        /*/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
        uint change_endianess(uint value, uint size)
        {
            if (value == 0 || size == 0) return 0;// This is a database bug. Size is zero and id garbage bytes
            return (uint)((((int)value & 0xff) << 24) +
                (((uint)value & 0xff00) << 8) +
                (((uint)value & 0xff0000) >> 8) +
                (((uint)value & 0xff000000) >> 24))
                >> (int)(8 * (4 - size));
        }

        private string get_fuse_name(string name)
        {
            string key = name.Split('@')[0].Trim();
                    key = key.Split('(')[0].Trim();
                    if (microchip_csv_list.ContainsKey(key))
                        return microchip_csv_list[key].fuses;
                    else if (atmel_csv_list.ContainsKey(key))
                        return atmel_csv_list[key].fuses;
                    return "NULL";
        }

        //Get device info in ini format
        private string get_ic_string_ini(DevStruct devstruct)
        {
            return string.Format(
@"[{0}]
protocol = 0x{1:x2}
variant = 0x{2:x2}
read_buffer_size =  0x{3:x2}
write_buffer_size = 0x{4:x2}
code_memory_size = 0x{5:x2}
data_memory_size = 0x{6:x2}
data_memory2_size = 0x{7:x2}
chip_id = 0x{8:x4}
chip_id_size = 0x{9:x2}
opts1 = 0x{10:x2}
opts2 = 0x{11:x2}
opts3 = 0x{12:x2}
opts4 = 0x{13:x2}
opts5 = 0x{14:x2}
opts6 = 0x{15:x2}
opts7 = 0x{16:x2}
package_details = 0x{17:x8}
fuses = {18}",
            devstruct.name.Trim(), devstruct.protocol, devstruct.type, devstruct.read_buffer_size,
            devstruct.write_buffer_size, devstruct.code_memory_size,
            devstruct.data_memory_size, devstruct.data_memory2_size,
            devstruct.chip_id, devstruct.chip_id_size, devstruct.opts1,
            devstruct.opts2, devstruct.opts3, devstruct.opts4, devstruct.opts5,
            devstruct.opts6, devstruct.opts7, devstruct.package_details, get_fuse_name(devstruct.name.Trim()));
        }

        //Get device info in c header format
        private string get_ic_string_c(DevStruct devstruct)
        {
            return string.Format(
@"{{
    .name = ""{0}"",
    .protocol = 0x{1:x2},
    .variant = 0x{2:x2},
    .read_buffer_size =  0x{3:x2},
    .write_buffer_size = 0x{4:x2},
    .code_memory_size = 0x{5:x2},
    .data_memory_size = 0x{6:x2},
    .data_memory2_size = 0x{7:x2},
    .chip_id = 0x{8:x4},
    .chip_id_size = 0x{9:x2},
    .opts1 = 0x{10:x2},
    .opts2 = 0x{11:x2},
    .opts3 = 0x{12:x2},
    .opts4 = 0x{13:x2},
    .opts5 = 0x{14:x2},
    .opts6 = 0x{15:x2},
    .opts7 = 0x{16:x2},
    .package_details = 0x{17:x8},
    .fuses = {18}
}},",
            devstruct.name.Trim(), devstruct.protocol, devstruct.type, devstruct.read_buffer_size,
            devstruct.write_buffer_size, devstruct.code_memory_size,
            devstruct.data_memory_size, devstruct.data_memory2_size,
            devstruct.chip_id, devstruct.chip_id_size, devstruct.opts1,
            devstruct.opts2, devstruct.opts3, devstruct.opts4, devstruct.opts5,
            devstruct.opts6, devstruct.opts7, devstruct.package_details,get_fuse_name(devstruct.name.Trim()));
        }


        //Get device info in xml format
        private device get_ic_xml(DevStruct devstruct)
        {
            device xml_chip = new device();
            xml_chip.icname = devstruct.name.Trim();
            xml_chip.protocol = "0x" + devstruct.protocol.ToString("x2");
            xml_chip.type = "0x" + devstruct.type.ToString("x2");
            xml_chip.read_buffer_size = "0x" + devstruct.read_buffer_size.ToString("x2");
            xml_chip.write_buffer_size = "0x" + devstruct.write_buffer_size.ToString("x2");
            xml_chip.code_memory_size = "0x" + devstruct.code_memory_size.ToString("x2");
            xml_chip.data_memory_size = "0x" + devstruct.data_memory_size.ToString("x2");
            xml_chip.data_memory2_size = "0x" + devstruct.data_memory2_size.ToString("x2");
            xml_chip.chip_id = "0x" + devstruct.chip_id.ToString("x4");
            xml_chip.chip_id_size = "0x" + devstruct.chip_id_size.ToString("x2");
            xml_chip.opts1 = "0x" + devstruct.opts1.ToString("x2");
            xml_chip.opts2 = "0x" + devstruct.opts2.ToString("x2");
            xml_chip.opts3 = "0x" + devstruct.opts3.ToString("x2");
            xml_chip.opts4 = "0x" + devstruct.opts4.ToString("x2");
            xml_chip.opts5 = "0x" + devstruct.opts5.ToString("x2");
            xml_chip.opts6 = "0x" + devstruct.opts6.ToString("x2");
            xml_chip.opts7 = "0x" + devstruct.opts7.ToString("x2");
            xml_chip.package_details = "0x" + devstruct.package_details.ToString("x8");
            xml_chip.fuses = get_fuse_name(devstruct.name.Trim());
            return xml_chip;
        }

        bool compare_devices(DevStruct device1, DevStruct device2)
        {
            return (string.Compare(device1.name.Split('@')[0], device2.name.Split('@')[0], true) == 0) &&
             (device1.protocol == device2.protocol) &&
             (device1.type == device2.type) &&
             (device1.read_buffer_size == device2.read_buffer_size) &&
             (device1.write_buffer_size == device2.write_buffer_size) &&
             (device1.code_memory_size == device2.code_memory_size) &&
             (device1.data_memory_size == device2.data_memory_size) &&
             (device1.data_memory2_size == device2.data_memory2_size) &&
             (device1.chip_id == device2.chip_id) &&
             (device1.chip_id_size == device2.chip_id_size) &&
             (device1.opts1 == device2.opts1) &&
             (device1.opts2 == device2.opts2) &&
             (device1.opts3 == device2.opts3) &&
             (device1.opts4 == device2.opts4) &&
             (device1.opts5 == device2.opts5) &&
             (device1.opts6 == device2.opts6) &&
             (device1.opts7 == device2.opts7);
        }

        bool is_duplicate(List<DevStruct> devices_list, DevStruct device)
        {
            foreach (DevStruct d in devices_list)
            {
                if (compare_devices(d, device) &&
                    (((device.package_details & 0X80000000) != 0) &&
                    ((device.package_details & 0X7FFFFFFF) == (d.package_details & 0X7FFFFFFF))) ||
                    (compare_devices(d, device) &&
                    ((device.package_details & 0X00FFFF00) == (d.package_details & 0X00FFFF00)))
                    )
                    return true;
            }
            return false;
        }


        //Perform the infoic.dll dump
        private void dump_database()
        {
            uint[] manufacturers = new uint[4096];
            uint[] devices = new uint[4096];
            DevStruct devstruct = new DevStruct();
            List<DevStruct> devices_list = new List<DevStruct>();
            List<string> duplicates = new List<string>();
            List<device> device_list_xml = new List<device>();
            List<string> device_list_ini = new List<string>();
            List<string> device_list_c = new List<string>();
            SortedDictionary<uint, string> total = new SortedDictionary<uint, string>();

            progressBar.Maximum = (int)GetIcMFC("", manufacturers, 0);

            //Iterate over the entire manufacturers
            for (uint i = 0; i < GetIcMFC("", manufacturers, 0); i++)
            {
                //Iterate over the entire devices in the curent manufacturer
                for (uint k = 0; k < GetIcList("", devices, manufacturers[i], 0); k++)
                {
                    //Get the device struct
                    GetIcStru(manufacturers[i], devices[k], ref devstruct);

                    //Remove spaces
                    devstruct.name = devstruct.name.Replace(" ", "");
                    devstruct.chip_id = change_endianess(devstruct.chip_id, devstruct.chip_id_size);

                    //If not duplicate process the chip
                    if (checkBox5.Checked == false || !is_duplicate(devices_list, devstruct))
                    {
                        //Log the device
                        if (total.ContainsKey(devstruct.protocol))
                            total[devstruct.protocol] += devstruct.name + Environment.NewLine;
                        else
                            total.Add(devstruct.protocol, devstruct.name + Environment.NewLine);

                        if (checkBox5.Checked == true)
                        {
                            //Rename specific adapter to generic
                            switch (devstruct.package_details & 0xFF)
                            {
                                case 1:
                                case 2:
                                case 6:
                                    devstruct.name = devstruct.name.Split('@')[0] + "@TSOP";
                                    break;
                            }
                        }
                        devices_list.Add(devstruct);
                    }
                    else
                        duplicates.Add(devstruct.name);
                }
                progressBar.Value += 1;
                Application.DoEvents();
            }
            progressBar.Value = progressBar.Maximum;

            List<DevStruct> tmp_list = new List<DevStruct>();

            foreach (DevStruct d in devices_list)
            {
                devstruct = d;
                //Patch Microchip and Atmel controllers
                if (devstruct.category == 2)
                {
                    string key = devstruct.name.Split('@')[0];
                    key = key.Split('(')[0];
                    if (microchip_csv_list.ContainsKey(key))
                    {
                        devstruct.chip_id = microchip_csv_list[key].DeviceID;
                        devstruct.opts3 = microchip_csv_list[key].DeviceIDMask;
                    }
                    else if (atmel_csv_list.ContainsKey(key))
                    {
                        devstruct.chip_id = atmel_csv_list[key].DeviceID;
                    }
                }
                tmp_list.Add(devstruct);
            }
            devices_list = tmp_list;
            tmp_list = new List<DevStruct>();



            //Sort the list by category
            if (checkBox6.Checked == true)
            {
                for (uint i = 1; i < 6; i++)
                {
                    foreach (DevStruct d in devices_list)
                    {
                        if (d.category == i)
                            tmp_list.Add(d);
                    }
                }
                devices_list = tmp_list;
            }

            //Convert
            for (int i = 0; i < devices_list.Count; i++)
            {
                //Get the element in ini format
                if (checkBox2.Checked)
                    device_list_ini.Add(get_ic_string_ini(devices_list[i]) + Environment.NewLine);

                //Get the element in C header format
                if (checkBox1.Checked)
                    device_list_c.Add(get_ic_string_c(devices_list[i]));

                //Get the element in xml format
                if (checkBox3.Checked)
                {
                    device_list_xml.Add(get_ic_xml(devices_list[i]));
                }
            }
            try
            {
                //Write the devices.h file
                if (checkBox1.Checked)
                {
                    using (StreamWriter stream_writer = new StreamWriter("devices.h"))
                    {
                        foreach (string elem in device_list_c)
                            stream_writer.WriteLine(elem);
                    }
                }

                //write the devices.ini file
                if (checkBox2.Checked)
                {
                    using (StreamWriter stream_writer = new StreamWriter("devices.ini"))
                    {
                        foreach (string elem in device_list_ini)
                            stream_writer.WriteLine(elem);
                    }
                }

                //write the devices.xml file
                if (checkBox3.Checked)
                {
                    XmlTextWriter xml_text_writer = new XmlTextWriter("Devices.xml", System.Text.Encoding.UTF8);
                    xml_text_writer.Formatting = Formatting.Indented;
                    xml_text_writer.Indentation = 2;
                    XmlSerializer serializer = new XmlSerializer(device_list_xml.GetType(), new XmlRootAttribute("devices"));
                    XmlSerializerNamespaces serializer_namespace = new XmlSerializerNamespaces();
                    serializer_namespace.Add("", "");
                    serializer.Serialize(xml_text_writer, device_list_xml, serializer_namespace);
                    xml_text_writer.Close();
                }

                //write the log.txt file
                if (checkBox4.Checked)
                {
                    using (StreamWriter stream_writer = new StreamWriter("log.txt"))
                    {
                        foreach (KeyValuePair<uint, string> key in total)
                        {
                            stream_writer.WriteLine("Protocol:0x" + key.Key.ToString("X2") + Environment.NewLine + key.Value);
                        }
                        stream_writer.Write(Environment.NewLine +
                            devices_list.Count.ToString() + " devices in " +
                            total.Count.ToString() + " protocols.");

                    }

                    using (StreamWriter stream_writer = new StreamWriter("duplicates.txt"))
                    {
                        foreach (string d in duplicates)
                        {
                            stream_writer.WriteLine(d);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                progressBar.Value = 0;
                return;
            }
            MessageBox.Show(this, "Dump was saved in " + Application.StartupPath, "InfoIc", MessageBoxButtons.OK, MessageBoxIcon.Information);
            progressBar.Value = 0;
        }

        //return a bitmap from resource id.
        private Bitmap GetBitmapFromResources(uint resourcesID)
        {
            IntPtr Hmodule = LoadLibrary("InfoIc.dll");
            Bitmap bmp = null;
            IntPtr hBitmap = default(IntPtr);

            try
            {
                if (!Hmodule.Equals(IntPtr.Zero))
                {
                    hBitmap = LoadBitmap(Hmodule, (int)resourcesID);
                    if (!hBitmap.Equals(IntPtr.Zero))
                    {
                        bmp = Bitmap.FromHbitmap(hBitmap);
                    }
                    DeleteObject(hBitmap);
                    FreeLibrary(Hmodule);
                    return bmp;
                }

            }
            catch
            {
            }
            finally
            {
                if (!hBitmap.Equals(null))
                {
                    DeleteObject(hBitmap);
                }
            }
            return bmp;
        }

        //resize bitmap
        private Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(sourceBMP, 0, 0, width, height);
            }
            return result;
        }

    }
}

