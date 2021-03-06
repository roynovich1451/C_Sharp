using CustomExtensions;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace HW1wpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ObservableCollection<Author> Authors;
        private ObservableCollection<Book> Books;
        private const int NOTFOUND = -1;

        #region buttons_fanctionality
        /// <summary>
        /// Handle add book button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!allTextBoxFilled(gBookDetails))
            {
                MessageBox.Show("You must fill all textboxes under Book details", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                Author a = new Author(tbAFN.Text, tbALN.Text, 1);
                int index = Authors.IndexOf(a);
                if (index == NOTFOUND)
                {
                    Authors.Add(a);
                }
                else
                {
                    a = Authors.ElementAt(index);
                    a.Published += 1;
                }
                Book b = new Book(tbIsbn.Text, tbTitle.Text, a, int.Parse(tbNOC.Text), decimal.Parse(tbPrice.Text));
                if (Books.Contains(b))
                {
                    throw new IsbnInUseException("The book ISBN, " + b.Isbn + ", is in use at library");
                }
                else
                {
                    Books.Add(b);
                    Books = new ObservableCollection<Book>(Books.OrderBy(book => book));
                    lbBooks.ItemsSource = Books;
                }
                rtbHistory.AppendText("Added " + b.Name, "Green");
                clearBooksTextBoxes(gBookDetails);
                lbBooks.SelectedIndex = lbBooks.Items.Count - 1;
            }
            catch (WrongIsbnException wie)
            {
                MessageBox.Show(wie.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (ArgumentException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        /// <summary>
        /// Handle delete selected book button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (lbBooks.SelectedItem == null)
            {
                MessageBox.Show("You must select a Book in order to delete", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Book book2del = lbBooks.SelectedItem as Book;
            MessageBoxResult res = MessageBox.Show("You're about to delete " + book2del.Name + " are you sure?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (res == MessageBoxResult.Cancel)
            {
                return;
            }
            try
            {
                Author auth = book2del.Auth;
                int pub = auth.Published - 1;
                if (pub > 0)
                {
                    auth.Published = pub;
                }
                else
                {
                    Authors.Remove(auth);
                }
                rtbHistory.AppendText("Deleted " + book2del.Name, "Red");
                clearBooksTextBoxes(gBookDetails);
                Books.Remove(book2del);
                if (lbBooks.Items.Count == 0)
                {
                    clearBooksTextBoxes(gDisBooks);
                }
                else
                {
                    lbBooks.SelectedIndex = lbBooks.Items.Count - 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        /// <summary>
        /// Handle book copy amount change button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpAm_Click(object sender, RoutedEventArgs e)
        {
            if (lbBooks.SelectedItem == null)
            {
                MessageBox.Show("You must select a Book to change number of copies", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                Book book2change = lbBooks.SelectedItem as Book;
                if (!int.TryParse(tbNOC.Text, out int tmp) || string.IsNullOrEmpty(tbNOC.Text))
                {
                    throw new ArgumentException("You must give natural number at 'copies' textbox");
                }
                int value = int.Parse(tbNOC.Text);
                int oldAmount = Books[Books.IndexOf(book2change)].Copies;
                Books[Books.IndexOf(book2change)].Copies = value;
                tbDisCopies.Text = tbNOC.Text;
                rtbHistory.AppendText("Changed amount of " + book2change.Name + " from " + oldAmount + " to " + tbNOC.Text, "Blue");
                clearBooksTextBoxes(gBookDetails);
            }
            catch (ArgumentException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        /// <summary>
        /// Handle book price change button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpPr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lbBooks.SelectedItem == null)
                {
                    MessageBox.Show("You must select a Book to change his price", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Book book2change = lbBooks.SelectedItem as Book;
                if (!decimal.TryParse(tbPrice.Text, out decimal tmp) || string.IsNullOrEmpty(tbPrice.Text))
                {
                    throw new ArgumentException("You must give natural number at 'copies' textbox");
                }
                if (book2change.Price < tmp)
                {
                    throw new ArgumentException("Book's new Price can only be lower then his old one");
                }
                decimal oldPrice = Books[Books.IndexOf(book2change)].Price;
                Books[Books.IndexOf(book2change)].Price = tmp;
                tbDisPrice.Text = tbPrice.Text;
                rtbHistory.AppendText("Changed price of " + book2change.Name + " to " + tbPrice.Text, "Blue");
                clearBooksTextBoxes(gBookDetails);
            }
            catch (ArgumentException ae)
            {
                MessageBox.Show(ae.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        #endregion buttons_fanctionality

        #region side_Functions
        /// <summary>
        /// check if all text boxes inside given Panel are filled
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>

        private bool allTextBoxFilled(Panel p)
        {
            foreach (var item in p.Children)
            {
                if (item is TextBox)
                {
                    TextBox tb = item as TextBox;
                    if (tb.IsEnabled == true && string.IsNullOrEmpty(tb.Text))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// clear all text boxes inside given Panel
        /// </summary>
        /// <param name="p"></param>
        private void clearBooksTextBoxes(Panel p)
        {
            foreach (var item in p.Children)
            {
                if (item is TextBox)
                {
                    TextBox tb = item as TextBox;
                    tb.Text = null;
                }
            }
        }
        /// <summary>
        /// Handle selection change even at list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbBooks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Book selected = lbBooks.SelectedItem as Book;
            if (selected == null) return;
            Author auth = selected.Auth as Author;
            tbDisAuthor.Text = auth.FirstName + " " + auth.LastName;
            tbDisAuthorBooks.Text = auth.Published.ToString();
            tbDisCopies.Text = selected.Copies.ToString();
            tbDisPrice.Text = selected.Price.ToString();
        }

        #endregion side_Functions

        #region windowFunc

        private static string fileData = "appData.xml";
        /// <summary>
        /// Handle application closing event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Books.Count > 0)
            {
                using (StreamWriter handle = new StreamWriter("filePath"))
                {
                    handle.Write(fileData);
                }
                saveDataToFile();
            }
            else
            {
                File.Delete(fileData);
            }
        }
        /// <summary>
        /// Save application data at output XML file
        /// </summary>
        private void saveDataToFile()
        {
            XmlTextWriter writer = new XmlTextWriter(fileData, Encoding.Unicode);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("DATA");
            writer.WriteStartElement("Authors");
            foreach (var auth in Authors)
            {
                writer.WriteStartElement("Author");
                writer.WriteElementString("FN", auth.FirstName);
                writer.WriteElementString("LN", auth.LastName);
                writer.WriteElementString("PUB", auth.Published.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("Books");
            foreach (var book in Books)
            {
                writer.WriteStartElement("Book");
                writer.WriteElementString("ISBN", book.Isbn);
                writer.WriteElementString("Title", book.Name);
                writer.WriteElementString("AFN", book.Auth.FirstName);
                writer.WriteElementString("ALN", book.Auth.LastName);
                writer.WriteElementString("Copies", book.Copies.ToString());
                writer.WriteElementString("Price", book.Price.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }
        /// <summary>
        /// Handle load data from file at application initialization process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Initialized(object sender, EventArgs e)
        {
            Authors = new ObservableCollection<Author>();
            Books = new ObservableCollection<Book>();
            lbBooks.ItemsSource = Books;
            if (File.Exists("filePath"))
            {
                using (StreamReader handle = new StreamReader("filePath"))
                {
                    fileData = handle.ReadToEnd();
                }
                if (File.Exists(fileData))
                {
                    Console.WriteLine("exist");
                    readDataFromXml();
                }
            }
        }
        /// <summary>
        /// Get data from XML file
        /// </summary>
        private void readDataFromXml()
        {
            XmlTextReader reader = new XmlTextReader(fileData);
            reader.WhitespaceHandling = WhitespaceHandling.None;
            while (reader.Name != "DATA")
            {
                reader.Read();
            }
            while (reader.Name != "Authors")
            {
                reader.Read();
            }
            reader.Read();
            while (reader.Name == "Author")
            {
                Author a;

                reader.ReadStartElement("Author");
                string afn = reader.ReadElementString("FN");
                string aln = reader.ReadElementString("LN");
                int ap = int.Parse(reader.ReadElementString("PUB"));
                a = new Author(afn, aln, ap);
                Authors.Add(a);
                reader.ReadEndElement();
            }
            while (reader.Name != "Books")
            {
                reader.Read();
            }
            reader.Read();
            while (reader.Name == "Book")
            {
                Book b;
                Author tmp;

                reader.ReadStartElement("Book");
                string isbn = reader.ReadElementString("ISBN");
                string title = reader.ReadElementString("Title");
                string afn = reader.ReadElementString("AFN");
                string aln = reader.ReadElementString("ALN");
                int copies = int.Parse(reader.ReadElementString("Copies"));
                decimal price = decimal.Parse(reader.ReadElementString("Price"));
                tmp = new Author(afn, aln, 0);
                b = new Book(isbn, title, Authors[Authors.IndexOf(tmp)], copies, price);
                Books.Add(b);
                reader.ReadEndElement();
            }
            reader.Close();
        }

        #endregion windowFunc
    }
}