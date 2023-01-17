using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1.X509;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using System.Data;
using System.ServiceProcess;

namespace Lab3Service
{
    public partial class Lab3Service : ServiceBase
    {
        public static string connectionString = @"Server=localhost;Database=pinokkio;Uid=root;Pwd=PWDQWERTY03";  
        public Lab3Service()
        {
            InitializeComponent();
        }
        public void OnDebug()
        {
            OnStart(null);
        }

        // Func which check whenever is data changed and if data in aforded range 
        public static void CHECK_data()
        {
            int Task_Execution_Duration_r;
            int Task_Claim_Check_Period_r;
            int Task_Execution_Quantity_r;
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var key = hklm.OpenSubKey(@"Software\\Task_Queue\\Parameters"))
            {

                if (key == null)
                {// if key does not exist -> create Data in Registry
                    try
                    {
                        Microsoft.Win32.Registry.LocalMachine.CreateSubKey("Software\\Task_Queue\\Parameters");
                        RegistryKey subkey;
                        subkey = Registry.LocalMachine.OpenSubKey("Software\\Task_Queue\\Parameters", true);
                        string[] inner_t = { "40" }; // default check time 
                        subkey.SetValue("Task_Claim_Check_Period", inner_t, RegistryValueKind.MultiString);
                        subkey.Close();

                        Microsoft.Win32.Registry.LocalMachine.CreateSubKey("Software\\Task_Queue\\Parameters");
                        RegistryKey subkey2;
                        subkey2 = Registry.LocalMachine.OpenSubKey("Software\\Task_Queue\\Parameters", true);
                        string[] inner_t2 = { "60" }; // default task process time 
                        subkey2.SetValue("Task_Execution_Duration", inner_t2, RegistryValueKind.MultiString);
                        subkey2.Close();

                        Microsoft.Win32.Registry.LocalMachine.CreateSubKey("Software\\Task_Queue\\Parameters");
                        RegistryKey subkey3;
                        subkey3 = Registry.LocalMachine.OpenSubKey("Software\\Task_Queue\\Parameters", true);
                        string[] inner_t3 = { "1" }; // default task amounts to process
                        subkey3.SetValue("Task_Execution_Quantity", inner_t3, RegistryValueKind.MultiString);
                        subkey3.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                else
                {// READING data from REGISTRY
                    try
                    {
                        string time_S = (key.GetValue("Task_Execution_Duration").ToString());
                        Task_Execution_Duration_r = int.Parse(time_S);              // seconds
                        string time_S2 = (key.GetValue("Task_Claim_Check_Period").ToString());
                        Task_Claim_Check_Period_r = int.Parse(time_S2);
                        string time_S3 = (key.GetValue("Task_Execution_Quantity").ToString());
                        Task_Execution_Quantity_r = int.Parse(time_S3);
                        /*if ((Task_Claim_Check_Period_r / 1000) % 5 != 0)
                        {
                            Task_Claim_Check_Period_r += ((Task_Claim_Check_Period_r / 1000) % 5) * 1000;
                        }
                        if ((Task_Execution_Duration_r / 1000) % 5 != 0)
                        {
                            Task_Execution_Duration_r += ((Task_Execution_Duration_r / 1000) % 5) * 1000;
                        }*/

                        if (Task_Claim_Check_Period_r < 10 || Task_Claim_Check_Period_r > 45)
                        {
                            MessageBox.Show("Task_Claim_Check_Period were " + Task_Claim_Check_Period_r + " program will use default time 30 sec.");
                            Task_Claim_Check_Period_r = 30000;
                            Microsoft.Win32.Registry.LocalMachine.CreateSubKey("Software\\Task_Queue\\Parameters");
                            RegistryKey subkey;
                            subkey = Registry.LocalMachine.OpenSubKey("Software\\Task_Queue\\Parameters", true);
                            string[] inner_t = { "30" }; // default check time 
                            subkey.SetValue("Task_Claim_Check_Period", inner_t, RegistryValueKind.MultiString);
                            subkey.Close();

                        }
                        if (Task_Execution_Duration_r < 10 || Task_Execution_Duration_r > 180) // 30
                        {
                            MessageBox.Show("Task_Execution_Duration were " + Task_Execution_Duration_r + " program will use default time 60 sec.");
                            Task_Execution_Duration_r = 60000;
                            Microsoft.Win32.Registry.LocalMachine.CreateSubKey("Software\\Task_Queue\\Parameters");
                            RegistryKey subkey2;
                            subkey2 = Registry.LocalMachine.OpenSubKey("Software\\Task_Queue\\Parameters", true);
                            string[] inner_t2 = { "10" }; // default task process time 60 but for DEBUG it will be 10
                            subkey2.SetValue("Task_Execution_Duration", inner_t2, RegistryValueKind.MultiString);
                            subkey2.Close();
                        }
                        if (Task_Execution_Quantity_r <= 0 || Task_Execution_Quantity_r >= 4)
                        {
                            MessageBox.Show("Task_Execution_Quantity were " + Task_Execution_Quantity_r + " program will use default amount(1).");
                            Task_Execution_Quantity_r = 1;
                            Microsoft.Win32.Registry.LocalMachine.CreateSubKey("Software\\Task_Queue\\Parameters");
                            RegistryKey subkey3;
                            subkey3 = Registry.LocalMachine.OpenSubKey("Software\\Task_Queue\\Parameters", true);
                            string[] inner_t3 = { "1" }; // default task amounts to process
                            subkey3.SetValue("Task_Execution_Quantity", inner_t3, RegistryValueKind.MultiString);
                            subkey3.Close();
                        }

                        //Console.WriteLine("Task_Claim_Check_Period_r: " + Task_Claim_Check_Period_r);
                        //Console.WriteLine("Task_Execution_Duration_r: " + Task_Execution_Duration_r);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        protected override void OnStart(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            CHECK_data();
            //var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64); 
            //var key = hklm.OpenSubKey(@"Software\\Task_Queue\\Parameters");
            //int Merin = (int.Parse(key.GetValue("Task_Execution_Duration").ToString()) * 1000);
            //string Path = "HKEY_LOCAL_MACHINE\\Software\\Task_Queue\\Parameters";
            //int time_for_timer = int.Parse(Registry.GetValue(Path, "Task_Claim_Check_Period", RegistryValueKind.String).ToString()) * 1000;
            //System.Diagnostics.Debugger.Launch(); 
            Timer t = new Timer(int.Parse((RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)).OpenSubKey(@"Software\\Task_Queue\\Parameters").GetValue("Task_Claim_Check_Period").ToString()) * 1000);  // 
            t.Elapsed += new ElapsedEventHandler(WorkingCycle);
            t.Enabled = true;
            WriteLog(" --> Service Task_Queue is STARTED");
            System.Threading.Thread TH = new System.Threading.Thread(StartProgressCycle);
            TH.Start();
            System.Threading.Thread TM = new System.Threading.Thread(ProgressingTasksCycle);
            TM.Start();
        }
        protected override void OnStop()
        {
            WriteLog(" --> Service Task_Queue is STOPPED");
        }
        // Monitoring claims table --> if is anything in table procced with validation  --
        private static void WorkingCycle(object source, ElapsedEventArgs e)
        {

            string task = GetTask();
            //MessageBox.Show("TASK:"+ task + " TAsk_validation: " + TaskValidation(task));
            //MessageBox.Show("TASK (0,5): "+task.Substring(0, 5));
            if (task == "do_nothing")
            {
                return;
            }
            else
            {
                if (TaskValidationDifError(task) == task)
                {
                    if (TaskValidation(task))
                    {
                        AddNewTaskInTasks(task);
                        WriteLog(" --> Задача " + task + " успішно прийнята в обробку...");
                        DeleteTaskFromClaims(task);
                    }
                    else
                    {
                        WriteLog(" --> ПОМИЛКА розміщення заявки " + task);
                        DeleteTaskFromClaims(task);
                    }
                }
                else if (TaskValidationDifError(task) == "del " + task)
                {
                    WriteLog(" --> ПОМИЛКА розміщення заявки " + task + ". Некоректний синтаксис...");
                    DeleteTaskFromClaims(task);
                }
                else if (TaskValidationDifError(task) == "alr " + task)
                {
                    WriteLog(" --> ПОМИЛКА розміщення заявки " + task + ". Номер вже існує ...");
                    DeleteTaskFromClaims(task);
                }
            }
        }
        private static void StartProgressCycle()
        {
            //string Path = "HKEY_LOCAL_MACHINE\\Software\\Task_Queue\\Parameters";     // як часто оновлює виконання завдань
            Timer t2 = new Timer((int.Parse((RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)).OpenSubKey(@"Software\\Task_Queue\\Parameters").GetValue("Task_Execution_Duration").ToString()) * 1000 * 5) / 100); // * 5) / 100
            //int.Parse(Registry.GetValue(Path, "Task_Execution_Duration", RegistryValueKind.String).ToString()) * 1000);
            t2.Elapsed += new ElapsedEventHandler(WorkingCycleWhichStartsTasks);
            t2.Enabled = true;
        }

        private static void ProgressingTasksCycle()
        {
            //string Path = "HKEY_LOCAL_MACHINE\\Software\\Task_Queue\\Parameters";    // час виконання 1 завдання 
            Timer t2 = new Timer((int.Parse((RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)).OpenSubKey(@"Software\\Task_Queue\\Parameters").GetValue("Task_Execution_Duration").ToString()) * 1000 * 5) / 100); // * 5) / 100
            //Timer t2 = new Timer(5000);  -- 60 sec --> 5 sec 1%
            t2.Elapsed += new ElapsedEventHandler(WorkingCycleWhichProgressingTasks);
            t2.Enabled = true;
        }




        public static bool TaskValidation(string task)
        {
            string temp = makeQuery("select Task_name from pinokkio.tasks").ToString();
            //MessageBox.Show(task);
            //MessageBox.Show(task.Substring(0, 5) + " if task substring(5) <= 4: " + (task.Substring(5).Length).ToString() + " Is Int32: " + (int.Parse((task.Substring(5))).GetType().Name).ToString() + " temp: " + temp);
            if (task == "" || task.Substring(0, 5) != "Task_" || task.Substring(5).Length != 4 || !true_ter_func(task.Substring(5)) || temp.Contains(task))
            {
                return false;
            }
            return true;
        }

        public static string TaskValidationDifError(string task)
        {
            string temp = makeQuery("select Task_name from pinokkio.tasks").ToString();
            DataSet temp1 = makeQuery("select Task_name from pinokkio.tasks");
            List<string> strDetailIDList = new List<string>();
            foreach (DataTable dt in temp1.Tables)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    strDetailIDList.Add(dr[0].ToString());
                }
            }
            string strDetailID = String.Join(" ", strDetailIDList.ToArray());
            //bool contains = temp.AsEnumerable().Any(row => task == row.Field<String>("Task_name"));
            if (strDetailID.Contains(task))
            {
                string del = "alr ";
                return del + task;
            }
            else if(task == "" || task.Substring(0, 5) != "Task_" || task.Substring(5).Length != 4 || !true_ter_func(task.Substring(5)))
            {
                string del = "del ";
                return del + task;
            }
            return task;
        }
        public static bool true_ter_func(string data)
        {
            try
            {
                Int64.Parse(data);
                return true;
            }
            catch
            {
                return false;
            }
        }

