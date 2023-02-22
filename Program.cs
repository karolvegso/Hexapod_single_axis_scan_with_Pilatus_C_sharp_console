using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace hexapod_single_axis_scan_with_Pilatus
{
    class Program
    {
        static void Main(string[] args)
        {
            // this program perform single axis scan of hexapod with expositions of Pilatus detector
            // Hexapod section
            // ask for IP address of hexapod
            Console.WriteLine("Insert IP address of hexapod: ");
            string server_hexapod = Console.ReadLine();
            // ask for port of hexapod
            Console.WriteLine("Insert port number of hexapod (e.g. 50000): ");
            string port_hexapod_str = Console.ReadLine();
            Int32 port_hexapod = Int32.Parse(port_hexapod_str);
            // create session with hexapod
            TcpClient session_hexapod = new TcpClient(server_hexapod, port_hexapod);
            NetworkStream stream_hexapod = session_hexapod.GetStream();
            // select scanning axis 
            Console.WriteLine("Select scanning axis; X/Y/Z/U/V/W: ");
            string axis = Console.ReadLine();
            // scan axis from absolute value
            Console.WriteLine("Scan single axis from value (either in mm or deg): ");
            string from_value_str = Console.ReadLine();
            float from_value =float.Parse(from_value_str);
            // scan axis to absolute value
            Console.WriteLine("Scan single axis to value (either in mm or deg): ");
            string to_value_str = Console.ReadLine();
            float to_value = float.Parse(to_value_str);
            // scan single axis with step
            Console.WriteLine("Scan single axis with step (either in mm or deg)");
            string step_value_str = Console.ReadLine();
            float step_value = float.Parse(step_value_str);
            // wait until stage find initial position
            Console.WriteLine("Wait is seconds until stage find initial position: ");
            string wait_initial_str = Console.ReadLine();
            int wait_initial = int.Parse(wait_initial_str);
            wait_initial = wait_initial * 1000; // in ms, miliseconds
            // wait between steps
            Console.WriteLine("Wait is seconds between steps: ");
            string wait_step_str = Console.ReadLine();
            int wait_step = int.Parse(wait_step_str);
            wait_step = wait_step * 1000; // in ms, miliseconds
            // Pilatus detector section
            // ask for IP address of Pilatus detector
            Console.WriteLine("Insert IP address of Pilatus detector: ");
            string server_Pilatus = Console.ReadLine();
            // ask for port of Pilatus detector
            Console.WriteLine("Insert port number of Pilatus detector (e.g. 41234): ");
            string port_Pilatus_str = Console.ReadLine();
            Int32 port_Pilatus = Int32.Parse(port_Pilatus_str);
            // set number of images
            Console.WriteLine("Set number of images: ");
            string ni_str = Console.ReadLine();
            int ni = int.Parse(ni_str);
            // set exposure time
            Console.WriteLine("Set exposure time in seconds: ");
            string expt_str = Console.ReadLine();
            float expt = float.Parse(expt_str);
            // set exposure period
            Console.WriteLine("Set exposure period in seconds: ");
            string expp_str = Console.ReadLine();
            float expp = float.Parse(expp_str);
            // set image name
            Console.WriteLine("Set image name: ");
            string image_name = Console.ReadLine();
            // declare data as byte array in method and initialize it to null value
            Byte[] data = null;
            // move to initial position
            string cmd_mov_initial = "MOV " + from_value_str + "\n";
            Console.WriteLine(cmd_mov_initial);
            data = System.Text.Encoding.ASCII.GetBytes(cmd_mov_initial);
            stream_hexapod.Write(data, 0, data.Length);
            Thread.Sleep(wait_initial);
            data = null;
            // take exposition with Pilatus detector
            string image_name_number = "0";
            image_name_number = image_name_number.PadLeft(5, '0');
            string image_name_total = image_name + "_" + image_name_number;
            Pilatus_exposition(server_Pilatus, port_Pilatus, ni, expt, expp, image_name_total);
            // initialize position variable
            float position = from_value;
            // initialize image counter
            int counter = 1;
            // main while loop
            while (position <= to_value)
            {
                string cmd_mvr = "MVR " + step_value_str + "\n";
                Console.WriteLine(cmd_mvr);
                data = System.Text.Encoding.ASCII.GetBytes(cmd_mvr);
                stream_hexapod.Write(data, 0, data.Length);
                Thread.Sleep(wait_step);
                data = null;
                // take exposition with Pilatus detector
                image_name_number = Convert.ToString(counter);
                image_name_number = image_name_number.PadLeft(5, '0');
                image_name_total = image_name + "_" + image_name_number;
                Pilatus_exposition(server_Pilatus, port_Pilatus, ni, expt, expp, image_name_total);
                // increment position variable with step value
                position += step_value;
                // increment image counter
                counter += 1;
            }
            // close stream and session
            stream_hexapod.Flush();
            stream_hexapod.Close();
            session_hexapod.Close();
            // end of program
        }

        static void Pilatus_exposition(string server_Pilatus, int port_Pilatus, int ni, float expt, float expp, string image_name)
        {
            // create new session with Pilatus detector
            TcpClient session_Pilatus = new TcpClient(server_Pilatus, port_Pilatus);

            // initialize stream
            NetworkStream stream = session_Pilatus.GetStream();

            // set number of images
            string ni_str = Convert.ToString(ni);
            string niString = "ni " + ni_str + "\n";
            Console.WriteLine(niString);
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(niString);
            stream.Write(data, 0, data.Length);
            // Buffer to store the response bytes.
            data = new Byte[256];
            // String to store the response ASCII representation.
            string responseData_ni = String.Empty;
            // Read Pilatus's response
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData_ni = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            data = null;
            Thread.Sleep(250);

            // set exposure time
            string expt_str = Convert.ToString(expt);
            string exptString = "expt " + expt_str + "\n";
            Console.WriteLine(exptString);
            data = System.Text.Encoding.ASCII.GetBytes(exptString);
            stream.Write(data, 0, data.Length);
            // Read Pilatus's response
            data = new Byte[256];
            // String to store the response ASCII representation.
            string responseData_expt = String.Empty;
            bytes = stream.Read(data, 0, data.Length);
            responseData_expt = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            data = null;
            Thread.Sleep(250);

            // set exposure period
            string expp_str = Convert.ToString(expp);
            string exppString = "expp " + expp_str + "\n";
            Console.WriteLine(exppString);
            data = System.Text.Encoding.ASCII.GetBytes(exppString);
            stream.Write(data, 0, data.Length);
            // Read Pilatus's response
            data = new Byte[256];
            // String to store the response ASCII representation.
            string responseData_expp = String.Empty;
            // Read Pilatus's response
            bytes = stream.Read(data, 0, data.Length);
            responseData_expp = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            data = null;
            Thread.Sleep(250);

            // take image, do exposure
            string expoString = "expo " + image_name + ".tif" + "\n";
            Console.WriteLine(expoString);
            data = System.Text.Encoding.ASCII.GetBytes(expoString);
            stream.Write(data, 0, data.Length);
            // Read Pilatus's response
            data = new Byte[256];
            // String to store the response ASCII representation.
            string responseData_expo = String.Empty;
            // Read Pilatus's response
            bytes = stream.Read(data, 0, data.Length);
            responseData_expo = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            data = null;
            Thread.Sleep(250);

            // print all responses
            Console.WriteLine(responseData_ni);
            Console.WriteLine(responseData_expt);
            Console.WriteLine(responseData_expp);
            Console.WriteLine(responseData_expo);

            float ni_float = Convert.ToSingle(ni);
            float wait_in_ms_float = ni_float * expp * 1000 + 100;
            int wait_in_ms = Convert.ToInt32(wait_in_ms_float);
            Thread.Sleep(wait_in_ms);

            // Close everything
            stream.Flush();
            stream.Close();
            session_Pilatus.Close();
        }
    }
}
