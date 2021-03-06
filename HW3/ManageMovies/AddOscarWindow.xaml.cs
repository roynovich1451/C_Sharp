﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ManageMovies
{
    /// <summary>
    /// Interaction logic for AddOscarWindow.xaml
    /// </summary>
    /// 
    public partial class AddOscarWindow : Window
    {
        int oscarYear = -1;
        public AddOscarWindow()
        {
            InitializeComponent();
            updateComboBoxes();
        }

        private void btnBestActor_Click(object sender, RoutedEventArgs e)
        {
            var addActorWindow = new AddActorWindow();
            addActorWindow.ShowDialog();
            updateComboBoxes();
        }

        private void btnBestDirector_Click(object sender, RoutedEventArgs e)
        {
            var addDirectorWindow = new AddDirectorWindow();
            addDirectorWindow.ShowDialog();
            updateComboBoxes();
        }

        private void btnBestMovie_Click(object sender, RoutedEventArgs e)
        {
            var addMovieWindow = new AddMovieWindow();
            addMovieWindow.ShowDialog();
            updateComboBoxes();
        }

        private void btnAddOscar_Click(object sender, RoutedEventArgs e)
        {
            if (oscarYear == -1)
            {
                MessageBox.Show("Must enter year and click on 'Show year best movies' button", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (missData())
            {
                MessageBox.Show("Must provide all data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Actor selectedActor = cmbBestActor.SelectedItem as Actor;
            Actor selectedActress = cmbBestActress.SelectedItem as Actor;
            Director selectedDirector = cmbBestDirector.SelectedItem as Director;
            Movie selectedMovie = cmbBestMovie.SelectedItem as Movie;
            Oscar newOscar = new Oscar
            {
                Year = oscarYear,
                BestActorId = selectedActor.Id,
                BestActressId = selectedActress.Id,
                BestDirectorId = selectedDirector.Id,
                MovieSerial = selectedMovie.MovieSerial,
            };
            try
            {
                using (var ctx = new dbContext())
                {
                    ctx.Oscars.Add(newOscar);
                    ctx.SaveChanges();
                }
                MessageBox.Show($"Oscar winners in {newOscar.Year}!\n" +
                $"Best movie: {selectedMovie.Title}\n" +
                $"Best Actor: {selectedActor.FirstName} {selectedActor.LastName}\n" +
                $"Best Actress: {selectedActress.FirstName} {selectedActress.LastName}\n" +
                $"Best director: {selectedDirector.FirstName} {selectedDirector.LastName}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                emptyTextBoxes();
            }
            catch (FormatException)
            {
                MessageBox.Show("Data is NOT in the correct format", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException.Message.Contains("duplicate key"))
                {
                    MessageBox.Show($"Oscar Id already in DB", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(ex.Message + ", Iner: " + ex.InnerException.Message + "\n" + "Type: " + ex.GetType().ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ", Iner: " + ex.InnerException.Message + "\n" + "Type: " + ex.GetType().ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void btnSetYear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string buttonContent = btnSetYear.Content.ToString();
                if (buttonContent.Equals("Set oscar year"))
                {
                    oscarYear = int.Parse(cmbYear.Text.Trim());
                    using (var ctx = new dbContext())
                    {
                        var allYearMovies = (from m in ctx.Movies
                                             where m.Year == oscarYear
                                             select m).ToList();
                        if (allYearMovies.Count == 0)
                        {
                            MessageBox.Show("No movies available in selected year.\n" +
                                "Please add movie for year or pick diffrent one.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        cmbYear.IsEnabled = false;
                        btnSetYear.Content = "Change year";
                        cmbBestMovie.ItemsSource = allYearMovies;
                        cmbBestActor.Visibility = Visibility.Visible;
                        cmbBestActress.Visibility = Visibility.Visible;
                        cmbBestDirector.Visibility = Visibility.Visible;
                        cmbBestMovie.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    emptyTextBoxes();

                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Must select year from comboBox", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show(ex.Message + ", Iner: " + ex.InnerException.Message + "\n" + "Type: " + ex.GetType().ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ", Iner: " + ex.InnerException.Message + "\n" + "Type: " + ex.GetType().ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        #region helpers
        public void updateComboBoxes()
        {
            try
            {
                using (var ctx = new dbContext())
                {
                    cmbYear.ItemsSource = (from m in ctx.Movies
                                           select m.Year).ToList().Distinct().ToList();
                    cmbBestActor.ItemsSource = (from a in ctx.Actors
                                                where a.Gender == 1
                                                select a).ToList();
                    cmbBestActress.ItemsSource = (from a in ctx.Actors
                                                  where a.Gender == 0
                                                  select a).ToList();
                    cmbBestDirector.ItemsSource = (from d in ctx.Directors
                                                   select d).ToList();
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Data is NOT in the correct format", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException.Message.Contains("duplicate key"))
                {
                    MessageBox.Show("Your pick already in DB", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(ex.Message + ", Iner: " + ex.InnerException.Message + "\n" + "Type: " + ex.GetType().ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ", Iner: " + ex.InnerException.Message + "\n" + "Type: " + ex.GetType().ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool missData()
        {
            return cmbBestActor.SelectedItem == null ||
                cmbBestActress.SelectedItem == null ||
                cmbBestDirector.SelectedItem == null ||
                cmbBestMovie.SelectedItem == null ||
                string.IsNullOrEmpty(cmbYear.Text);
        }

        private void emptyTextBoxes()
        {
            btnSetYear.Content = "Set oscar year";
            cmbYear.IsEnabled = true;
            cmbBestActor.Visibility = Visibility.Hidden;
            cmbBestActress.Visibility = Visibility.Hidden;
            cmbBestDirector.Visibility = Visibility.Hidden;
            cmbBestMovie.Visibility = Visibility.Hidden;
            cmbBestActor.SelectedIndex = -1;
            cmbBestActress.SelectedIndex = -1;
            cmbBestDirector.SelectedIndex = -1;
            cmbBestMovie.SelectedIndex = -1;
            cmbYear.SelectedIndex = -1;
            oscarYear = -1;
        }
        #endregion
    }
}