            private static void WorkingCycleWhichStartsTasks(object source, ElapsedEventArgs e)
        {
            string[] Task = GetTaskToDo(); // Task_1234 - [....................]-Queued

            //MessageBox.Show("Task.Length: " + Task.Length + " Task itself: " + Task);   // 1 + String[] - not "actual_string"           \DGJF;OADHIFTJKSK\KGKCX.CJKRZSAKKLJFISDUHGDlsKdh'gS;FLK /./J,M,CC/LK FH NMVMC[''AP ;Z,JNDTCGBN;VOO\;UF LZJSNRXG.VLO\
            for (int i = 0; i < Task.Length; i++)
            {
                //MessageBox.Show("Task[i]: " + Task[i]);
                MakeTaskInProgress(Task[i]);
            }

        }

        private static void WorkingCycleWhichProgressingTasks(object source, ElapsedEventArgs e)
        {
            string[] tasks = GetTasksToProgress();
            //MessageBox.Show("Tasks[1]: " + GetTasksToProgress()[0]);  
            for (int i = 0; i < tasks.Length; i++)
            {

                int dotes = CountDotes(ref tasks[i]);
                //MessageBox.Show("dotes : " + dotes.ToString()); // 18?
                string reCreatedTask = "";
                if (dotes != 1)
                {
                    //string local = "tasks[i].Substring(0, 13) " + tasks[i].Substring(0, 13) + " makeIs(20 - dotes + 1):" + makeIs(20 - dotes + 1) + " makeDotes(dotes - 1):" + makeDotes(dotes - 1) + " tasks[i].Substring(31, 14): " + tasks[i].Substring(33, 14) + " " + ((20 - dotes + 1) * 5) + "%"; ;
                    //WriteLog(local);
                    reCreatedTask = tasks[i].Substring(0, 13) + makeIs(20 - dotes + 1) + makeDotes(dotes - 1) + tasks[i].Substring(33, 14) + " " + ((20 - dotes + 1) * 5) + "%";
                }
                else
                {
                    reCreatedTask = tasks[i].Substring(0, 13) + makeIs(20 - dotes + 1) + makeDotes(dotes - 1) + tasks[i].Substring(33, 2) + "COMPLETED" + "100%";
                    WriteLog(" " + tasks[i].Substring(0, 9) + " - COMPLETED");
                }
                makeQuery("update pinokkio.tasks Set Task_name ='" + reCreatedTask + "' WHERE Task_name = '" + tasks[i] + "'");
            }

        }

