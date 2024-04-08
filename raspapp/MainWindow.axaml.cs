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

using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualBasic;
using System.Device.Gpio;
using System.Runtime.CompilerServices;

namespace raspapp
{
    public partial class MainWindow : Window
    {

        private readonly TextBlock Clock;
        private readonly TextBox InputBox;
        private readonly TextBlock ElevNamn;
        private readonly TextBlock Klass;
        private readonly TextBlock Tid;
        private readonly TextBlock Varv;
        readonly TextBlock TaggID;

        string taggId;
        Dictionary<string, Student> students = new ();

        private Dictionary<string, DateTime> lastTaggedTime = new Dictionary<string, DateTime>();
        private TimeSpan restrictionPeriod = TimeSpan.FromSeconds(5);

        private const int ShutdownPin = 14; // GPIO pin number for the shutdown button
        private const int ShutdownDuration = 5000; // Shutdown duration in milliseconds (5 seconds)
        private GpioController gpioController;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGpioController();

            this.WindowState = WindowState.FullScreen;

            Clock = this.FindControl<TextBlock>("clock");

            // Update the current time every second
            var timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, UpdateCurrentTime);
            timer.Start();
        

            InputBox = this.FindControl<TextBox>("names");
            Tid = this.FindControl<TextBlock>("tid");
            Varv = this.FindControl<TextBlock>("varv");
            TaggID = this.FindControl<TextBlock>("taggID");

            InputBox.KeyUp += TextBox_KeyUp;
            InputBox.Focus();

            this.AttachedToVisualTree += (sender, e) => InputBox.Focus();
            this.KeyDown += Window_KeyDown;
        }




        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            bool doSave = true;

            if (e.Key == Key.Enter)
            {
                // User pressed Enter, process the entered line
                var current = InputBox.Text + Environment.NewLine;
                InputBox.Text = current + taggId + " : " + DateTime.Now.ToString("HH:mm:ss");                      

                if (students.ContainsKey(taggId))  // This tagg has been tagged before
                {
                    Student student = students[taggId];

                    // Check if this student tagged recently
                    if (TaggedRecently(taggId))
                    {
                        Varv.Text = "Can't tag";
                        Tid.Text = "again";
                        TaggID.Text = "so soon!";
                        doSave = false;
                    }
                    else
                    {
                        
                        students[taggId].Laps++;
                        Varv.Text = "Varv " + students[taggId].Laps.ToString();
                    }
                }
                else   // It's a new taggId
                {
                    students.Add(taggId, new Student() {TaggID = taggId, Laps = 1});

                    Varv.Text = "Varv " + students[taggId].Laps.ToString();
                }

                if (doSave)
                {
                    // Get the last 5 characters of TaggID
                    Student taggedStudent = students[taggId];
                    string lastFiveCharacters = taggedStudent.TaggID.Substring(Math.Max(0, taggedStudent.TaggID.Length - 5));

                    // Write the students info in the boxes
                    TaggID.Text = lastFiveCharacters;
                    Tid.Text = DateTime.Now.ToString("HH:mm:ss");

                    students[taggId].Times.Add(Tid.Text);

                    SaveStudents(students);
                }

                // Update last tagging time for this student
                lastTaggedTime[taggId] = DateTime.Now;

                taggId = ""; // Reset taggId for the next entry

                return;
            }
            taggId += e.KeySymbol;
        }

        private bool TaggedRecently(string taggId)
        {
            // Check if the student has tagged before
            if (lastTaggedTime.ContainsKey(taggId))
            {
                // Calculate the time difference since the last tagging
                TimeSpan timeSinceLastTagging = DateTime.Now - lastTaggedTime[taggId];

                // Check if the time difference is less than the restriction period
                if (timeSinceLastTagging < restrictionPeriod)
                {
                    return true;
                }
            }

            return false;
        }



        private void UpdateCurrentTime(object sender, EventArgs e)
        {
            InputBox.Focus();
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

        
        
        void SaveStudents(Dictionary<string, Student> students)
        {
            // Specify the path to the output CSV file
            string filePath = "./Resultat.csv";

            // Open a StreamWriter to write to the file
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Write the header line
                writer.WriteLine("TagId;Laps;Date;Times");


                // Go through each value of the students in the dictionary
                foreach (var kvp in students)
                {
                    // Get the student object
                    var student = kvp.Value;

                    // Concatenate the list of times into a single string
                    string times = string.Join(";", student.Times);

                    // Write the student information as a line in the CSV file
                    string studentsInfo = $"{student.TaggID};{student.Laps};{DateTime.Now.ToShortDateString()};{times}";

                    writer.WriteLine(studentsInfo);
                    Debug.WriteLine(studentsInfo);
                }
            }
        }

 

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Close window if Escape is pressed
            if (e.Key == Key.Escape)
            {
                this.WindowState = WindowState.Normal;

                this.Close();
            }
        }


        
        private void InitializeGpioController()
        {
            // Initialize GPIO controller
            //gpioController = new GpioController();

            //// Set GPIO pin as input with pull-up resistor enabled
            //gpioController.OpenPin(ShutdownPin, PinMode.InputPullUp);

            //// Attach event handler for pin value changes
            //gpioController.RegisterCallbackForPinValueChangedEvent(ShutdownPin, PinEventTypes.Rising | PinEventTypes.Falling, HandleButtonPress);
        }

        DateTime lastClick = DateTime.MinValue;
        private void HandleButtonPress(object sender, PinValueChangedEventArgs e)
        {

            var timeSinceLastClick = DateTime.Now - lastClick;
            lastClick = DateTime.Now;

            if (timeSinceLastClick.TotalSeconds > 3 && timeSinceLastClick.TotalSeconds < 8)
            {
                if (e.ChangeType == PinEventTypes.Rising)
                {
                    Dispatcher.UIThread.InvokeAsync(() => Clock.Text = $"Shutdown");
                    Process.Start("sudo", "shutdown -h now");
                }
            }
        }
        


    }
}