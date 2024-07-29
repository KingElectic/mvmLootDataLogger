using System.Collections;
using System.IO;
using System.Net.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mvmLootDataLogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    // main/window
    public partial class MainWindow : Window
    {
        // inst vars
        // file writer/reader
        private StreamWriter writer;
        private StreamReader reader;
        // ArrayList of mvmLoot objects
        ArrayList lootList = new ArrayList();

        // arrays containing dropdown info
        // tours
        private string[] tours = { "Two Cities", "Mecha Engine", "Gear Grinder" };
        // aussies
        private string[] aussies = { "Scattergun", "Force-A-Nature",                         // scout
                                     "Rocket Launcher", "Black Box",                         // soldier
                                     "Flame Thrower", "Axtinguisher",                        // pyro
                                     "Grenade Launcher", "Stickybomb Launcher", "Eyelander", // demo
                                     "Minigun", "Tomislav",                                  // heavy
                                     "Frontier Justice", "Wrench",                           // engie
                                     "Blutsauger", "Medi Gun",                               // medic
                                     "Sniper Rifle", "SMG",                                  // sniper
                                     "Ambassador", "Knife",                                  // spy
                                     "Golden Pan", };                                        // all class

        public MainWindow()
        {
            InitializeComponent();

            // add items to combo boxes
            // tours
            foreach (var t in tours)
                tourInput.Items.Add(t);
            // aussies
            foreach (var a in aussies)
                australiumInput.Items.Add(a);

            // populate lootList from reader
            splitInputIntoLootList(readFile());
            FileStream fs = new FileStream("mvm_loot_data.txt", FileMode.Append, FileAccess.Write);
            // set up writer
            writer = new StreamWriter(fs);
        }
        
        // ##
        // ## BUTTONS
        // ##

        // submit button
        private void submit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!tourInput.Text.Equals(string.Empty))
                {
                    // temp insterts for incoming data                      
                    string tr = tourInput.Text.Replace(" ", "_");
                    int trn = Math.Abs(Int32.Parse(tourNumberInput.Text));
                    string aus = australiumInput.Text.Replace(" ", "_");
                    bool fb = (bool)is4boxCheckbox.IsChecked;

                    // verify tour# is > 0
                    if (trn > 0)
                    {
                        mvmLoot temp = new mvmLoot(tr, trn, aus, fb); // create temp mvmLoot object
                        lootList.Add(temp); // add input to list
                        writeToFile(temp); // give temp to file writer
                    }
                    else
                    {
                        MessageBox.Show("Tour number must be > 0");
                    }

                    // clear inputs
                    tourNumberInput.Text = string.Empty; // tour#
                    is4boxCheckbox.IsChecked = false;    // is4box
                }
            }
            catch
            {
                MessageBox.Show("An error has occurred in submitting input to file."); // probably a non number in the tour number input
            }
        } // end submit button click

        // run format for Two Cities
        private void calc2C_Click(object sender, RoutedEventArgs e)
        {
            // clear output
            outputTB.Text = string.Empty;

            // format for 2c
            formatFromTour("Two_Cities");
        }

        // run format for Mecha Engine
        private void calcME_Click(object sender, RoutedEventArgs e)
        {
            // clear output
            outputTB.Text = string.Empty;

            // format for me
            formatFromTour("Mecha_Engine");
        }

        // run format for Gear Grinder
        private void calcGG_Click(object sender, RoutedEventArgs e)
        {
            // clear output
            outputTB.Text = string.Empty;

            // format for gg
            formatFromTour("Gear_Grinder");
        }

        // run drop count and calc/format return
        private void calcDC_Click(object sender, RoutedEventArgs e)
        {
            // clear output
            outputTB.Text = string.Empty;
            // temp var holder for return vals
            int[] r = calcDropCount("All");
            // format for dc
            outputTB.Text += $"Australiums: {r[0]}\nGolden Pans: {r[1]}";
        }

        // full print
        private void fullPrint_Click(object sender, RoutedEventArgs e)
        {


            // var set up

            // temp var holder for return vals
            int[] r = calcDropCount("All");

            // print

            // clear output
            outputTB.Text = string.Empty;

            outputTB.Text += $"[b]Total drop count:[/b]\n\n";

            outputTB.Text += $"Australiums: {r[0]}\n" +
                             $"Golden Pans: {r[1]}\n\n";

            outputTB.Text += $"[b]Drop list/tours:[/b]\n\n";

            formatFromTour("Two_Cities");   // format for 2c
            
            formatFromTour("Mecha_Engine"); // format for me

            formatFromTour("Gear_Grinder"); // format for gg
        }

        // COPY TO CLIPBOARD
        private void copyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // clear clipboard before hand. this stop an exepeption from throwing. why? idfk.
                Clipboard.Clear(); 

                // set clipboard to current output display
                Clipboard.SetText(outputTB.Text.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Data failed to copy to clipboard.");
            }
        }

        // ##
        // ## FORMAT/CALCULATION
        // ##

        // format from given tour
        public void formatFromTour(string t)
        {
            // number saving into vars
            int auss = calcDropCount(t)[0];
            int pans = calcDropCount(t)[1];

            // display average
            outputTB.Text += $"{t.Replace("_", " ")}: avg. ~{findAverage(t):f2}\n";

            if (auss > 0)
                outputTB.Text += $"[i]Australiums: {auss}[/i]\n";
            if (pans > 0)
                outputTB.Text += $"[i]Golden Pans: {pans}[/i]\n";
            outputTB.Text += "\n";
            if (auss != 0 || pans != 0)
            {
                
                foreach (mvmLoot m in lootList)
                {
                    // check if current m is 2c
                    if (m.getTour().Equals(t))
                    {
                        // main component
                        outputTB.Text += $"• Tour {m.getTourNumber()} Aussie {m.getAustralium().Replace("_", " ")}";

                        // 4box addititon check
                        if (m.is4Box())
                            outputTB.Text += " [i](4 box)[/i]";
                    }

                    if (m.getTour().Equals(t))
                        // newline
                        outputTB.Text += Environment.NewLine;
                }
            }
            else
            {
                outputTB.Text += "[i]n/a[/i]";
                // newline
                outputTB.Text += Environment.NewLine;
            }
            // newline
            outputTB.Text += Environment.NewLine;
        }

        // find average drop from given tour
        public double findAverage(string t)
        {
            int last = 0;
            int count = 0;
            foreach (mvmLoot m in lootList)
            {
                if (m.getTour().Equals(t))
                { 
                    if (m.getTourNumber() > last)
                        last = m.getTourNumber();
                    count++;
                }
            }
            //return ((double)last/count) == double.NaN:0,((double)last / count); // it'd be kinda cool if i could figure out how this works, prolly using it completely wrong though
            // NaN/div by 0 check
            if (count == 0)
                return 0;
            return (double)last / count;
        }
       
        // calculate and format drop count
        public int[] calcDropCount(string t)
        {
            int ausCount = 0;
            int panCount = 0;
            if (t.Equals("All"))
            {
                foreach (mvmLoot m in lootList)
                {
                    ausCount++;
                    if (m.getAustralium().Equals("Golden_Pan")) // note: a pan is an aussie, but added to its own counter as well
                        panCount++;
                }
            }
            else 
            {
                foreach (mvmLoot m in lootList)
                {
                    if (m.getTour().Equals(t))
                    {
                        ausCount++;
                        if (m.getAustralium().Equals("Golden_Pan"))
                            panCount++;
                    }
                }
            }
            return new int[] { ausCount, panCount };
        }

        // split fileInput into array, into lootList
        public void splitInputIntoLootList(string input)
        {
            try
            {
                // split and remove \r\n from input
                string[] temp = input.Split(Environment.NewLine);

                // parse temp into lootList
                // # EXAMPLE READIN SECTION : Two_Cities 77 Frontier_Justice False
                foreach (string s in temp)
                {
                    // make sure that s in not null or empty string
                    if (!s.Equals(""))
                    {
                        // split current temp/s position into own array
                        string[] split = s.Split(" ");
                        // create new vars as correct types (tour# mustr be int, is4box must be bool)
                        // tour #
                        int t = Int32.Parse(split[1]); // parse out the tour count into an int
                        // is4box
                        bool fb = Boolean.Parse(split[3]); // parse out is4box into a bool
                        // rebuild lootList
                        lootList.Add(new mvmLoot(split[0], t, split[2], fb));
                    }
                }
            }
            catch
            {
                MessageBox.Show("An error has occurred in reading from file."); // likely someone messing with file manually (me)
            }
        }

        // ##
        // ## FILE INTERACTION
        // ##

        // writer to file
        public void writeToFile(mvmLoot obj)
        {
            // send to file in format:
            //     {Tour} {Tour Count} {Australium} {is a 4 Box}
            // ex: Two_Cities 77 Frontier_Justice False
            writer.WriteLine($"{obj.getTour()} {obj.getTourNumber()} {obj.getAustralium()} {obj.is4Box()}");
        }

        // read from file
        public string readFile()
        {
            reader = new StreamReader("mvm_loot_data.txt");
            string fileData = reader.ReadToEnd();

            // close reader
            reader?.Close();

            return fileData;
        }

        // when form closes
        private void Window_Closed(object sender, EventArgs e)
        {
            // close writer
            writer?.Close();
        }

        
    } // end main class

    // mvm game loot class
    public class mvmLoot
    {
        // inst vars
        // *note : { get; } wasnt working for some reason >:(
        private string tour;
        private int tourNumber;
        private string australium;
        private bool is4box;

        public mvmLoot(string t, int n, string a, bool f) 
        {
            tour = t; 
            tourNumber = n;
            australium = a;
            is4box = f;
        }

        // getters
        public string getTour() { return tour; }
        public int getTourNumber() { return tourNumber; }
        public string getAustralium() { return australium; }
        public bool is4Box() { return is4box; }

        // toString override
        override
        public string ToString()
        {
            return $"{tour} {tourNumber} {australium} {is4box}";
        }
    } // end mvmLoot class
} // end namespace