        private static string makeDotes(int numberOfDotes)
        {
            string dotes = "";
            for (int i = 0; i < numberOfDotes; i++)
            {
                dotes += ".";
            }
            return dotes;
        }

        private static string makeIs(int numberOfIs)
        {
            string Is = "";
            for (int i = 0; i < numberOfIs; i++)
            {
                Is += "I";
            }
            return Is;
        }

        private static int CountDotes(ref string task)
        {
            string dotes = task.Substring(13, 20);
            //MessageBox.Show(dotes);
            int dotesLength = 0;
            for (int i = 0; i < dotes.Length; i++)
            {
                if (dotes[i] == '.')
                {
                    dotesLength++;
                }
            }
            return dotesLength;
        }

        private static string[] GetTasksToProgress()
        {
            string query = "select * from pinokkio.tasks";
            DataSet data = makeQuery(query);
            int tasksLength = 0;
            foreach (DataTable dt in data.Tables)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr[1].ToString().Contains("progress"))
                    {
                        tasksLength++;
                    }

                }
            }

            string[] tasks = new string[tasksLength];
            tasksLength--;
            foreach (DataTable dt in data.Tables)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr[1].ToString().Contains("progress"))
                    {
                        tasks[tasksLength] = dr[1].ToString();
                        tasksLength--;
                    }

                }
            }
            return tasks;
        }

        private static void WriteLog(string data)
        {
            string[] log_data_lines = new string[] { DateTime.Now.ToString() };
            string temp = log_data_lines[0].ToString();
            string full_date = temp.Substring(0);
            List<String> date;
            date = full_date.Split('.').ToList();
            string path_to_log = @"C:\KI3\log_" + date[0] + "-" + date[1] + "-" + "2022.log";
            string filler = "--------------------------------";
            if (!File.Exists(path_to_log))
            {
                using (StreamWriter sw = File.CreateText(path_to_log))
                {
                    sw.WriteLine(filler + DateTime.Now.ToString() + filler + Environment.NewLine + data + Environment.NewLine);
                }
            }
            // This text is always added, making the file longer over time if it is not deleted.
            else
            {
                using (StreamWriter sw = new StreamWriter(path_to_log, true))
                {
                    sw.WriteLine(filler + DateTime.Now.ToString() + filler + Environment.NewLine + data + Environment.NewLine);
                }
            }

        }


        private static string[] GetTaskToDo()                                                   /// RETURNS STRING[] ELEMENT NOT A STRING
        {
            string query = "select * from `pinokkio`.`tasks` ORDER BY Task_name ASC;";
            //string Path = "HKEY_LOCAL_MACHINE\\Software\\Task_Queue\\Parameters";
            string[] task = new string[1];
            task[0] = "_NONE";
            int NumberOfTasksToProgress = int.Parse((RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)).OpenSubKey(@"Software\\Task_Queue\\Parameters").GetValue("Task_Execution_Quantity").ToString());
            if (HowManyTasksIsQueuedNow() >= NumberOfTasksToProgress)
            {
                //int loclare = HowManyTasksIsProgressingNow() - NumberOfTasksToProgress;
                NumberOfTasksToProgress -= HowManyTasksIsProgressingNow();
                //MessageBox.Show("numberOfTasksInProgress" + HowManyTasksIsProgressingNow() + " NumberOfTasksToProgress" + NumberOfTasksToProgress);
                //MessageBox.Show("NumberOfTasksToProgress: " + NumberOfTasksToProgress.ToString());
            }
            else if (HowManyTasksIsQueuedNow() <= NumberOfTasksToProgress - HowManyTasksIsProgressingNow())         // 3 <= 3 - 0
            {
                NumberOfTasksToProgress = HowManyTasksIsQueuedNow();
            }
            //MessageBox.Show("NumberOfTasksToProgress: "+ NumberOfTasksToProgress.ToString());  // -1
            if (NumberOfTasksToProgress > 0)                // RECIEVES FALSE WHY
            {
                task = new string[NumberOfTasksToProgress];
                //MessageBox.Show("taskType: " + task.GetType());
                DataSet data = makeQuery(query);                        // 1 / Task_1234 - [....................]-Queued
                //MessageBox.Show(data.Tables.ToString());              // 2 / Task_1235 - [....................]-Queued
                //MessageBox.Show(task.ToString());
                foreach (DataTable dt in data.Tables)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        //MessageBox.Show("Dr[1] = " + dr[1].ToString() + "NumberOfTasksToProgress: " + NumberOfTasksToProgress);
                        //MessageBox.Show("dr[1].ToString().Contains('Queued') = " + (dr[1].ToString().Contains("Queued")).ToString());
                        if (dr[1].ToString().Contains("Queued") && NumberOfTasksToProgress > 0)
                        {
                            //MessageBox.Show("dr[1].ToString: " + dr[1].ToString());
                            task[NumberOfTasksToProgress - 1] = dr[1].ToString();
                            //MessageBox.Show("task[NumberOfTasksToProgress - 1]: " + task[NumberOfTasksToProgress - 1]);
                            NumberOfTasksToProgress--;
                            //MessageBox.Show("task: " + task);
                            return task;
                        }

                    }
                }
            }
            return task;
        }

        private static int HowManyTasksIsQueuedNow()
        {
            string query = "Select Task_name from `pinokkio`.`tasks`";
            DataSet data = makeQuery(query);
            int numberOfTasksIsQueued = 0;
            foreach (DataTable dt in data.Tables)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr[0].ToString().Contains("Queued"))
                    {
                        numberOfTasksIsQueued++;
                    }

                }
            }

            return numberOfTasksIsQueued;
        }

        private static int HowManyTasksIsProgressingNow()
        {
            string query = "Select Task_name from `pinokkio`.`tasks`";
            DataSet data = makeQuery(query);
            int numberOfTasksInProgress = 0;
            foreach (DataTable dt in data.Tables)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr[0].ToString().Contains("progress"))
                    {
                        numberOfTasksInProgress++;
                    }

                }
            }
            
            return numberOfTasksInProgress;

        }

        public List<int> task_sorted_list()
        {
            List<int> id_data_int = new List<int>();
            string command_for_selection = "SELECT Task_name FROM pinokkio.tasks;";
            MySqlConnection Conn = new MySqlConnection(connectionString);
            MySqlCommand Comm_add = new MySqlCommand(command_for_selection, Conn);
            Conn.Open();
            MySqlDataReader DR_loca = Comm_add.ExecuteReader();
            while (DR_loca.Read())
            {
                string[] slova1 = DR_loca[0].ToString().Split('_');     //  slova1 = Task_1234 - ....................] - Queued
                string[] slova2 = slova1[1].Split(' ');
                id_data_int.Add(int.Parse(slova2[0]));
            }
            id_data_int.Sort();
            return id_data_int;
        }
        // read claims table                                           <--- CLAIMS
        private static string GetTask()
        {
            string temp = "";
            try
            {
                string query = "select Task_claim from `pinokkio`.`claims` limit 1";
                MySqlConnection Conn = new MySqlConnection(connectionString);
                MySqlCommand Comm_add = new MySqlCommand(query, Conn);
                Conn.Open();
                MySqlDataReader DR_loca = Comm_add.ExecuteReader();
                string id_data = "";
                if (DR_loca.Read())
                {
                    id_data = DR_loca[0].ToString(); // отримали перше число зі списку
                }
                if (id_data.Length == 0)
                {
                    return "do_nothing";
                }
                else
                {
                    temp = makeQuery(query).Tables[0].Rows[0][0].ToString();
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return temp;
        }

        public static void AddNewTaskInTasks(string task)
        {
            string query = "INSERT INTO `pinokkio`.`tasks` (ID, Task_name) VALUES (null, '" + task + " - [....................]-Queued')"; // INSERT INTO `pinokkio`.`tasks` (ID, Task_name) VALUES (null,'" + "Task_" + id_data + " - [....................]-Queued');
            makeQuery(query);
        }

        public static void MakeTaskInProgress(string task)
        {
            //MessageBox.Show((task == "_NONE").ToString());
            if (task != "_NONE")
            {
                if (task == null)
                {
                    return;
                }
                else
                {
                    string query = "update `pinokkio`.`tasks` set Task_name ='" + task.Substring(0, 9) + " - [....................]-In progress 0%' WHERE Task_name = '" + task + "'";  //  MakeTaskInProgress receives nothing
                    makeQuery(query);
                }
            }
            else
            {
                return;
            }
        }

        public static void DeleteTaskFromClaims(string task)
        {
            string query = "delete from `pinokkio`.`claims` WHERE Task_Claim RLIKE '" + task + "'";
            makeQuery(query);
            CHECK_data();
        }

        public static DataSet makeQuery(string query)
        {
            DataSet data = new DataSet();
            using (MySqlConnection conn = new MySqlConnection(@"Server=localhost;Database=pinokkio;Uid=root;Pwd=PWDQWERTY03"))
            {
                conn.Open();
                MySqlDataAdapter ad = new MySqlDataAdapter(query, connectionString);
                ad.Fill(data);
            }

            return data;
        }
    }
}
