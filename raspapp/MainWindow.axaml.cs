using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace raspapp
{
    public partial class MainWindow : Window
    {

        private readonly TextBlock Clock;
        private readonly TextBox Names;
        private readonly TextBlock ElevNamn;
        private readonly TextBlock Klass;
        private readonly TextBlock Tid;
        private readonly TextBlock Varv;

        string taggId;

        Dictionary<string, Student> students = new ();

        int IdValue = 0;
        int antalVarv = 1;
        int laps = 0;

        public MainWindow()
        {
            InitializeComponent();

            Clock = this.FindControl<TextBlock>("clock");

            // Update the current time every second
            var timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, UpdateCurrentTime);
            timer.Start();
            //timer.Elapsed += (sender, e) => UpdateCurrentTime();
            //timer.Start();

            LoadStudents(students);

            Names = this.FindControl<TextBox>("names");
            ElevNamn = this.FindControl<TextBlock>("elevNamn");
            Klass = this.FindControl<TextBlock>("klass");
            Tid = this.FindControl<TextBlock>("tid");
            Varv = this.FindControl<TextBlock>("varv");

            Names.KeyUp += TextBox_KeyUp;
            Names.Focus();

            this.AttachedToVisualTree += (sender, e) => Names.Focus();
        }




        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // User pressed Enter, process the entered line
                var current = Names.Text + Environment.NewLine;
                Names.Text = current + taggId + "Namn" + DateTime.Now.ToString("HH:mm:ss");                      

                if (students.ContainsKey(taggId))  // This tagg has been tagged before
                {
                    students[taggId].Laps++;

                    Varv.Text = "Varv " + students[taggId].Laps.ToString();
                }
                else   // It's a new taggId
                {
                    students.Add(taggId, new Student() {TaggID = taggId, Laps = 1});

                    Varv.Text = "Varv " + students[taggId].Laps.ToString();
                }

                ElevNamn.Text = students[taggId].FName + " " + students[taggId].EName;
                Klass.Text = students[taggId].Class;
                Tid.Text = DateTime.Now.ToString("HH:mm:ss");

                students[taggId].Times.Add(Tid.Text);

                SaveStudents(students);
                   
                taggId = ""; // Reset taggId for the next entry

                return;
            }
            taggId += e.KeySymbol;
        }


        private void UpdateCurrentTime(object sender, EventArgs e)
        {
            Names.Focus();
            // Update the current time property
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");

            // Update the TextBox on the UI thread
            Dispatcher.UIThread.InvokeAsync(() => Clock.Text = $"{CurrentTime}");
        }

        public string CurrentTime
        {
            get { return GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }

        public static readonly StyledProperty<string> CurrentTimeProperty =
        AvaloniaProperty.Register<MainWindow, string>(nameof(CurrentTime));

        


        static void LoadStudents(Dictionary<string, Student> students)
        {
            // Specify the path to your text file
            string filePath = "EleverExempel.csv";

            // Read all lines from the text file
            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

            // Go through each line and split the values by semicolon
            for (int i = 0; i < lines.Length; i++)
            {
                // Split the line by semicolon and store the values in the array
                string[] lineValues = lines[i].Split(';');

                var s = new Student { TaggID = lineValues[0], FName = lineValues[1], EName = lineValues[2], Class = lineValues[3] };
                students.Add(s.TaggID, s);
            }

        }



        void SaveStudents(Dictionary<string, Student> students)
        {
            // Specify the path to the output CSV file
            string filePath = "./Resultat.csv";

            // Open a StreamWriter to write to the file
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Write the header line
                writer.WriteLine("TagId;FName;EName;Class;Laps;Date;Time");


                // Go through each value of the students in the dictionary
                foreach (var kvp in students.Skip(1))
                {
                    // Get the student object
                    var student = kvp.Value;

                    // Concatenate the list of times into a single string
                    string times = string.Join(";", student.Times);

                    // Write the student information as a line in the CSV file
                    string studentsInfo = $"{student.TaggID};{student.FName};{student.EName};{student.Class};{student.Laps};{DateTime.Now.ToShortDateString()};{times}";

                    writer.WriteLine(studentsInfo);
                    Debug.WriteLine(studentsInfo);
                }
            }
        }



    }
}