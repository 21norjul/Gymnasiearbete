using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        Dictionary<string, Resultat> resultat = new();

        int IdValue = 0;
        int antalVarv = 1;

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

            // Subscribe to the KeyUp event to handle keyboard input
            Names.KeyUp += TextBox_KeyUp;
            Names.Focus();

            this.AttachedToVisualTree += (sender, e) => Names.Focus();
        }




        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                /*array.Add("hej");*/
                // User pressed Enter, process the entered line
                var current = Names.Text + Environment.NewLine;
                Names.Text = current + taggId + "  No Name  " + DateTime.Now.ToString("HH:mm:ss");


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


                //int person = students[taggId];

                //foreach (var kvp in students)
                //{
                //    Debug.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}   Antal varv: {antalVarv}  Varvlist: {varvList[taggId]}");

                //}

                //person++;

                ElevNamn.Text = students[taggId].FName + " " + students[taggId].EName;
                Klass.Text = students[taggId].Class;
                Tid.Text = DateTime.Now.ToString("HH:mm:ss");


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
            string[] lines = File.ReadAllLines(filePath);

            // Iterate through each line and split the values by semicolon
            for (int i = 0; i < lines.Length; i++)
            {
                // Split the line by semicolon and store the values in the array
                string[] lineValues = lines[i].Split(';');

                var s = new Student { TaggID = lineValues[0], FName = lineValues[1], EName = lineValues[2], Class = lineValues[3] };
                students.Add(s.TaggID, s);
            }

        }



        static void SaveStudents(Dictionary<string, Student> students)
        {
            // Specify the path to the output CSV file
            string filePath = "ResultExample.csv";

            // Open a StreamWriter to write to the file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write the header line
                writer.WriteLine("TagId;FName;EName;Class");

                // Iterate over each student in the dictionary
                foreach (var kvp in students)
                {
                    // Get the student object
                    var student = kvp.Value;

                    // Write the student information as a line in the CSV file
                    writer.WriteLine($"{student.TaggID},{student.FName},{student.EName},{student.Class}");
                }
            }

            Debug.WriteLine("CSV file saved successfully.");
        }

        /*

        static void SaveResult(Dictionary<string, Resultat> resultat)
        {
            string filePath = "ResultatExempel.csv";

            var r = new Resultat { TaggID =  };
            resultat.Add()
        }*/

    }

}